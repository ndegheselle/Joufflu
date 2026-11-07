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
    /// <see cref="SearchCommand"/> (MVVM). <see cref="TextBox.Text"/> still updates on every change;
    /// the three members above only fire once it settles, whether typed, pasted or set from code.
    /// </para>
    /// </summary>
    public partial class Search : TextBox
    {
        static Search()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Search), new FrameworkPropertyMetadata(typeof(Search)));
        }

        public event Action<string>? SearchChanged;

        /// <summary>The debounced search text. Bind this (two-way) to a view model query property.</summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(
                nameof(SearchText),
                typeof(string),
                typeof(Search),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

        public string? SearchText
        {
            get => (string?)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        /// <summary>
        /// Shows a search text set from outside (XAML, binding, code) in the box. It is taken as already searched:
        /// whoever set it knows the query, so it doesn't raise the event or the command again.
        /// </summary>
        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var search = (Search)d;
            string text = (string?)e.NewValue ?? string.Empty;
            // Set by RaiseSearch from the current text, nothing to show.
            if (search.Text == text)
                return;

            search._lastSearch = text;
            search.Text = text;
            // Setting the text restarted the debounce through OnTextChanged, there is nothing left to search.
            search._searchTimer.Stop();
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

        /// <summary>Text of the last search raised, so that a text settling back to it doesn't search again.</summary>
        private string _lastSearch = string.Empty;

        public Search()
        {
            _searchTimer = InitSearchTimer();
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

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            // Any change, typed, pasted or set from code, restarts the debounce.
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ClearSearch();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
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
            // Clear() restarted the debounce through OnTextChanged, the search is raised right away instead.
            _searchTimer.Stop();
            RaiseSearch();
        }

        /// <summary>Publishes the current text through the event, the bindable property and the command.</summary>
        private void RaiseSearch()
        {
            // The text settled back to the last searched one (typed then erased for instance).
            if (Text == _lastSearch)
                return;
            _lastSearch = Text;

            SearchText = Text;
            SearchChanged?.Invoke(Text);
            if (SearchCommand?.CanExecute(Text) == true)
                SearchCommand.Execute(Text);
        }
    }
}
