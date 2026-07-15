using Joufflu.Navigation.Controls;

namespace Joufflu.Navigation;

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

    void Navigate(object page);

    event EventHandler<object?>? Navigated;
}

/// <summary>
/// Hosts a stack of modal overlays on top of the current page.
/// </summary>
public interface IOverlayService
{
    /// <summary>
    /// Shows <paramref name="content"/> as a modal overlay and completes when it is closed.
    /// The result carries whatever the closing action provided (<see langword="null"/> when dismissed).
    /// </summary>
    Task<bool?> Show(object content, OverlayOptions? options = null);

    void Close(OverlayInstance overlay, bool? result = null);

    void CloseTop(bool? result = null);
}

/// <summary>
/// Optional contract for overlay content that wants to provide its own options
/// (title, action bar, ...) instead of having them supplied at <see cref="IOverlayService.Show"/> time.
/// </summary>
public interface IOverlayContent : IPage
{
    OverlayOptions Options { get; }
}
