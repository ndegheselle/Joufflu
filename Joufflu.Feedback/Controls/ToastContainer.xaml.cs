using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Feedback.Controls;

/// <summary>Corner a <see cref="ToastContainer"/> stacks its toasts in.</summary>
public enum ToastPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

/// <summary>
/// Wraps the whole application and stacks the toasts of a <see cref="ToastService"/> above
/// its content, in the corner given by <see cref="Position"/>. Wrapping everything keeps the
/// toasts on top of the rest of the UI, modal overlays included.
/// The service is created automatically but can be overridden (e.g. bound from a shell view
/// model) so the same instance can be shared with the pages.
/// </summary>
public class ToastContainer : ContentControl
{
    static ToastContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToastContainer),
            new FrameworkPropertyMetadata(typeof(ToastContainer)));
    }

    public ToastContainer()
    {
        // Provide a working default while still allowing a binding to override it.
        SetCurrentValue(ToastsProperty, new ToastService());
    }

    public ToastService Toasts
    {
        get => (ToastService)GetValue(ToastsProperty);
        set => SetValue(ToastsProperty, value);
    }

    public static readonly DependencyProperty ToastsProperty = DependencyProperty.Register(
        nameof(Toasts), typeof(ToastService), typeof(ToastContainer), new PropertyMetadata(null));

    /// <summary>
    /// Corner the toasts stack in. The newest toast always sits closest to that corner.
    /// </summary>
    public ToastPosition Position
    {
        get => (ToastPosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
        nameof(Position), typeof(ToastPosition), typeof(ToastContainer),
        new PropertyMetadata(ToastPosition.TopRight));
}
