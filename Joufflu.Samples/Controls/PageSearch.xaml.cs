using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Joufflu.Samples.Controls;

/// <summary>
/// The title bar search box and the popup listing the pages it finds. Down moves from the box to the results, Enter
/// or a click opens a page, Escape or Up from the first result goes back to the box.
/// </summary>
public partial class PageSearch : UserControl
{
    private PageSearchViewModel ViewModel => (PageSearchViewModel)DataContext;

    public PageSearch()
    {
        InitializeComponent();
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);
        // The results popup counts as within: only leaving both the box and the results closes it.
        if (!IsKeyboardFocusWithin)
            ViewModel.IsOpen = false;
    }

    private void SearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        // Back in the box with a query still typed: its results show again.
        ViewModel.ShowResults();
    }

    private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (ViewModel.IsOpen == false)
            ViewModel.ShowResults();

        if (e.Key is not (Key.Down or Key.Enter))
            return;

        // The box only reports its text once typing settles: a key pressed right after typing would act on the
        // results of the previous text.
        ViewModel.Query = SearchBox.Text;
        if (ViewModel.Results.Count == 0)
            return;

        if (e.Key == Key.Enter)
            Open(ViewModel.Results[0]);
        else
            FocusResult(0);
        e.Handled = true;
    }

    private void ResultsList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter when ResultsList.SelectedItem is PageEntry page:
                Open(page);
                e.Handled = true;
                break;
            case Key.Up when ResultsList.SelectedIndex <= 0:
            case Key.Escape:
                SearchBox.Focus();
                e.Handled = true;
                break;
        }
    }

    private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var element = sender as FrameworkElement;
        var context = element?.DataContext as PageEntry;
        if (context == null)
            return;
        Open(context);
    }

    private void FocusResult(int index)
    {
        ViewModel.ShowResults();
        ResultsList.SelectedIndex = index;
        // The items are only generated once the popup is shown and laid out.
        Dispatcher.InvokeAsync(() =>
        {
            var container = (ListBoxItem?)ResultsList.ItemContainerGenerator.ContainerFromIndex(index);
            container?.Focus();
        }, DispatcherPriority.Loaded);
    }

    private void Open(PageEntry page)
    {
        ViewModel.Open(page);
        // The page is reached: the box is emptied, ready for the next search.
        SearchBox.ClearSearch();
        Keyboard.ClearFocus();
    }
}
