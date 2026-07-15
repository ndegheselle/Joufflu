using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Joufflu.Navigation;

/// <summary>
/// Default <see cref="INavigator"/> implementation. Shows a single page (view model) at a time;
/// the matching view is resolved by WPF through implicit <c>DataTemplate</c>s.
/// </summary>
public class Navigator : ObservableObject, INavigator
{
    private object? _currentPage;

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    /// <summary>Navigates to the page passed as command parameter.</summary>
    public ICommand NavigateCommand { get; }

    public event EventHandler<object?>? Navigated;

    public Navigator()
    {
        NavigateCommand = new RelayCommand<object>(page =>
        {
            if (page != null)
                Navigate(page);
        });
    }

    public void Navigate(object page)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (ReferenceEquals(_currentPage, page))
            return;

        (_currentPage as IPage)?.OnNavigatedFrom();
        CurrentPage = page;
        (page as IPage)?.OnNavigatedTo();
        Navigated?.Invoke(this, page);
    }
}
