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
                new PropertyMetadata(1, (o, value) => ((Paging)o).OnPageNumberChange()));

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

        public int PageMax
        {
            get
            {
                if (Total <= 0)
                    return int.MaxValue;
                int max = (int)Math.Ceiling(Total / (double)Capacity);
                return Math.Max(1, max);
            }
        }

        public int IntervalMin { get { return Capacity * (PageNumber - 1) + 1; } }

        public int IntervalMax
        {
            get
            {
                if (IntervalMin + Capacity > Total)
                    return Total;
                else
                    return IntervalMin + Capacity - 1;
            }
        }
        #endregion

        public Paging()
        {
        }

        #region Change Events
        private void OnTotalChanged()
        {
            UpdateAvailablesPages();
            NotifyPropertyChanged(nameof(PageMax));
            NotifyPropertyChanged(nameof(IntervalMin));
            NotifyPropertyChanged(nameof(IntervalMax));
            RaiseCommandsChanged();
        }

        private void OnPageNumberChange()
        {
            int value = (int)GetValue(PageNumberProperty);
            if (value > PageMax)
                value = PageMax;
            if (value < 1)
                value = 1;

            UpdateAvailablesPages();
            SetValue(PageNumberProperty, value);

            PagingChange?.Invoke(PageNumber, Capacity);
            NotifyPropertyChanged(nameof(IntervalMin));
            NotifyPropertyChanged(nameof(IntervalMax));
            RaiseCommandsChanged();
        }

        private void OnCapacityChanged()
        {
            if (PageNumber > PageMax && PageMax != 0)
                PageNumber = PageMax;

            PagingChange?.Invoke(PageNumber, Capacity);
            NotifyPropertyChanged(nameof(PageMax));
            NotifyPropertyChanged(nameof(IntervalMin));
            NotifyPropertyChanged(nameof(IntervalMax));
            RaiseCommandsChanged();
        }

        private void RaiseCommandsChanged()
        {
            PreviousCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
        }
        #endregion

        private void UpdateAvailablesPages()
        {
            AvailablePages.Clear();

            // Few enough pages -> show them all
            if (PageMax < 8)
            {
                for (int i = 0; i < Math.Min(PageMax, 7); i++)
                    AvailablePages.Add(new PagingSelectionItem(i + 1, PageNumber));
                return;
            }

            int middlePage = Math.Clamp(PageNumber, 4, PageMax - 3);
            AvailablePages.Add(new PagingSelectionItem(1, PageNumber));

            AvailablePages.Add(PageNumber > 3 ? new PagingSelectionSeparator() : new PagingSelectionItem(2, PageNumber));

            AvailablePages.Add(new PagingSelectionItem(middlePage - 1, PageNumber));
            AvailablePages.Add(new PagingSelectionItem(middlePage, PageNumber));
            AvailablePages.Add(new PagingSelectionItem(middlePage + 1, PageNumber));

            AvailablePages.Add(PageNumber < PageMax - 3 ? new PagingSelectionSeparator() : new PagingSelectionItem(PageMax - 1, PageNumber));

            AvailablePages.Add(new PagingSelectionItem(PageMax, PageNumber));
        }

        #region Commands
        [RelayCommand()]
        private void GotTo(int pageNumber)
        {
            PageNumber = pageNumber;
        }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private void Previous() { PageNumber -= 1; }

        [RelayCommand(CanExecute = nameof(CanGoForward))]
        private void Next() { PageNumber += 1; }

        private bool CanGoBack() => PageNumber > 1;
        private bool CanGoForward() => PageNumber < PageMax;
        #endregion
    }
}