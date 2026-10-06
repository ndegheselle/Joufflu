using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Navigation;

public enum EnumConfirmationType
{
    Neutral,
    Info,
    Success,
    Warning,
    Danger
}

/// <summary>
/// Optional contract for a view model that wants to react to navigation lifecycle events.
/// Navigation works on any object (resolved to a view through implicit <c>DataTemplate</c>s);
/// implementing this interface is only needed when lifecycle callbacks are useful.
/// </summary>
public interface IPage
{
    /// <summary>Called right after the page becomes the active content.</summary>
    void OnNavigatedTo() { }

    /// <summary>Called right after the page stops being the active content.</summary>
    void OnNavigatedFrom() { }
}

/// <summary>
/// Displays a single page (view model) at a time and exposes the current one.
/// </summary>
public interface INavigator
{
    object? CurrentPage { get; }

    void Navigate(object? page);

    /// <summary>Navigates to the page the implementation resolves for <paramref name="type"/>.</summary>
    void Navigate(Type? type);

    event EventHandler<object?>? Navigated;
}

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
