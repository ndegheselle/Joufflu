using System.Windows;
using System.Windows.Controls;
using Joufflu.Controls;

namespace Joufflu.Navigation.Controls;

/// <summary>
/// Root navigation surface. Displays the current page and hosts the modal overlay stack
/// and the toast stack (always on top). The three services are created automatically but
/// can be overridden (e.g. bound from a shell view model) so the same instances can be
/// shared with a <see cref="NavigationMenu"/> or with pages.
/// </summary>
public class NavigationContainer : Control
{
    static NavigationContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationContainer),
            new FrameworkPropertyMetadata(typeof(NavigationContainer)));
    }

    public NavigationContainer()
    {
        // Provide working defaults while still allowing a binding to override them.
        SetCurrentValue(NavigatorProperty, new Navigator());
        SetCurrentValue(OverlaysProperty, new OverlayService());
        SetCurrentValue(ToastsProperty, new ToastService());
    }

    public Navigator Navigator
    {
        get => (Navigator)GetValue(NavigatorProperty);
        set => SetValue(NavigatorProperty, value);
    }

    public static readonly DependencyProperty NavigatorProperty = DependencyProperty.Register(
        nameof(Navigator), typeof(Navigator), typeof(NavigationContainer), new PropertyMetadata(null));

    public OverlayService Overlays
    {
        get => (OverlayService)GetValue(OverlaysProperty);
        set => SetValue(OverlaysProperty, value);
    }

    public static readonly DependencyProperty OverlaysProperty = DependencyProperty.Register(
        nameof(Overlays), typeof(OverlayService), typeof(NavigationContainer), new PropertyMetadata(null));

    public ToastService Toasts
    {
        get => (ToastService)GetValue(ToastsProperty);
        set => SetValue(ToastsProperty, value);
    }

    public static readonly DependencyProperty ToastsProperty = DependencyProperty.Register(
        nameof(Toasts), typeof(ToastService), typeof(NavigationContainer), new PropertyMetadata(null));
}
