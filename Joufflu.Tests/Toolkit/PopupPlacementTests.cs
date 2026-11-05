using System.Windows;
using System.Windows.Controls.Primitives;
using Joufflu.Toolkit;

namespace Joufflu.Tests.Toolkit;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class PopupPlacementTests
{
    private static readonly Size Popup = new(100, 20);
    private static readonly Size Target = new(40, 30);
    private static readonly Point NoOffset = new(0, 0);

    private static Point PreferredPoint(PopupSide side, PopupAlignment alignment, double gap = 0)
        => PopupPlacement.Place(side, alignment, gap, Popup, Target, NoOffset)[0].Point;

    [Test]
    public void A_popup_below_and_at_the_start_shares_the_left_edge_of_its_target()
    {
        Assert.That(PreferredPoint(PopupSide.Bottom, PopupAlignment.Start), Is.EqualTo(new Point(0, 30)));
    }

    [Test]
    public void A_popup_below_and_at_the_end_shares_the_right_edge_of_its_target()
    {
        Assert.That(PreferredPoint(PopupSide.Bottom, PopupAlignment.End), Is.EqualTo(new Point(-60, 30)));
    }

    [Test]
    public void A_popup_above_and_centred_is_centred_over_its_target_with_the_gap()
    {
        Assert.That(PreferredPoint(PopupSide.Top, PopupAlignment.Center, gap: 2), Is.EqualTo(new Point(-30, -22)));
    }

    [Test]
    public void A_popup_beside_aligns_on_the_vertical_edges()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PreferredPoint(PopupSide.Right, PopupAlignment.Start, gap: 2), Is.EqualTo(new Point(42, 0)), "right, top edges");
            Assert.That(PreferredPoint(PopupSide.Left, PopupAlignment.End, gap: 2), Is.EqualTo(new Point(-102, 10)), "left, bottom edges");
        });
    }

    [Test]
    public void The_opposite_side_with_the_same_alignment_is_the_fallback()
    {
        CustomPopupPlacement[] placements = PopupPlacement.Place(PopupSide.Bottom, PopupAlignment.End, 4, Popup, Target, NoOffset);

        Assert.Multiple(() =>
        {
            Assert.That(placements[0].Point, Is.EqualTo(new Point(-60, 34)), "below");
            Assert.That(placements[1].Point, Is.EqualTo(new Point(-60, -24)), "above");
        });
    }

    [Test]
    public void The_offset_of_the_popup_is_added_to_the_placement()
    {
        CustomPopupPlacement[] placements = PopupPlacement.Place(PopupSide.Bottom, PopupAlignment.Start, 0, Popup, Target, new Point(5, 7));

        Assert.That(placements[0].Point, Is.EqualTo(new Point(5, 37)));
    }

    [Test]
    public void Setting_the_default_side_still_hands_the_placement_over()
    {
        var popup = new Popup();

        PopupPlacement.SetSide(popup, PopupSide.Bottom);

        Assert.Multiple(() =>
        {
            Assert.That(popup.Placement, Is.EqualTo(PlacementMode.Custom));
            Assert.That(popup.CustomPopupPlacementCallback, Is.Not.Null);
        });
    }

    [Test]
    public void A_placed_popup_keeps_the_tooltip_gap_by_default()
    {
        var popup = new Popup();
        PopupPlacement.SetSide(popup, PopupSide.Bottom);

        Point preferred = popup.CustomPopupPlacementCallback(Popup, Target, NoOffset)[0].Point;

        Assert.That(preferred, Is.EqualTo(new Point(0, 30 + PopupPlacement.DefaultGap)));
    }

    [Test]
    public void A_gap_alone_leaves_the_native_placement()
    {
        var popup = new Popup { Placement = PlacementMode.Right };

        PopupPlacement.SetGap(popup, 4);

        Assert.That(popup.Placement, Is.EqualTo(PlacementMode.Right));
    }

    [Test]
    public void Clearing_the_side_and_alignment_gives_the_native_placement_back()
    {
        var popup = new Popup();
        PopupPlacement.SetSide(popup, PopupSide.Top);
        PopupPlacement.SetAlignment(popup, PopupAlignment.End);

        popup.ClearValue(PopupPlacement.SideProperty);
        popup.ClearValue(PopupPlacement.AlignmentProperty);

        Assert.Multiple(() =>
        {
            Assert.That(popup.Placement, Is.EqualTo(PlacementMode.Bottom));
            Assert.That(popup.CustomPopupPlacementCallback, Is.Null);
        });
    }
}
