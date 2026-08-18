using System.Windows;
using System.Windows.Controls;
using Joufflu.Feedback.Controls;

namespace Joufflu.Navigation.Controls;

/// <summary>
/// Wraps the whole application and layers the modal overlay stack and the toast stack
/// (always on top) above its content. Because it encapsulates everything — side menu and
/// page alike — a full screen overlay covers the whole surface.
/// The two services are created automatically but can be overridden (e.g. bound from a
/// shell view model) so the same instances can be shared with the pages.
/// </summary>
public class OverlayContainer : ContentControl
{
    static OverlayContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(OverlayContainer),
            new FrameworkPropertyMetadata(typeof(OverlayContainer)));
    }

    public OverlayContainer()
    {
        // Provide working defaults while still allowing a binding to override them.
        SetCurrentValue(OverlaysProperty, new OverlayService());
        SetCurrentValue(ToastsProperty, new ToastService());
    }

    public OverlayService Overlays
    {
        get => (OverlayService)GetValue(OverlaysProperty);
        set => SetValue(OverlaysProperty, value);
    }

    public static readonly DependencyProperty OverlaysProperty = DependencyProperty.Register(
        nameof(Overlays), typeof(OverlayService), typeof(OverlayContainer), new PropertyMetadata(null));

    public ToastService Toasts
    {
        get => (ToastService)GetValue(ToastsProperty);
        set => SetValue(ToastsProperty, value);
    }

    public static readonly DependencyProperty ToastsProperty = DependencyProperty.Register(
        nameof(Toasts), typeof(ToastService), typeof(OverlayContainer), new PropertyMetadata(null));
}
