using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Joufflu.Toolkit;

/// <summary>Side of the target element a tooltip is placed on.</summary>
public enum TooltipPlacement { Top, Bottom, Left, Right }

/// <summary>
/// Places and sizes the native <see cref="ToolTip"/>, set the usual way (<c>ToolTip="…"</c> or <c>&lt;Button.ToolTip&gt;</c>).
/// On the element: <see cref="PlacementProperty"/> picks the side the tooltip sits on, centred on it with a small gap,
/// and <see cref="MaxWidthProperty"/> the width past which its text wraps.
/// <para>
/// The ToolTip style places every tooltip through <see cref="IsPlacedProperty"/> : a custom placement centred on the
/// chosen side, with the opposite side as a fallback, so WPF flips it at the edge of the screen.
/// </para>
/// </summary>
public static class ThemedTooltip
{
    /// <summary>Side of the element its tooltip is placed on (defaults to <see cref="TooltipPlacement.Bottom"/>).</summary>
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.RegisterAttached(
            "Placement",
            typeof(TooltipPlacement),
            typeof(ThemedTooltip),
            new PropertyMetadata(TooltipPlacement.Bottom));

    public static TooltipPlacement GetPlacement(DependencyObject obj) => (TooltipPlacement)obj.GetValue(PlacementProperty);
    public static void SetPlacement(DependencyObject obj, TooltipPlacement value) => obj.SetValue(PlacementProperty, value);

    /// <summary>Width of the element's tooltip past which its text wraps (defaults to 320).</summary>
    public static readonly DependencyProperty MaxWidthProperty =
        DependencyProperty.RegisterAttached(
            "MaxWidth",
            typeof(double),
            typeof(ThemedTooltip),
            new PropertyMetadata(320d));

    public static double GetMaxWidth(DependencyObject obj) => (double)obj.GetValue(MaxWidthProperty);
    public static void SetMaxWidth(DependencyObject obj, double value) => obj.SetValue(MaxWidthProperty, value);

    /// <summary>
    /// Set on a <see cref="ToolTip"/> by its style : places it on the <see cref="PlacementProperty"/> side of its
    /// target and bounds it to the target's <see cref="MaxWidthProperty"/>. Done in code : the callback is handed sizes
    /// alone, not the tooltip, and a binding to an attached property of the target written in the style never resolves.
    /// </summary>
    public static readonly DependencyProperty IsPlacedProperty =
        DependencyProperty.RegisterAttached(
            "IsPlaced",
            typeof(bool),
            typeof(ThemedTooltip),
            new PropertyMetadata(false, OnIsPlacedChanged));

    public static bool GetIsPlaced(DependencyObject obj) => (bool)obj.GetValue(IsPlacedProperty);
    public static void SetIsPlaced(DependencyObject obj, bool value) => obj.SetValue(IsPlacedProperty, value);

    /// <summary>Gap in DIPs kept between the element and the tooltip.</summary>
    private const double Gap = 2;

    private static void OnIsPlacedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToolTip toolTip)
            return;

        if ((bool)e.NewValue)
        {
            toolTip.Placement = PlacementMode.Custom;
            toolTip.CustomPopupPlacementCallback = (popup, target, _) =>
                Place(toolTip.PlacementTarget is { } element ? GetPlacement(element) : TooltipPlacement.Bottom, popup, target);
            toolTip.SetBinding(FrameworkElement.MaxWidthProperty, new Binding
            {
                Path = new PropertyPath("PlacementTarget.(0)", MaxWidthProperty),
                RelativeSource = RelativeSource.Self,
            });
        }
        else
        {
            toolTip.ClearValue(ToolTip.PlacementProperty);
            toolTip.ClearValue(ToolTip.CustomPopupPlacementCallbackProperty);
            toolTip.ClearValue(FrameworkElement.MaxWidthProperty);
        }
    }

    /// <summary>
    /// The tooltip centred on the <paramref name="placement"/> side of the target and <see cref="Gap"/> clear of it,
    /// then on the opposite side, which WPF takes when the first does not fit on the screen.
    /// </summary>
    public static CustomPopupPlacement[] Place(TooltipPlacement placement, Size popup, Size target)
    {
        double centerX = (target.Width - popup.Width) / 2;
        double centerY = (target.Height - popup.Height) / 2;
        var top = new CustomPopupPlacement(new Point(centerX, -popup.Height - Gap), PopupPrimaryAxis.Horizontal);
        var bottom = new CustomPopupPlacement(new Point(centerX, target.Height + Gap), PopupPrimaryAxis.Horizontal);
        var left = new CustomPopupPlacement(new Point(-popup.Width - Gap, centerY), PopupPrimaryAxis.Vertical);
        var right = new CustomPopupPlacement(new Point(target.Width + Gap, centerY), PopupPrimaryAxis.Vertical);

        return placement switch
        {
            TooltipPlacement.Top => [top, bottom],
            TooltipPlacement.Left => [left, right],
            TooltipPlacement.Right => [right, left],
            _ => [bottom, top],
        };
    }
}
