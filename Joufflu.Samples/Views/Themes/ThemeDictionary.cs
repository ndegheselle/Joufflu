using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Joufflu.Samples.Views.Themes;

/// <summary>One size of a scale : the <c>Dimensions</c> key it feeds, by name, and its ratio to the scale's md value.</summary>
public sealed record ThemeScaleSize(string Label, string ResourceName, double Ratio);

/// <summary>
/// The size families the <i>Customize theme</i> page derives from a single md value, with the ratios that make the
/// shipped defaults come out of the shipped md values. Height and padding stop at lg : the toolkit defines no xl key
/// for them.
/// </summary>
public static class ThemeScales
{
    public static readonly IReadOnlyList<ThemeScaleSize> Height =
    [
        new("xs", "HeightXs", 0.75),
        new("sm", "HeightSm", 0.875),
        new("md", "HeightMd", 1),
        new("lg", "HeightLg", 1.25),
    ];

    public static readonly IReadOnlyList<ThemeScaleSize> FontSize =
    [
        new("xs", "FontSizeXs", 0.85),
        new("sm", "FontSizeSm", 0.92),
        new("md", "FontSizeMd", 1),
        new("lg", "FontSizeLg", 1.23),
        new("xl", "FontSizeXl", 1.85),
    ];

    /// <summary>Control paddings, symmetric : the ratio applies to the horizontal and to the vertical alike.</summary>
    public static readonly IReadOnlyList<ThemeScaleSize> Padding =
    [
        new("xs", "PaddingXs", 0.5),
        new("sm", "PaddingSm", 0.75),
        new("md", "PaddingMd", 1),
        new("lg", "PaddingLg", 1.5),
    ];

    /// <summary>Ratio of <c>Dimensions.HeightEmbedded</c> to the control height's md value.</summary>
    public const double EmbeddedHeightRatio = 0.625;

    /// <summary>Horizontal ratio of the text input paddings to the control paddings they follow.</summary>
    public const double InputPaddingHorizontalRatio = 0.5;

    /// <summary>A size derived from its scale's md value, in whole DIPs.</summary>
    public static double Derive(double md, double ratio) => Math.Round(md * ratio);

    /// <summary>
    /// The text input padding following a control padding. Not rounded : the shipped <c>InputPaddingSm</c> is 4.5.
    /// </summary>
    public static Thickness InputPadding(double horizontal, double vertical)
    {
        double inputHorizontal = horizontal * InputPaddingHorizontalRatio;
        return new Thickness(inputHorizontal, vertical, inputHorizontal, vertical);
    }
}

/// <summary>
/// What the <i>Customize theme</i> page edits, as plain values : the colours by their <c>Joufflu.Colors</c> name, the
/// shape dimensions and the md value of each scale.
/// </summary>
public sealed record ThemeValues(
    IReadOnlyList<KeyValuePair<string, Color>> Colors,
    double Thickness,
    double Radius,
    double Spacing,
    double HeightMd,
    double FontSizeMd,
    double PaddingMdHorizontal,
    double PaddingMdVertical);

/// <summary>
/// Writes the resource dictionary a <see cref="ThemeValues"/> amounts to, a drop-in merged after
/// <c>Resources.xaml</c> : every size derived from a scale is written with its value.
/// </summary>
public static class ThemeDictionaryWriter
{
    public static string Write(ThemeValues values)
    {
        var xaml = new StringBuilder();
        xaml.AppendLine("<ResourceDictionary");
        xaml.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
        xaml.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
        xaml.AppendLine("    xmlns:joufflu=\"clr-namespace:Joufflu;assembly=Joufflu\"");
        xaml.AppendLine("    xmlns:system=\"clr-namespace:System;assembly=mscorlib\">");
        xaml.AppendLine();

        // Colours only : the core resources build every brush from them.
        xaml.AppendLine("    <!--  Colors  -->");
        foreach ((string name, Color color) in values.Colors)
            xaml.AppendLine($"    <Color x:Key=\"{{x:Static joufflu:Colors.{name}}}\">{Hex(color)}</Color>");
        xaml.AppendLine();

        AppendShape(xaml, values);
        AppendScale(xaml, "Control height", ThemeScales.Height, values.HeightMd);
        AppendEmbeddedHeight(xaml, values.HeightMd);
        AppendScale(xaml, "Font size", ThemeScales.FontSize, values.FontSizeMd);
        AppendPaddings(xaml, values.PaddingMdHorizontal, values.PaddingMdVertical);

        xaml.Append("</ResourceDictionary>");
        return xaml.ToString();
    }

    private static void AppendShape(StringBuilder xaml, ThemeValues values)
    {
        xaml.AppendLine("    <!--  Dimensions  -->");
        xaml.AppendLine(DoubleEntry("Thickness", values.Thickness));
        xaml.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.BorderThickness}}\">{Num(values.Thickness)}</Thickness>");
        xaml.AppendLine();
        xaml.AppendLine(DoubleEntry("Radius", values.Radius));
        xaml.AppendLine($"    <CornerRadius x:Key=\"{{x:Static joufflu:Dimensions.CornerRadius}}\">{Num(values.Radius)}</CornerRadius>");
        xaml.AppendLine();
        xaml.AppendLine(DoubleEntry("Spacing", values.Spacing));
        xaml.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.SpacingThickness}}\">{Num(values.Spacing)}</Thickness>");
        xaml.AppendLine();
    }

    private static void AppendScale(StringBuilder xaml, string label, IReadOnlyList<ThemeScaleSize> sizes, double md)
    {
        xaml.AppendLine($"    <!--  {label}  -->");
        foreach (ThemeScaleSize size in sizes)
            xaml.AppendLine(DoubleEntry(size.ResourceName, ThemeScales.Derive(md, size.Ratio)));
        xaml.AppendLine();
    }

    private static void AppendEmbeddedHeight(StringBuilder xaml, double heightMd)
    {
        double embedded = ThemeScales.Derive(heightMd, ThemeScales.EmbeddedHeightRatio);
        xaml.AppendLine($"    <!--  Close button height ({Num(ThemeScales.EmbeddedHeightRatio)} of the control height's base value)  -->");
        xaml.AppendLine(DoubleEntry("HeightEmbedded", embedded));
        xaml.AppendLine();
    }

    private static void AppendPaddings(StringBuilder xaml, double horizontalMd, double verticalMd)
    {
        xaml.AppendLine("    <!--  Control padding  -->");
        foreach (ThemeScaleSize size in ThemeScales.Padding)
        {
            double horizontal = ThemeScales.Derive(horizontalMd, size.Ratio);
            double vertical = ThemeScales.Derive(verticalMd, size.Ratio);
            xaml.AppendLine(ThicknessEntry(size.ResourceName, horizontal, vertical));
        }
        xaml.AppendLine();

        // The input paddings ride along with the control paddings, at half the horizontal.
        xaml.AppendLine("    <!--  Input padding (control padding at half the horizontal)  -->");
        foreach (ThemeScaleSize size in ThemeScales.Padding)
        {
            double horizontal = ThemeScales.Derive(horizontalMd, size.Ratio);
            double vertical = ThemeScales.Derive(verticalMd, size.Ratio);
            Thickness input = ThemeScales.InputPadding(horizontal, vertical);
            xaml.AppendLine(ThicknessEntry("Input" + size.ResourceName, input.Left, input.Top));
        }
        xaml.AppendLine();
    }

    private static string DoubleEntry(string name, double value)
        => $"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.{name}}}\">{Num(value)}</system:Double>";

    private static string ThicknessEntry(string name, double horizontal, double vertical)
        => $"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.{name}}}\">{Num(horizontal)},{Num(vertical)}</Thickness>";

    private static string Hex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
