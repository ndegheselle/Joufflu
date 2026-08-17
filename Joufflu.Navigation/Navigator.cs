using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Navigation;

/// <summary>
/// Default <see cref="INavigator"/> implementation. Shows a single page (view model) at a time;
/// the matching view is resolved by WPF through implicit <c>DataTemplate</c>s.
/// </summary>
public partial class Navigator : ObservableObject, INavigator
{
    [ObservableProperty]
    private object? currentPage;

    public event EventHandler<object?>? Navigated;

    private readonly Func<object?, Type> resolver;

    public Navigator(Func<object?, Type> resolver)
    {
        this.resolver = resolver;
    }

    public void Navigate(Type? type)
    {
        var page = resolver.Invoke(type);
        if (page == null) return;

        Navigate(page);
    }

    public void Navigate(object? page)
    {
        if (ReferenceEquals(CurrentPage, page))
            return;

        (CurrentPage as IPage)?.OnNavigatedFrom();
        CurrentPage = page;
        (page as IPage)?.OnNavigatedTo();
        Navigated?.Invoke(this, page);
    }
}
