using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Joufflu.Toolkit;

namespace Joufflu.Tests.Toolkit;

/// <summary>
/// The placement maths itself is covered by <see cref="PopupPlacementTests"/>. Fixtures run in a single-threaded
/// apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class TooltipTests
{
    private static readonly Size Popup = new(100, 20);
    private static readonly Size Target = new(40, 30);

    private static Point PreferredPoint(ToolTip toolTip)
        => toolTip.CustomPopupPlacementCallback(Popup, Target, new Point(0, 0))[0].Point;

    [Test]
    public void A_placed_tooltip_takes_the_custom_placement()
    {
        var toolTip = new ToolTip();

        Tooltip.SetIsPlaced(toolTip, true);

        Assert.Multiple(() =>
        {
            Assert.That(toolTip.Placement, Is.EqualTo(PlacementMode.Custom));
            Assert.That(toolTip.CustomPopupPlacementCallback, Is.Not.Null);
        });
    }

    [Test]
    public void A_tooltip_is_centred_under_its_element_with_a_gap_by_default()
    {
        var toolTip = new ToolTip { PlacementTarget = new Border() };
        Tooltip.SetIsPlaced(toolTip, true);

        Assert.That(PreferredPoint(toolTip), Is.EqualTo(new Point(-30, 32)));
    }

    [Test]
    public void The_side_and_alignment_are_read_from_the_element()
    {
        var element = new Border();
        Tooltip.SetPlacement(element, PopupSide.Top);
        Tooltip.SetAlignment(element, PopupAlignment.End);
        var toolTip = new ToolTip { PlacementTarget = element };
        Tooltip.SetIsPlaced(toolTip, true);

        Assert.That(PreferredPoint(toolTip), Is.EqualTo(new Point(-60, -22)));
    }

    [Test]
    public void The_tooltip_text_wraps_at_320_unless_the_element_says_otherwise()
    {
        var element = new Border();

        Assert.That(Tooltip.GetMaxWidth(element), Is.EqualTo(320));
    }
}
