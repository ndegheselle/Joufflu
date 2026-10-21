using System.Windows.Controls;
using Joufflu.Toolkit;

namespace Joufflu.Tests.Toolkit;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ThemedTooltipTests
{
    [Test]
    public void A_tooltip_shows_instantly_by_default()
    {
        var element = new Border();

        ThemedTooltip.SetContent(element, "Hello");

        Assert.That(ToolTipService.GetInitialShowDelay(element), Is.EqualTo(0));
    }

    [Test]
    public void A_delay_the_element_sets_itself_is_kept()
    {
        var element = new Border();
        ToolTipService.SetInitialShowDelay(element, 500);

        ThemedTooltip.SetContent(element, "Hello");

        Assert.That(ToolTipService.GetInitialShowDelay(element), Is.EqualTo(500));
    }
}
