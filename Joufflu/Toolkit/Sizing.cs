using System.Windows;

namespace Joufflu.Toolkit;

public enum ControlSize { xs, sm, md, lg, xl }

public static class Sizing
{
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.RegisterAttached(
            "Size",
            typeof(ControlSize),
            typeof(Sizing),
            new FrameworkPropertyMetadata(ControlSize.md, FrameworkPropertyMetadataOptions.Inherits));

    public static ControlSize GetSize(DependencyObject obj) => (ControlSize)obj.GetValue(SizeProperty);
    public static void SetSize(DependencyObject obj, ControlSize value) => obj.SetValue(SizeProperty, value);

    public static readonly DependencyProperty IsSquareProperty =
        DependencyProperty.RegisterAttached(
            "IsSquare",
            typeof(bool),
            typeof(Sizing),
            new FrameworkPropertyMetadata(false));

    public static bool GetIsSquare(DependencyObject obj) => (bool)obj.GetValue(IsSquareProperty);
    public static void SetIsSquare(DependencyObject obj, bool value) => obj.SetValue(IsSquareProperty, value);
}
