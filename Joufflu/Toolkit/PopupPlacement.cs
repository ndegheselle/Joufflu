using System.Windows;
using System.Windows.Controls.Primitives;

namespace Joufflu.Toolkit;

/// <summary>Side of the target a popup is placed on.</summary>
public enum PopupSide { Top, Bottom, Left, Right }

/// <summary>
/// How a popup lines up with its target along the side it is placed on. On <see cref="PopupSide.Top"/> and
/// <see cref="PopupSide.Bottom"/>, <see cref="Start"/> aligns the left edges and <see cref="End"/> the right ones; on
/// <see cref="PopupSide.Left"/> and <see cref="PopupSide.Right"/>, the top and the bottom edges.
/// </summary>
public enum PopupAlignment { Start, Center, End }

/// <summary>
/// Places a <see cref="Popup"/> on a side of its <see cref="Popup.PlacementTarget"/>, aligned on it, with the opposite
/// side as a fallback so WPF flips it at the edge of the screen.
/// <example>
/// <code>
/// &lt;Popup PlacementTarget="{Binding ElementName=SearchBox}"
///        toolkit:PopupPlacement.Side="Bottom" toolkit:PopupPlacement.Alignment="End" toolkit:PopupPlacement.Gap="4"&gt;
/// </code>
/// </example>
/// <para>
/// Setting <see cref="SideProperty"/> or <see cref="AlignmentProperty"/> hands the placement over: the popup's
/// <see cref="Popup.Placement"/> becomes <see cref="PlacementMode.Custom"/>, and its
/// <see cref="Popup.HorizontalOffset"/> / <see cref="Popup.VerticalOffset"/> still apply on top. Both are nullable so
/// that a value equal to the default (<c>Side="Bottom"</c>) still counts as set.
/// </para>
/// <para>
/// On anything but a <see cref="Popup"/> the properties do nothing by themselves: an element owning a popup it creates
/// reads them from itself, the way <c>ToolTipService.Placement</c> is set on the element. A <c>Joufflu.Inputs</c>
/// dropdown button does so for its popup.
/// </para>
/// </summary>
public static class PopupPlacement
{
    /// <summary>Side of the target the popup sits on; <see cref="PopupSide.Bottom"/> when only the alignment is set.</summary>
    public static readonly DependencyProperty SideProperty =
        DependencyProperty.RegisterAttached(
            "Side",
            typeof(PopupSide?),
            typeof(PopupPlacement),
            new PropertyMetadata(null, OnPlacementChanged));

    public static PopupSide? GetSide(DependencyObject obj) => (PopupSide?)obj.GetValue(SideProperty);
    public static void SetSide(DependencyObject obj, PopupSide? value) => obj.SetValue(SideProperty, value);

    /// <summary>How the popup lines up with the target; <see cref="PopupAlignment.Start"/> when only the side is set.</summary>
    public static readonly DependencyProperty AlignmentProperty =
        DependencyProperty.RegisterAttached(
            "Alignment",
            typeof(PopupAlignment?),
            typeof(PopupPlacement),
            new PropertyMetadata(null, OnPlacementChanged));

    public static PopupAlignment? GetAlignment(DependencyObject obj) => (PopupAlignment?)obj.GetValue(AlignmentProperty);
    public static void SetAlignment(DependencyObject obj, PopupAlignment? value) => obj.SetValue(AlignmentProperty, value);

    /// <summary>Gap in DIPs kept between a placed popup and its target unless told otherwise, tooltips included.</summary>
    public const double DefaultGap = 2;

    /// <summary>
    /// Space in DIPs kept between the target and the popup, <see cref="DefaultGap"/> by default. Only applies with a
    /// side or an alignment.
    /// </summary>
    public static readonly DependencyProperty GapProperty =
        DependencyProperty.RegisterAttached(
            "Gap",
            typeof(double),
            typeof(PopupPlacement),
            new PropertyMetadata(DefaultGap, OnGapChanged));

    public static double GetGap(DependencyObject obj) => (double)obj.GetValue(GapProperty);
    public static void SetGap(DependencyObject obj, double value) => obj.SetValue(GapProperty, value);

    private static bool IsPlaced(Popup popup) => GetSide(popup) != null || GetAlignment(popup) != null;

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Popup popup)
            return;

        // Both back to null: the popup is given back its native placement.
        if (!IsPlaced(popup))
        {
            popup.ClearValue(Popup.PlacementProperty);
            popup.ClearValue(Popup.CustomPopupPlacementCallbackProperty);
            return;
        }

        popup.Placement = PlacementMode.Custom;
        // Reads the properties when the popup opens, so a later change needs no rewiring.
        popup.CustomPopupPlacementCallback = (popupSize, targetSize, offset) => Place(
            GetSide(popup) ?? PopupSide.Bottom,
            GetAlignment(popup) ?? PopupAlignment.Start,
            GetGap(popup),
            popupSize,
            targetSize,
            offset);
        Reposition(popup);
    }

    private static void OnGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // A gap alone does not hand the placement over: it only applies along with a side or an alignment.
        if (d is Popup popup && IsPlaced(popup))
            Reposition(popup);
    }

    /// <summary>WPF only runs the placement callback when the popup opens: an open popup is reopened to move it.</summary>
    private static void Reposition(Popup popup)
    {
        if (!popup.IsOpen)
            return;
        popup.IsOpen = false;
        popup.IsOpen = true;
    }

    /// <summary>
    /// The popup on the <paramref name="side"/> of the target, lined up following <paramref name="alignment"/> and
    /// <paramref name="gap"/> clear of it, then on the opposite side, which WPF takes when the first does not fit on the
    /// screen. <paramref name="offset"/> is the popup's own offset, added to both.
    /// </summary>
    public static CustomPopupPlacement[] Place(
        PopupSide side,
        PopupAlignment alignment,
        double gap,
        Size popup,
        Size target,
        Point offset)
    {
        CustomPopupPlacement preferred = PlaceOn(side, alignment, gap, popup, target, offset);
        CustomPopupPlacement fallback = PlaceOn(OppositeOf(side), alignment, gap, popup, target, offset);
        return [preferred, fallback];
    }

    private static CustomPopupPlacement PlaceOn(
        PopupSide side,
        PopupAlignment alignment,
        double gap,
        Size popup,
        Size target,
        Point offset)
    {
        double alongX = Align(alignment, popup.Width, target.Width);
        double alongY = Align(alignment, popup.Height, target.Height);
        Point point = side switch
        {
            PopupSide.Top => new Point(alongX, -popup.Height - gap),
            PopupSide.Left => new Point(-popup.Width - gap, alongY),
            PopupSide.Right => new Point(target.Width + gap, alongY),
            _ => new Point(alongX, target.Height + gap),
        };
        // Above or below, the popup slides sideways to stay on screen; beside, it slides up or down.
        PopupPrimaryAxis axis = side is PopupSide.Top or PopupSide.Bottom
            ? PopupPrimaryAxis.Horizontal
            : PopupPrimaryAxis.Vertical;

        var shifted = new Point(point.X + offset.X, point.Y + offset.Y);
        return new CustomPopupPlacement(shifted, axis);
    }

    /// <summary>Where the popup starts along the target's edge.</summary>
    private static double Align(PopupAlignment alignment, double popupLength, double targetLength) => alignment switch
    {
        PopupAlignment.Center => (targetLength - popupLength) / 2,
        PopupAlignment.End => targetLength - popupLength,
        _ => 0,
    };

    private static PopupSide OppositeOf(PopupSide side) => side switch
    {
        PopupSide.Top => PopupSide.Bottom,
        PopupSide.Left => PopupSide.Right,
        PopupSide.Right => PopupSide.Left,
        _ => PopupSide.Top,
    };
}
