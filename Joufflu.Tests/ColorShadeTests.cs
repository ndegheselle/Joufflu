using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Joufflu.Tests;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ColorShadeTests
{
    private static readonly Color White = Color.FromRgb(0xff, 0xff, 0xff);
    private static readonly Color Black = Color.FromRgb(0x0a, 0x0a, 0x0a);

    /// <summary>
    /// A dictionary loaded from a pack uri fails until WPF has initialized, which creating a first element does:
    /// an application gets there on startup, a test fixture has to do it before loading a theme.
    /// </summary>
    [OneTimeSetUp]
    public void InitializeWpf() => _ = new Border();

    [Test]
    public void An_amount_of_zero_keeps_the_colour()
    {
        Assert.That(ColorShade.Mix(White, Black, 0), Is.EqualTo(White));
    }

    [Test]
    public void An_amount_of_one_gives_the_other_colour()
    {
        Assert.That(ColorShade.Mix(White, Black, 1), Is.EqualTo(Black));
    }

    [Test]
    public void A_tenth_of_the_way_to_the_foreground_gives_the_hand_picked_hover_of_the_light_theme()
    {
        Color hover = ColorShade.Mix(White, Black, 0.1);

        // The light theme used #E6E6E6 for it.
        Assert.That(hover.R, Is.EqualTo(0xe6).Within(1));
    }

    [Test]
    public void The_alpha_of_the_colour_is_kept()
    {
        Color translucent = Color.FromArgb(0x80, 0xff, 0xff, 0xff);

        Assert.That(ColorShade.Mix(translucent, Black, 0.5).A, Is.EqualTo(0x80));
    }

    [Test]
    public void The_result_follows_the_colour_when_it_changes()
    {
        var shade = new ColorShade { Color = White, Toward = Black, Amount = 0.1 };
        Color first = shade.Result;

        shade.Color = Color.FromRgb(0x20, 0x20, 0x20);

        Assert.That(shade.Result, Is.Not.EqualTo(first));
    }

    [Test]
    public void A_surface_darkens_on_hover_in_the_light_theme_and_lightens_in_the_dark_one()
    {
        ResourceDictionary light = BuiltIn("Light");
        ResourceDictionary dark = BuiltIn("Dark");

        Assert.Multiple(() =>
        {
            Assert.That(Lightness(Resolve(light, Brushes.Surface1HoverBrush)), Is.LessThan(Lightness(Resolve(light, Brushes.Surface1Brush))), "light");
            Assert.That(Lightness(Resolve(dark, Brushes.Surface1HoverBrush)), Is.GreaterThan(Lightness(Resolve(dark, Brushes.Surface1Brush))), "dark");
        });
    }

    [Test]
    public void A_button_filled_with_the_base_colour_darkens_on_hover_in_the_light_theme_and_lightens_in_the_dark_one()
    {
        ResourceDictionary light = BuiltIn("Light");
        ResourceDictionary dark = BuiltIn("Dark");

        Assert.Multiple(() =>
        {
            Assert.That(Lightness(Resolve(light, Brushes.BaseHoverBrush)), Is.LessThan(Lightness(Resolve(light, Brushes.BaseBrush))), "light");
            Assert.That(Lightness(Resolve(dark, Brushes.BaseHoverBrush)), Is.GreaterThan(Lightness(Resolve(dark, Brushes.BaseBrush))), "dark");
        });
    }

    [Test]
    public void The_pressed_fill_is_further_from_the_base_than_the_hover_one()
    {
        ResourceDictionary light = BuiltIn("Light");

        double rest = Lightness(Resolve(light, Brushes.BaseBrush));
        double hover = Math.Abs(rest - Lightness(Resolve(light, Brushes.BaseHoverBrush)));
        double pressed = Math.Abs(rest - Lightness(Resolve(light, Brushes.BasePressedBrush)));

        Assert.That(pressed, Is.GreaterThan(hover));
    }

    [Test]
    public void The_focus_stroke_is_stronger_than_the_hover_one()
    {
        ResourceDictionary light = BuiltIn("Light");

        double rest = Lightness(Resolve(light, Brushes.StrokeBrush));
        double hover = Math.Abs(rest - Lightness(Resolve(light, Brushes.StrokeStrongBrush)));
        double focus = Math.Abs(rest - Lightness(Resolve(light, Brushes.StrokeFocusBrush)));

        Assert.That(focus, Is.GreaterThan(hover));
    }

    [Test]
    public void A_dark_primary_lightens_on_hover_in_the_light_theme_and_a_pale_one_darkens_in_the_dark_theme()
    {
        ResourceDictionary light = BuiltIn("Light");
        ResourceDictionary dark = BuiltIn("Dark");

        Assert.Multiple(() =>
        {
            Assert.That(Lightness(Resolve(light, Brushes.PrimaryElevatedBrush)), Is.GreaterThan(Lightness(Resolve(light, Brushes.PrimaryBrush))), "light");
            Assert.That(Lightness(Resolve(dark, Brushes.PrimaryElevatedBrush)), Is.LessThan(Lightness(Resolve(dark, Brushes.PrimaryBrush))), "dark");
        });
    }

    [Test]
    public void A_pale_secondary_darkens_in_the_light_theme_and_lightens_in_the_dark_one()
    {
        ResourceDictionary light = BuiltIn("Light");
        ResourceDictionary dark = BuiltIn("Dark");

        Assert.Multiple(() =>
        {
            Assert.That(Lightness(Resolve(light, Brushes.SecondaryElevatedBrush)), Is.LessThan(Lightness(Resolve(light, Brushes.SecondaryBrush))), "light");
            Assert.That(Lightness(Resolve(dark, Brushes.SecondaryElevatedBrush)), Is.GreaterThan(Lightness(Resolve(dark, Brushes.SecondaryBrush))), "dark");
        });
    }

    [Test]
    public void The_strong_stroke_is_the_stroke_mixed_toward_the_foreground()
    {
        ResourceDictionary light = BuiltIn("Light");

        Color stroke = Resolve(light, Brushes.StrokeBrush);
        Color foreground = Resolve(light, Brushes.ForegroundBrush);

        Assert.That(Resolve(light, Brushes.StrokeStrongBrush), Is.EqualTo(ColorShade.Mix(stroke, foreground, 0.15)));
    }

    /// <summary>The theme customizer redefines a colour while the app runs: the derived brushes must follow it.</summary>
    [Test]
    public void The_derived_brushes_follow_the_colour_when_it_is_redefined()
    {
        var theme = new ResourceDictionary { Source = new Uri("/Joufflu;component/Themes/Light.xaml", UriKind.Relative) };
        ResourceDictionary resources = WithBrushes(theme);
        var hover = new Border { Resources = resources };
        hover.SetResourceReference(Border.BackgroundProperty, Brushes.Surface1HoverBrush);
        hover.Measure(new Size(10, 10));
        var pink = Color.FromRgb(0xff, 0xc0, 0xcb);

        theme[Colors.Surface1Color] = pink;

        Assert.That(((SolidColorBrush)hover.Background).Color, Is.EqualTo(ColorShade.Mix(pink, Black, 0.1)));
    }

    /// <summary>A theme paired with a fresh copy of the brushes, as ThemeManager does.</summary>
    private static ResourceDictionary WithBrushes(ResourceDictionary theme)
    {
        var brushes = new ResourceDictionary { Source = new Uri("/Joufflu;component/Styles/Brushes.xaml", UriKind.Relative) };
        return new ResourceDictionary { MergedDictionaries = { theme, brushes } };
    }

    private static ResourceDictionary BuiltIn(string theme)
        => WithBrushes(new ResourceDictionary { Source = new Uri($"/Joufflu;component/Themes/{theme}.xaml", UriKind.Relative) });

    private static Color Resolve(ResourceDictionary resources, ComponentResourceKey brushKey)
    {
        var border = new Border { Resources = resources };
        border.SetResourceReference(Border.BackgroundProperty, brushKey);
        border.Measure(new Size(10, 10));
        return ((SolidColorBrush)border.Background).Color;
    }

    private static double Lightness(Color color) => (Math.Max(color.R, Math.Max(color.G, color.B)) + Math.Min(color.R, Math.Min(color.G, color.B))) / 510.0;
}
