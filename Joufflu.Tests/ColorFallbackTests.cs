using System.Windows.Media;

namespace Joufflu.Tests;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ColorFallbackTests
{
    private static readonly Color Pink = Color.FromRgb(0xff, 0xc0, 0xcb);
    private static readonly Color Grey = Color.FromRgb(0x80, 0x80, 0x80);

    [Test]
    public void Without_a_colour_the_result_is_the_fallback()
    {
        var fallback = new ColorFallback { Fallback = Grey };

        Assert.That(fallback.Result, Is.EqualTo(Grey));
    }

    [Test]
    public void A_colour_wins_over_the_fallback_even_when_transparent()
    {
        var fallback = new ColorFallback { Fallback = Grey, Color = System.Windows.Media.Colors.Transparent };

        Assert.That(fallback.Result, Is.EqualTo(System.Windows.Media.Colors.Transparent));
    }

    [Test]
    public void Removing_the_colour_returns_to_the_fallback()
    {
        var fallback = new ColorFallback { Fallback = Grey, Color = Pink };

        fallback.Color = null;

        Assert.That(fallback.Result, Is.EqualTo(Grey));
    }
}
