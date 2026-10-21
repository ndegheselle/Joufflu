using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Navigation.Controls
{
    public partial class PagingSelectionItem : ObservableObject
    {
        public bool IsActive { get; set; }
        public int Target { get; set; }

        public PagingSelectionItem(int target, int actualPage)
        {
            Target = target;
            IsActive = Target == actualPage;
        }
    }

    public partial class PagingSelectionSeparator : PagingSelectionItem
    {
        public PagingSelectionSeparator() : base(-1, 0) { }
    }

    public partial class Paging : Control, INotifyPropertyChanged
    {
        static Paging()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Paging),
                new FrameworkPropertyMetadata(typeof(Paging)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public delegate void HandlePagingChange(int pageNumber, int capacity);
        public event HandlePagingChange? PagingChange;

        #region DependencyProperties
        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register(
                nameof(Total),
                typeof(int),
                typeof(Paging),
                new PropertyMetadata(-1, (o, value) => ((Paging)o).OnTotalChanged()));

        public static readonly DependencyProperty PageNumberProperty =
            DependencyProperty.Register(
                nameof(PageNumber),
                typeof(int),
                typeof(Paging),
                new PropertyMetadata(1, (o, value) => ((Paging)o).OnPageNumberChange(), (o, value) => ((Paging)o).CoercePageNumber((int)value)));

        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register(
                nameof(Capacity),
                typeof(int),
                typeof(Paging),
                new PropertyMetadata(10, (o, value) => ((Paging)o).OnCapacityChanged()));
        #endregion

        #region Properties
        public int Total { get { return (int)GetValue(TotalProperty); } set { SetValue(TotalProperty, value); } }
        public int PageNumber { get { return (int)GetValue(PageNumberProperty); } set { SetValue(PageNumberProperty, value); } }
        public int Capacity { get { return (int)GetValue(CapacityProperty); } set { SetValue(CapacityProperty, value); } }

        public List<int> AvailableCapacities { get; set; } = new List<int>() { 5, 10, 25, 50, 100, 200 };
        public ObservableCollection<PagingSelectionItem> AvailablePages { get; set; } = [];

        private PagingState State => new(Total, Capacity, PageNumber);

        /// <summary>
        /// Whether the size of the set is known : a negative <see cref="Total"/> leaves it unknown, while 0 is an
        /// empty set.
        /// </summary>
        public bool IsTotalKnown => State.IsTotalKnown;
        public int PageMax => State.PageMax;
        public int IntervalMin => State.IntervalMin;
        public int IntervalMax => State.IntervalMax;
        #endregion

        public Paging()
        {
        }

        #region Change Events
        private void OnTotalChanged()
        {
            // A page the new total no longer reaches is clamped, which goes through OnPageNumberChange.
            CoerceValue(PageNumberProperty);
            Refresh();
        }

        private int CoercePageNumber(int value) => State.Clamp(value);

        private void OnPageNumberChange()
        {
            Refresh();
            PagingChange?.Invoke(PageNumber, Capacity);
        }

        private void OnCapacityChanged()
        {
            int previous = PageNumber;
            CoerceValue(PageNumberProperty);
            // A clamped page already went through OnPageNumberChange : refreshing and raising again would repeat it.
            if (previous != PageNumber)
                return;

            Refresh();
            PagingChange?.Invoke(PageNumber, Capacity);
        }

        /// <summary>
        /// Shows the current state : the page buttons, the interval and whether Previous and Next are available.
        /// </summary>
        private void Refresh()
        {
            UpdateAvailablePages();
            NotifyPropertyChanged(nameof(IsTotalKnown));
            NotifyPropertyChanged(nameof(PageMax));
            NotifyPropertyChanged(nameof(IntervalMin));
            NotifyPropertyChanged(nameof(IntervalMax));
            PreviousCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }
        #endregion

        private void UpdateAvailablePages()
        {
            AvailablePages.Clear();
            foreach (int? page in State.VisiblePages())
            {
                PagingSelectionItem item = page is int target
                    ? new PagingSelectionItem(target, PageNumber)
                    : new PagingSelectionSeparator();
                AvailablePages.Add(item);
            }
        }

        #region Commands
        [RelayCommand()]
        private void GotTo(int pageNumber)
        {
            // A click toggles the page button off ; on the current page nothing changes, so the buttons are built
            // again to show it checked.
            if (pageNumber == PageNumber)
                UpdateAvailablePages();
            else
                PageNumber = pageNumber;
        }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private void Previous() { PageNumber -= 1; }

        [RelayCommand(CanExecute = nameof(CanGoForward))]
        private void Next() { PageNumber += 1; }

        private bool CanGoBack() => State.CanGoBack;
        private bool CanGoForward() => State.CanGoForward;
        #endregion
    }
}