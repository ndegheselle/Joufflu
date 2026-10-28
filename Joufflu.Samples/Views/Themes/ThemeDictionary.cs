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
        new("xs", "ActionPaddingXs", 0.5),
        new("sm", "ActionPaddingSm", 0.75),
        new("md", "ActionPaddingMd", 1),
        new("lg", "ActionPaddingLg", 1.5),
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

    /// <summary>The input padding key a padding size feeds (<c>ActionPaddingSm</c> gives <c>InputPaddingSm</c>).</summary>
    public static string InputPaddingName(string actionPaddingName) => actionPaddingName.Replace("Action", "Input");

    /// <summary>
    /// The action padding of one size : the control padding in the simple mode, the action's own md padding in the
    /// complete one, at the size's ratio.
    /// </summary>
    public static Thickness ActionPadding(double ratio, ThemeValues values)
        => values.Categories is { } categories
            ? Sized(ratio, categories.Action.PaddingHorizontal, categories.Action.PaddingVertical)
            : Sized(ratio, values.PaddingMdHorizontal, values.PaddingMdVertical);

    /// <summary>
    /// The input padding of one size : the control padding at half the horizontal in the simple mode, the input's own
    /// md padding in the complete one, at the size's ratio.
    /// </summary>
    public static Thickness InputPadding(double ratio, ThemeValues values)
    {
        if (values.Categories is { } categories)
            return Sized(ratio, categories.Input.PaddingHorizontal, categories.Input.PaddingVertical);
        Thickness action = Sized(ratio, values.PaddingMdHorizontal, values.PaddingMdVertical);
        return InputPadding(action.Left, action.Top);
    }

    private static Thickness Sized(double ratio, double horizontalMd, double verticalMd)
    {
        double horizontal = Derive(horizontalMd, ratio);
        double vertical = Derive(verticalMd, ratio);
        return new Thickness(horizontal, vertical, horizontal, vertical);
    }
}

/// <summary>
/// The colours and the shape of one category (Display, Input or Action) in the complete mode. The padding is the md
/// one : the inside of a display as it is, the xs to lg paddings of an input or an action derived from it.
/// </summary>
public sealed record ThemeCategoryValues(
    Color Surface1,
    Color Surface2,
    Color Border,
    double Radius,
    double Thickness,
    double PaddingHorizontal,
    double PaddingVertical);

/// <summary>The categories of the complete mode, each with its own colours and shape.</summary>
public sealed record ThemeCategories(ThemeCategoryValues Display, ThemeCategoryValues Input, ThemeCategoryValues Action)
{
    public IEnumerable<(string Name, ThemeCategoryValues Values)> All()
    {
        yield return ("Display", Display);
        yield return ("Input", Input);
        yield return ("Action", Action);
    }
}

/// <summary>
/// What the <i>Customize theme</i> page edits, as plain values : the colours by their <c>Joufflu.Colors</c> name, the
/// shape dimensions and the md value of each scale. <see cref="Categories"/> is the complete mode, null in the simple
/// one, where every category follows the global colours and shape.
/// </summary>
public sealed record ThemeValues(
    IReadOnlyList<KeyValuePair<string, Color>> Colors,
    double Thickness,
    double Radius,
    double Spacing,
    double HeightMd,
    double FontSizeMd,
    double PaddingMdHorizontal,
    double PaddingMdVertical,
    ThemeCategories? Categories = null);

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

        // The simple mode leaves the category colours out, so they fall back to the global ones.
        if (values.Categories is not null)
        {
            xaml.AppendLine("    <!--  Category colors  -->");
            foreach ((string name, ThemeCategoryValues category) in values.Categories.All())
            {
                xaml.AppendLine($"    <Color x:Key=\"{{x:Static joufflu:Colors.{name}Surface1Color}}\">{Hex(category.Surface1)}</Color>");
                xaml.AppendLine($"    <Color x:Key=\"{{x:Static joufflu:Colors.{name}Surface2Color}}\">{Hex(category.Surface2)}</Color>");
                xaml.AppendLine($"    <Color x:Key=\"{{x:Static joufflu:Colors.{name}BorderColor}}\">{Hex(category.Border)}</Color>");
            }
            xaml.AppendLine();
        }

        AppendShape(xaml, values);
        AppendScale(xaml, "Control height", ThemeScales.Height, values.HeightMd);
        AppendEmbeddedHeight(xaml, values.HeightMd);
        AppendScale(xaml, "Font size", ThemeScales.FontSize, values.FontSizeMd);
        AppendPaddings(xaml, values);

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

        // Each category has its own shape, the global one in the simple mode: the controls read only these.
        xaml.AppendLine("    <!--  Categories  -->");
        foreach (string category in new[] { "Display", "Input", "Action" })
        {
            ThemeCategoryValues? own = values.Categories?.All().First(c => c.Name == category).Values;
            double radius = own?.Radius ?? values.Radius;
            double thickness = own?.Thickness ?? values.Thickness;
            xaml.AppendLine(DoubleEntry($"{category}Radius", radius));
            xaml.AppendLine($"    <CornerRadius x:Key=\"{{x:Static joufflu:Dimensions.{category}CornerRadius}}\">{Num(radius)}</CornerRadius>");
            xaml.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.{category}BorderThickness}}\">{Num(thickness)}</Thickness>");
        }
        ThemeCategoryValues? display = values.Categories?.Display;
        xaml.AppendLine(display is null
            ? $"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.DisplayPadding}}\">{Num(values.Spacing)}</Thickness>"
            : ThicknessEntry("DisplayPadding", display.PaddingHorizontal, display.PaddingVertical));
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

    private static void AppendPaddings(StringBuilder xaml, ThemeValues values)
    {
        xaml.AppendLine("    <!--  Action padding  -->");
        foreach (ThemeScaleSize size in ThemeScales.Padding)
        {
            Thickness action = ThemeScales.ActionPadding(size.Ratio, values);
            xaml.AppendLine(ThicknessEntry(size.ResourceName, action.Left, action.Top));
        }
        xaml.AppendLine();

        xaml.AppendLine(values.Categories is null
            ? "    <!--  Input padding (control padding at half the horizontal)  -->"
            : "    <!--  Input padding  -->");
        foreach (ThemeScaleSize size in ThemeScales.Padding)
        {
            Thickness input = ThemeScales.InputPadding(size.Ratio, values);
            xaml.AppendLine(ThicknessEntry(ThemeScales.InputPaddingName(size.ResourceName), input.Left, input.Top));
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
