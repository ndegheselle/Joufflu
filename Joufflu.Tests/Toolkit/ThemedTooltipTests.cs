using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Joufflu.Toolkit;

namespace Joufflu.Tests.Toolkit;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ThemedTooltipTests
{
    private static readonly Size Popup = new(100, 20);
    private static readonly Size Target = new(40, 30);

    [Test]
    public void A_tooltip_below_is_centred_under_the_element_with_a_gap()
    {
        CustomPopupPlacement[] placements = ThemedTooltip.Place(TooltipPlacement.Bottom, Popup, Target);

        Assert.That(placements[0].Point, Is.EqualTo(new Point(-30, 32)));
    }

    [Test]
    public void A_tooltip_on_the_left_is_centred_beside_the_element_with_a_gap()
    {
        CustomPopupPlacement[] placements = ThemedTooltip.Place(TooltipPlacement.Left, Popup, Target);

        Assert.That(placements[0].Point, Is.EqualTo(new Point(-102, 5)));
    }

    [Test]
    public void The_opposite_side_is_the_fallback_when_the_first_does_not_fit()
    {
        CustomPopupPlacement[] placements = ThemedTooltip.Place(TooltipPlacement.Top, Popup, Target);

        Assert.Multiple(() =>
        {
            Assert.That(placements[0].Point, Is.EqualTo(new Point(-30, -22)), "above");
            Assert.That(placements[1].Point, Is.EqualTo(new Point(-30, 32)), "below");
        });
    }

    [Test]
    public void A_placed_tooltip_takes_the_custom_placement()
    {
        var toolTip = new ToolTip();

        ThemedTooltip.SetIsPlaced(toolTip, true);

        Assert.Multiple(() =>
        {
            Assert.That(toolTip.Placement, Is.EqualTo(PlacementMode.Custom));
            Assert.That(toolTip.CustomPopupPlacementCallback, Is.Not.Null);
        });
    }

    [Test]
    public void The_tooltip_text_wraps_at_320_unless_the_element_says_otherwise()
    {
        var element = new Border();

        Assert.That(ThemedTooltip.GetMaxWidth(element), Is.EqualTo(320));
    }
}
