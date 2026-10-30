using System.IO.Packaging;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Joufflu.Samples.Views.Themes;
using JColors = Joufflu.Colors;
using JDimensions = Joufflu.Dimensions;

namespace Joufflu.Samples.Tests.Themes;

/// <summary>
/// The dictionary the <i>Customize theme</i> page generates. Fixtures run in a single-threaded apartment, the only one
/// WPF parses XAML in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class ThemeDictionaryWriterTests
{
    private static readonly ThemeValues Edited = new(
        [KeyValuePair.Create("PrimaryColor", Color.FromRgb(0x11, 0x22, 0x33))],
        Thickness: 2,
        Radius: 8,
        Spacing: 16,
        HeightMd: 40,
        FontSizeMd: 14,
        PaddingMdHorizontal: 16,
        PaddingMdVertical: 6);

    [Test]
    public void The_shipped_md_values_reproduce_every_shipped_dimension()
    {
        ResourceDictionary shipped = LoadShippedDimensions();
        Thickness paddingMd = (Thickness)shipped[JDimensions.ActionPaddingMd];
        var defaults = new ThemeValues(
            [],
            Thickness: (double)shipped[JDimensions.Thickness],
            Radius: (double)shipped[JDimensions.Radius],
            Spacing: (double)shipped[JDimensions.Spacing],
            HeightMd: (double)shipped[JDimensions.HeightMd],
            FontSizeMd: (double)shipped[JDimensions.FontSizeMd],
            PaddingMdHorizontal: paddingMd.Left,
            PaddingMdVertical: paddingMd.Top);

        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(defaults));

        Assert.That(generated.Keys, Is.Not.Empty);
        foreach (object key in generated.Keys)
            Assert.That(generated[key], Is.EqualTo(shipped[key]), $"{key}");
    }

    [Test]
    public void An_edited_colour_is_written_under_its_key()
    {
        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(Edited));

        Assert.That(generated[JColors.PrimaryColor], Is.EqualTo(Color.FromRgb(0x11, 0x22, 0x33)));
    }

    [Test]
    public void A_shape_dimension_is_written_with_the_value_built_from_it()
    {
        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(Edited));

        Assert.That(generated[JDimensions.CornerRadius], Is.EqualTo(new CornerRadius(8)));
        Assert.That(generated[JDimensions.BorderThickness], Is.EqualTo(new Thickness(2)));
    }

    [Test]
    public void Every_category_takes_the_global_shape()
    {
        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(Edited));

        Assert.Multiple(() =>
        {
            Assert.That(generated[JDimensions.ActionRadius], Is.EqualTo(8d));
            Assert.That(generated[JDimensions.InputCornerRadius], Is.EqualTo(new CornerRadius(8)));
            Assert.That(generated[JDimensions.DisplayBorderThickness], Is.EqualTo(new Thickness(2)));
            Assert.That(generated[JDimensions.DisplayPadding], Is.EqualTo(new Thickness(16)));
        });
    }

    [Test]
    public void The_simple_mode_leaves_the_category_colours_to_their_fallback()
    {
        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(Edited));

        Assert.That(generated.Contains(JColors.ActionSurface1Color), Is.False);
    }

    [Test]
    public void The_complete_mode_writes_each_category_with_its_own_colours_and_shape()
    {
        var pink = Color.FromRgb(0xff, 0xc0, 0xcb);
        var grey = Color.FromRgb(0x80, 0x80, 0x80);
        ThemeValues complete = Edited with
        {
            Categories = new ThemeCategories(
                Display: new ThemeCategoryValues(grey, grey, grey, Radius: 12, Thickness: 1, PaddingHorizontal: 20, PaddingVertical: 14),
                Input: new ThemeCategoryValues(grey, grey, grey, Radius: 2, Thickness: 1, PaddingHorizontal: 10, PaddingVertical: 4),
                Action: new ThemeCategoryValues(pink, grey, grey, Radius: 99, Thickness: 0, PaddingHorizontal: 20, PaddingVertical: 8)),
        };

        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(complete));

        Assert.Multiple(() =>
        {
            Assert.That(generated[JColors.ActionSurface1Color], Is.EqualTo(pink));
            Assert.That(generated[JDimensions.ActionRadius], Is.EqualTo(99d));
            Assert.That(generated[JDimensions.ActionBorderThickness], Is.EqualTo(new Thickness(0)));
            Assert.That(generated[JDimensions.DisplayCornerRadius], Is.EqualTo(new CornerRadius(12)));
            Assert.That(generated[JDimensions.DisplayPadding], Is.EqualTo(new Thickness(20, 14, 20, 14)));
            Assert.That(generated[JDimensions.DisplayPaddingSm], Is.EqualTo(new Thickness(7, 5, 7, 5)), "a third of the display padding");
            Assert.That(generated[JDimensions.InputPaddingLg], Is.EqualTo(new Thickness(15, 6, 15, 6)), "the input's own md padding at the lg ratio");
            Assert.That(generated[JDimensions.ActionPaddingLg], Is.EqualTo(new Thickness(30, 12, 30, 12)), "the action's own, not the control padding");
            Assert.That(generated[JDimensions.Radius], Is.EqualTo(8d), "the global shape stays, for an app's own layout");
        });
    }

    [Test]
    public void Every_size_follows_its_scale_md_value()
    {
        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(Edited));

        Assert.That(generated[JDimensions.HeightLg], Is.EqualTo(50d));
        Assert.That(generated[JDimensions.HeightEmbedded], Is.EqualTo(25d));
        Assert.That(generated[JDimensions.ActionPaddingLg], Is.EqualTo(new Thickness(24, 9, 24, 9)));
        Assert.That(generated[JDimensions.InputPaddingLg], Is.EqualTo(new Thickness(12, 9, 12, 9)));
    }

    private static ResourceDictionary Parse(string xaml) => (ResourceDictionary)XamlReader.Parse(xaml);

    private static ResourceDictionary LoadShippedDimensions()
    {
        // Registers the pack:// scheme, which a test process has no Application to do.
        _ = PackUriHelper.UriSchemePack;
        return new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Joufflu;component/Styles/Dimensions.xaml"),
        };
    }
}
