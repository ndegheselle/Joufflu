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
        Thickness paddingMd = (Thickness)shipped[JDimensions.PaddingMd];
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
    public void Every_size_follows_its_scale_md_value()
    {
        ResourceDictionary generated = Parse(ThemeDictionaryWriter.Write(Edited));

        Assert.That(generated[JDimensions.HeightLg], Is.EqualTo(50d));
        Assert.That(generated[JDimensions.HeightEmbedded], Is.EqualTo(25d));
        Assert.That(generated[JDimensions.PaddingLg], Is.EqualTo(new Thickness(24, 9, 24, 9)));
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
