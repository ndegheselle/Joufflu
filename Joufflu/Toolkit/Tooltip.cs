using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Joufflu.Toolkit;

/// <summary>
/// Places and sizes the native <see cref="ToolTip"/>, set the usual way (<c>ToolTip="…"</c> or <c>&lt;Button.ToolTip&gt;</c>).
/// On the element: <see cref="PlacementProperty"/> picks the side the tooltip sits on, <see cref="AlignmentProperty"/>
/// how it lines up with the element (centred by default), and <see cref="MaxWidthProperty"/> the width past which its
/// text wraps.
/// <para>
/// The ToolTip style places every tooltip through <see cref="IsPlacedProperty"/>, with <see cref="PopupPlacement"/>: the
/// <see cref="PopupPlacement.DefaultGap"/> clear of the chosen side, and the opposite side as a fallback, so WPF flips it at the edge of the screen.
/// </para>
/// </summary>
public static class Tooltip
{
    /// <summary>Side of the element its tooltip is placed on (defaults to <see cref="PopupSide.Bottom"/>).</summary>
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.RegisterAttached(
            "Placement",
            typeof(PopupSide),
            typeof(Tooltip),
            new PropertyMetadata(PopupSide.Bottom));

    public static PopupSide GetPlacement(DependencyObject obj) => (PopupSide)obj.GetValue(PlacementProperty);
    public static void SetPlacement(DependencyObject obj, PopupSide value) => obj.SetValue(PlacementProperty, value);

    /// <summary>How the element's tooltip lines up with it along its side (defaults to <see cref="PopupAlignment.Center"/>).</summary>
    public static readonly DependencyProperty AlignmentProperty =
        DependencyProperty.RegisterAttached(
            "Alignment",
            typeof(PopupAlignment),
            typeof(Tooltip),
            new PropertyMetadata(PopupAlignment.Center));

    public static PopupAlignment GetAlignment(DependencyObject obj) => (PopupAlignment)obj.GetValue(AlignmentProperty);
    public static void SetAlignment(DependencyObject obj, PopupAlignment value) => obj.SetValue(AlignmentProperty, value);

    /// <summary>Width of the element's tooltip past which its text wraps (defaults to 320).</summary>
    public static readonly DependencyProperty MaxWidthProperty =
        DependencyProperty.RegisterAttached(
            "MaxWidth",
            typeof(double),
            typeof(Tooltip),
            new PropertyMetadata(320d));

    public static double GetMaxWidth(DependencyObject obj) => (double)obj.GetValue(MaxWidthProperty);
    public static void SetMaxWidth(DependencyObject obj, double value) => obj.SetValue(MaxWidthProperty, value);

    /// <summary>
    /// Set on a <see cref="ToolTip"/> by its style : places it following the <see cref="PlacementProperty"/> and
    /// <see cref="AlignmentProperty"/> of its target and bounds it to the target's <see cref="MaxWidthProperty"/>. The
    /// settings live on the element, not on the tooltip, as the tooltip is usually created from a plain
    /// <c>ToolTip="…"</c>. Done in code : the callback is handed sizes alone, not the tooltip, and a binding to an
    /// attached property of the target written in the style never resolves.
    /// </summary>
    public static readonly DependencyProperty IsPlacedProperty =
        DependencyProperty.RegisterAttached(
            "IsPlaced",
            typeof(bool),
            typeof(Tooltip),
            new PropertyMetadata(false, OnIsPlacedChanged));

    public static bool GetIsPlaced(DependencyObject obj) => (bool)obj.GetValue(IsPlacedProperty);
    public static void SetIsPlaced(DependencyObject obj, bool value) => obj.SetValue(IsPlacedProperty, value);

    private static void OnIsPlacedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToolTip toolTip)
            return;

        if ((bool)e.NewValue)
        {
            toolTip.Placement = PlacementMode.Custom;
            toolTip.CustomPopupPlacementCallback = (popup, target, offset) => Place(toolTip, popup, target, offset);
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

    private static CustomPopupPlacement[] Place(ToolTip toolTip, Size popup, Size target, Point offset)
    {
        UIElement? element = toolTip.PlacementTarget;
        PopupSide side = element is null ? PopupSide.Bottom : GetPlacement(element);
        PopupAlignment alignment = element is null ? PopupAlignment.Center : GetAlignment(element);
        return PopupPlacement.Place(side, alignment, PopupPlacement.DefaultGap, popup, target, offset);
    }
}
