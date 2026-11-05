using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Joufflu.Samples.Controls;

/// <summary>
/// The page search overlay: the search box above the pages it finds. Down moves from the box to the results, Enter or
/// a click opens a page, Up from the first result goes back to the box.
/// </summary>
public partial class PageSearch : UserControl
{
    private PageSearchViewModel ViewModel => (PageSearchViewModel)DataContext;

    public PageSearch()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Ready to type as soon as the overlay shows.
        SearchBox.Focus();
    }

    private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Down or Key.Enter))
            return;

        // The box only reports its text once typing settles: a key pressed right after typing would act on the
        // results of the previous text.
        ViewModel.Query = SearchBox.Text;
        if (ViewModel.Results.Count == 0)
            return;

        if (e.Key == Key.Enter)
            ViewModel.Open(ViewModel.Results[0]);
        else
            FocusResult(0);
        e.Handled = true;
    }

    private void ResultsList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter when ResultsList.SelectedItem is PageEntry page:
                ViewModel.Open(page);
                e.Handled = true;
                break;
            case Key.Up when ResultsList.SelectedIndex <= 0:
                SearchBox.Focus();
                e.Handled = true;
                break;
        }
    }

    private void ResultsList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var clicked = (DependencyObject)e.OriginalSource;
        // Null outside an item, on the scroll bar for instance.
        if (ItemsControl.ContainerFromElement(ResultsList, clicked) is not ListBoxItem { DataContext: PageEntry page })
            return;

        ViewModel.Open(page);
    }

    private void FocusResult(int index)
    {
        ResultsList.SelectedIndex = index;
        // The containers of a list just filtered are only generated at the next layout.
        Dispatcher.InvokeAsync(() =>
        {
            var container = (ListBoxItem?)ResultsList.ItemContainerGenerator.ContainerFromIndex(index);
            container?.Focus();
        }, DispatcherPriority.Loaded);
    }
}
