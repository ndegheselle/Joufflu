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
    public void An_action_darkens_on_hover_in_the_light_theme_and_lightens_in_the_dark_one()
    {
        ResourceDictionary light = BuiltIn("Light");
        ResourceDictionary dark = BuiltIn("Dark");

        Assert.Multiple(() =>
        {
            Assert.That(Lightness(Resolve(light, Brushes.ActionElevatedBrush)), Is.LessThan(Lightness(Resolve(light, Brushes.ActionSurface1Brush))), "light");
            Assert.That(Lightness(Resolve(dark, Brushes.ActionElevatedBrush)), Is.GreaterThan(Lightness(Resolve(dark, Brushes.ActionSurface1Brush))), "dark");
        });
    }

    [Test]
    public void The_categories_fall_back_to_the_global_colours_of_a_theme_that_does_not_define_them()
    {
        ResourceDictionary light = BuiltIn("Light");

        Assert.Multiple(() =>
        {
            Assert.That(Resolve(light, Brushes.ActionSurface1Brush), Is.EqualTo(Resolve(light, Brushes.BaseBrush)), "an action is filled with the page");
            Assert.That(Resolve(light, Brushes.InputSurface1Brush), Is.EqualTo(Resolve(light, Brushes.Surface1Brush)));
            Assert.That(Resolve(light, Brushes.DisplaySurface2Brush), Is.EqualTo(Resolve(light, Brushes.Surface2Brush)));
            Assert.That(Resolve(light, Brushes.DisplayBorderBrush), Is.EqualTo(Resolve(light, Brushes.StrokeBrush)));
        });
    }

    [Test]
    public void A_category_colour_defined_by_the_theme_wins_over_the_global_one()
    {
        var theme = new ResourceDictionary { Source = new Uri("/Joufflu;component/Themes/Light.xaml", UriKind.Relative) };
        var pink = Color.FromRgb(0xff, 0xc0, 0xcb);
        theme[Colors.InputSurface1Color] = pink;

        ResourceDictionary resources = WithBrushes(theme);

        Assert.Multiple(() =>
        {
            Assert.That(Resolve(resources, Brushes.InputSurface1Brush), Is.EqualTo(pink));
            Assert.That(Resolve(resources, Brushes.DisplaySurface1Brush), Is.EqualTo(Resolve(resources, Brushes.Surface1Brush)), "the other categories keep the global one");
        });
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
    public void The_strong_borders_are_the_category_border_mixed_toward_the_foreground()
    {
        ResourceDictionary light = BuiltIn("Light");

        Color foreground = Resolve(light, Brushes.ForegroundBrush);

        Assert.Multiple(() =>
        {
            Assert.That(Resolve(light, Brushes.InputBorderElevatedBrush), Is.EqualTo(ColorShade.Mix(Resolve(light, Brushes.InputBorderBrush), foreground, 0.4)));
            Assert.That(Resolve(light, Brushes.ActionBorderElevatedBrush), Is.EqualTo(ColorShade.Mix(Resolve(light, Brushes.ActionBorderBrush), foreground, 0.4)));
        });
    }

    /// <summary>The theme customizer redefines a colour while the app runs: the derived brushes must follow it.</summary>
    [Test]
    public void The_derived_brushes_follow_the_colour_when_it_is_redefined()
    {
        var theme = new ResourceDictionary { Source = new Uri("/Joufflu;component/Themes/Light.xaml", UriKind.Relative) };
        ResourceDictionary resources = WithBrushes(theme);
        var hover = new Border { Resources = resources };
        hover.SetResourceReference(Border.BackgroundProperty, Brushes.ActionElevatedBrush);
        hover.Measure(new Size(10, 10));
        var pink = Color.FromRgb(0xff, 0xc0, 0xcb);

        theme[Colors.BaseColor] = pink;

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
