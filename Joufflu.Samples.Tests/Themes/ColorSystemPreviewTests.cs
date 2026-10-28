using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Joufflu.Samples.Views.Themes;

namespace Joufflu.Samples.Tests.Themes;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ColorSystemPreviewTests
{
    private ColorSystemPreview _preview = null!;
    private ComboBox _palettes = null!;

    [SetUp]
    public void Show()
    {
        _preview = new ColorSystemPreview();
        _palettes = (ComboBox)_preview.FindName("PaletteChoice");
        _preview.Measure(new Size(1000, 1400));
    }

    [Test]
    public void The_hover_and_pressed_backgrounds_of_a_control_are_two_increasing_steps_from_the_page()
    {
        Select("Light");

        double hover = Distance("PreviewControlHoverBrush", "PreviewBaseBrush");
        double pressed = Distance("PreviewControlPressedBrush", "PreviewBaseBrush");

        Assert.That(hover, Is.GreaterThan(0));
        Assert.That(pressed, Is.GreaterThan(hover));
    }

    [Test]
    public void A_control_darkens_on_hover_in_the_light_palette_and_lightens_in_the_dark_one()
    {
        Select("Light");
        double lightRest = Lightness(Brush("PreviewBaseBrush"));
        double lightHover = Lightness(Brush("PreviewControlHoverBrush"));
        Select("Dark");
        double darkRest = Lightness(Brush("PreviewBaseBrush"));
        double darkHover = Lightness(Brush("PreviewControlHoverBrush"));

        Assert.Multiple(() =>
        {
            Assert.That(lightHover, Is.LessThan(lightRest), "light");
            Assert.That(darkHover, Is.GreaterThan(darkRest), "dark");
        });
    }

    [Test]
    public void A_control_hovers_away_from_the_page_in_the_daisyUI_palettes_too()
    {
        Select("daisyUI light");
        double lightRest = Lightness(Brush("PreviewBaseBrush"));
        double lightHover = Lightness(Brush("PreviewControlHoverBrush"));
        Select("daisyUI dark");
        double darkRest = Lightness(Brush("PreviewBaseBrush"));
        double darkHover = Lightness(Brush("PreviewControlHoverBrush"));

        Assert.Multiple(() =>
        {
            Assert.That(lightHover, Is.LessThan(lightRest), "light");
            Assert.That(darkHover, Is.GreaterThan(darkRest), "dark");
        });
    }

    [Test]
    public void A_row_on_a_surface_hovers_from_that_surface_not_from_the_page()
    {
        Select("Dark");

        Color surface2 = Brush("PreviewSurface2Brush");
        Color foreground = Brush("PreviewForegroundBrush");

        Assert.That(Brush("PreviewSurface2HoverBrush"), Is.EqualTo(ColorShade.Mix(surface2, foreground, 0.1)));
    }

    [Test]
    public void An_input_rests_on_the_card_colour_and_hovers_one_step_from_it()
    {
        Select("Dark");

        Color surface1 = Brush("PreviewSurface1Brush");
        Color foreground = Brush("PreviewForegroundBrush");

        Assert.That(Brush("PreviewSurface1HoverBrush"), Is.EqualTo(ColorShade.Mix(surface1, foreground, 0.1)));
    }

    [Test]
    public void The_strokes_get_stronger_from_rest_to_hover_to_focus()
    {
        Select("Light");

        double hover = Distance("PreviewStrokeStrongBrush", "PreviewStrokeBrush");
        double focus = Distance("PreviewStrokeFocusBrush", "PreviewStrokeBrush");

        Assert.That(hover, Is.GreaterThan(0));
        Assert.That(focus, Is.GreaterThan(hover));
    }

    private void Select(string name)
    {
        _palettes.SelectedIndex = _palettes.Items.Cast<object>().ToList().FindIndex(p => p.ToString()!.Contains($"Name = {name}"));
        _preview.Measure(new Size(1000, 1400));
    }

    private Color Brush(string key) => ((SolidColorBrush)_preview.Resources[key]).Color;

    private double Distance(string a, string b)
    {
        Color first = Brush(a);
        Color second = Brush(b);
        return Math.Abs(Lightness(first) - Lightness(second));
    }

    private static double Lightness(Color color) => (Math.Max(color.R, Math.Max(color.G, color.B)) + Math.Min(color.R, Math.Min(color.G, color.B))) / 510.0;
}
