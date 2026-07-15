using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.Navigation.Controls
{
    public partial class Paging : Control, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public delegate void HandlePagingChange(int pageNumber, int capacity);
        public event HandlePagingChange? PagingChange;

        private TextBox? _inputPage;

        #region DependencyProperties
        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register(
                nameof(Total),
                typeof(int),
                typeof(Paging),
                new PropertyMetadata(0, (o, value) => ((Paging)o).OnTotalChanged()));

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _inputPage = GetTemplateChild("PART_InputPage") as TextBox;
            if (_inputPage != null)
            {
                _inputPage.PreviewTextInput += TextBox_PreviewTextInput;
                _inputPage.KeyDown += TextBox_OnKeyDown;
            }
        }

        #region Change Events
        private void OnTotalChanged()
        {
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
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(PageMax));
            NotifyPropertyChanged(nameof(IntervalMin));
            NotifyPropertyChanged(nameof(IntervalMax));
            RaiseCommandsChanged();
        }

        private void RaiseCommandsChanged()
        {
            PreviousCommand.NotifyCanExecuteChanged();
            NextCommand.NotifyCanExecuteChanged();
            FirstCommand.NotifyCanExecuteChanged();
            LastCommand.NotifyCanExecuteChanged();
        }
        #endregion

        #region Commands
        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private void Previous() { PageNumber -= 1; }

        [RelayCommand(CanExecute = nameof(CanGoForward))]
        private void Next() { PageNumber += 1; }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private void First() { PageNumber = 1; }

        [RelayCommand(CanExecute = nameof(CanGoForward))]
        private void Last() { PageNumber = PageMax; }

        private bool CanGoBack() => PageNumber > 1;
        private bool CanGoForward() => PageNumber < PageMax;
        #endregion

        #region UI Events
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_inputPage == null) return;
            if (PageMax > 1 && int.TryParse(_inputPage.Text, out int number))
            {
                int clamped = Math.Clamp(number, 1, PageMax);
                if (PageNumber != clamped)
                    PageNumber = clamped;
            }
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                Keyboard.ClearFocus();
        }
        #endregion
    }
}