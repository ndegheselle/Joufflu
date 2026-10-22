using System.Windows.Media;
using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Colors;

public class ColorModelTests
{
    private ColorModel _model = null!;

    // NUnit shares one fixture instance across its tests : a fresh model keeps them independent.
    [SetUp]
    public void SetUp() => _model = new ColorModel();

    [Test]
    public void Reads_pure_red_as_hue_zero_fully_saturated_and_bright()
    {
        _model.SetColor(Parse("#FF0000"));

        Assert.That((_model.Hue, _model.Saturation, _model.Brightness), Is.EqualTo((0d, 1d, 1d)));
    }

    [Test]
    public void Builds_green_from_hue_120()
    {
        _model.SetHsb(120, 1, 1);

        Assert.That(Spell(_model.Color), Is.EqualTo("#FF00FF00"));
    }

    [TestCase("#FF336699")]
    [TestCase("#FFFA8072")]
    [TestCase("#FF0D0D0D")]
    [TestCase("#80FFFFFF")]
    public void Rgb_survives_a_round_trip_through_hsb(string color)
    {
        _model.SetColor(Parse(color));

        _model.SetHsb(_model.Hue, _model.Saturation, _model.Brightness);

        Assert.That(Spell(_model.Color), Is.EqualTo(color));
    }

    [Test]
    public void A_grey_keeps_the_previous_hue()
    {
        _model.SetColor(Parse("#0000FF"));

        _model.SetColor(Parse("#808080"));

        Assert.That(_model.Hue, Is.EqualTo(240));
    }

    [Test]
    public void A_six_digit_hex_keeps_the_current_alpha()
    {
        _model.SetColor(Parse("#80FF0000"));

        _model.TrySetHex("#00FF00");

        Assert.That(Spell(_model.Color), Is.EqualTo("#8000FF00"));
    }

    [Test]
    public void An_eight_digit_hex_sets_the_alpha_it_spells()
    {
        _model.SetColor(Parse("#80FF0000"));

        _model.TrySetHex("#4000FF00");

        Assert.That(Spell(_model.Color), Is.EqualTo("#4000FF00"));
    }

    [Test]
    public void Text_spelling_no_colour_leaves_the_colour_unchanged()
    {
        _model.SetColor(Parse("#336699"));

        bool accepted = _model.TrySetHex("#12XY");

        Assert.That(accepted, Is.False);
        Assert.That(Spell(_model.Color), Is.EqualTo("#FF336699"));
    }

    [Test]
    public void Changing_the_alpha_keeps_the_rgb()
    {
        _model.SetColor(Parse("#336699"));

        _model.SetAlpha(0.5);

        Assert.That(Spell(_model.Color), Is.EqualTo("#80336699"));
    }

    [Test]
    public void An_alpha_percent_past_100_is_full_opacity()
    {
        _model.SetAlphaPercent(150);

        Assert.That(_model.Alpha, Is.EqualTo(1));
    }

    private static Color Parse(string text) => (Color)ColorConverter.ConvertFromString(text);

    // "#AARRGGBB", compared as text so a failure shows the colour.
    private static string Spell(Color color) => color.ToString();
}
