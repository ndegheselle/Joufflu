using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// Search input with a built-in delay to limit the number of requests to an API or database.
    /// <para>
    /// The debounced query is exposed three ways so it fits any style: the <see cref="SearchChanged"/>
    /// event (code-behind), the two-way bindable <see cref="SearchText"/> property, and the
    /// <see cref="SearchCommand"/> (MVVM). <see cref="TextBox.Text"/> still updates on every keystroke;
    /// the three members above only fire once typing settles.
    /// </para>
    /// </summary>
    public partial class Search : TextBox
    {
        public event Action<string>? SearchChanged;

        /// <summary>The debounced search text. Bind this (two-way) to a view model query property.</summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(
                nameof(SearchText),
                typeof(string),
                typeof(Search),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? SearchText
        {
            get => (string?)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        /// <summary>Executed with the debounced search text as parameter whenever the query settles.</summary>
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(
                nameof(SearchCommand),
                typeof(ICommand),
                typeof(Search),
                new PropertyMetadata(null));

        public ICommand? SearchCommand
        {
            get => (ICommand?)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        private readonly DispatcherTimer _searchTimer;
        public Search()
        {
            _searchTimer = InitSearchTimer();
            this.KeyUp += OnKeyUp;
            // Stop the debounce timer when leaving the visual tree: a running timer would
            // otherwise keep this control alive (and could fire SearchChanged after unload).
            this.Unloaded += (_, _) => _searchTimer.Stop();
        }

        private DispatcherTimer InitSearchTimer()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            timer.Tick += FilterTimer_Tick;
            return timer;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ClearSearch();
                return;
            }

            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void FilterTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            RaiseSearch();
        }

        [RelayCommand]
        public void ClearSearch()
        {
            Clear();
            RaiseSearch();
        }

        /// <summary>Publishes the current text through the event, the bindable property and the command.</summary>
        private void RaiseSearch()
        {
            SearchText = Text;
            SearchChanged?.Invoke(Text);
            if (SearchCommand?.CanExecute(Text) == true)
                SearchCommand.Execute(Text);
        }
    }
}
