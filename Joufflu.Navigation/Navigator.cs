using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Navigation;

/// <summary>
/// Default <see cref="INavigator"/> implementation. Shows a single page (view model) at a time;
/// the matching view is resolved by WPF through implicit <c>DataTemplate</c>s.
/// </summary>
public partial class Navigator : ObservableObject, INavigator
{
    /// <summary>
    /// Turns a page type into the page instance to display. Required to navigate by type
    /// (what a <see cref="Controls.NavigationItem"/> does through its
    /// <see cref="Controls.NavigationItem.TargetType"/>).
    /// </summary>
    private readonly Func<Type, object?> resolver;

    [ObservableProperty]
    private object? currentPage;

    public event EventHandler<object?>? Navigated;

    public Navigator(Func<Type, object?> resolver)
    {
        this.resolver = resolver;
    }

    /// <summary>Navigates to the page the resolver returns for <paramref name="type"/>.</summary>
    public void Navigate(Type? type)
    {
        if (type == null)
            return;

        object? page = resolver.Invoke(type);
        if (page == null)
            return;

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
