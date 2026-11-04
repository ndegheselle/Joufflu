using System.Windows;

namespace Joufflu.Toolkit;

/// <summary>
/// Size step of a control. <c>xl</c> is for text and icons only: controls without an <c>xl</c>
/// variant keep their <c>md</c> look.
/// </summary>
public enum ControlSize { xs, sm, md, lg, xl }

/// <summary>Size attached properties read by the Joufflu styles.</summary>
public static class Sizing
{
    /// <summary>
    /// Size of the element (defaults to <see cref="ControlSize.md"/>). Inherited, so setting it on a
    /// panel sizes every control in it.
    /// </summary>
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.RegisterAttached(
            "Size",
            typeof(ControlSize),
            typeof(Sizing),
            new FrameworkPropertyMetadata(ControlSize.md, FrameworkPropertyMetadataOptions.Inherits));

    public static ControlSize GetSize(DependencyObject obj) => (ControlSize)obj.GetValue(SizeProperty);
    public static void SetSize(DependencyObject obj, ControlSize value) => obj.SetValue(SizeProperty, value);

    /// <summary>Whether the width follows the height, for a button holding a single icon. Not inherited.</summary>
    public static readonly DependencyProperty IsSquareProperty =
        DependencyProperty.RegisterAttached(
            "IsSquare",
            typeof(bool),
            typeof(Sizing),
            new FrameworkPropertyMetadata(false));

    public static bool GetIsSquare(DependencyObject obj) => (bool)obj.GetValue(IsSquareProperty);
    public static void SetIsSquare(DependencyObject obj, bool value) => obj.SetValue(IsSquareProperty, value);
}
