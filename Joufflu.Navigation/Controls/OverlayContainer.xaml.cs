using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Navigation.Controls;

/// <summary>
/// Wraps the whole application and layers the modal overlay stack above its content.
/// Because it encapsulates everything — side menu and page alike — a full screen overlay
/// covers the whole surface.
/// The service is created automatically but can be overridden (e.g. bound from a shell view
/// model) so the same instance can be shared with the pages.
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
        // Provide a working default while still allowing a binding to override it.
        SetCurrentValue(OverlaysProperty, new Overlayer());
    }

    /// <summary>
    /// The service whose overlays are shown. Typed by its contract, so a shell can bind the
    /// <see cref="IOverlayer"/> it hands its pages. The template draws the stack of an
    /// <see cref="Overlayer"/> (its <see cref="Overlayer.Overlays"/>): an implementation of your
    /// own shows nothing unless it exposes the same.
    /// </summary>
    public IOverlayer Overlays
    {
        get => (IOverlayer)GetValue(OverlaysProperty);
        set => SetValue(OverlaysProperty, value);
    }

    public static readonly DependencyProperty OverlaysProperty = DependencyProperty.Register(
        nameof(Overlays), typeof(IOverlayer), typeof(OverlayContainer), new PropertyMetadata(null));
}
