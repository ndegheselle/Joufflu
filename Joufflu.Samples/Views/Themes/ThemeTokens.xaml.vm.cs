using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Themes;

/// <summary>
/// One documented resource key: its name, what the control styles use it for, and - for a colour -
/// the brush key painting the live swatch shown next to it.
/// </summary>
public sealed record TokenEntry(string Key, string Usage, ComponentResourceKey? Swatch = null);

/// <summary>A family of related keys, shown as one titled table.</summary>
public sealed record TokenGroup(string Title, string Description, IReadOnlyList<TokenEntry> Entries);

/// <summary>
/// One accent family, the keys it is made of shown side by side rather than described: the fill, the
/// hover and pressed fill, the content drawn on top of both, then the soft tint and its hover.
/// </summary>
public sealed record AccentEntry(
    string Name,
    string Usage,
    ComponentResourceKey Fill,
    ComponentResourceKey Hover,
    ComponentResourceKey Content,
    ComponentResourceKey Soft,
    ComponentResourceKey SoftElevated);

/// <summary>
/// Reference of every key a theme is made of. The descriptions are the roles the keys actually play
/// in the control styles, so a custom palette can be written without reading them.
/// </summary>
public class ThemeTokensViewModel : ObservableObject
{
    /// <summary>Every key of <c>Joufflu.Colors</c> / <c>Joufflu.Brushes</c>.</summary>
    public IReadOnlyList<TokenGroup> ColorTokens { get; } =
    [
        new TokenGroup(
            "Surfaces",
            "Five colours a theme defines for the neutrals, by where a control sits. Every state is derived from them by mixing a colour toward the Foreground: hover 10%, pressed 15%, the strong stroke 15%, the focus stroke 40%. Disabled is an opacity.",
            [
                new TokenEntry(
                    "Base",
                    "The page, and what a button is filled with, so on a card it reads as set into it. A button is not put directly on the page.",
                    Joufflu.Brushes.BaseBrush),
                new TokenEntry(
                    "Surface1",
                    "The card and what rests on it: inputs (told from the card by their stroke), popups, rows.",
                    Joufflu.Brushes.Surface1Brush),
                new TokenEntry(
                    "Surface2",
                    "Only the contrast inside a control: a table or group header, a card secondary, a track, a badge.",
                    Joufflu.Brushes.Surface2Brush),
                new TokenEntry(
                    "Stroke",
                    "Borders, separators and the strokes of icons: default border of every framed control.",
                    Joufflu.Brushes.StrokeBrush),
                new TokenEntry(
                    "BaseHoverBrush, BasePressedBrush",
                    "Derived from Base: a button hovered or pressed.",
                    Joufflu.Brushes.BaseHoverBrush),
                new TokenEntry(
                    "Surface1HoverBrush, Surface1PressedBrush",
                    "Derived from Surface1: a hovered input, a row, a menu item.",
                    Joufflu.Brushes.Surface1HoverBrush),
                new TokenEntry(
                    "Surface2HoverBrush, Surface2PressedBrush",
                    "Derived from Surface2: a row inside a header or a card secondary.",
                    Joufflu.Brushes.Surface2HoverBrush),
                new TokenEntry(
                    "StrokeStrongBrush",
                    "Derived from Stroke: the border of a control that is hovered or pressed.",
                    Joufflu.Brushes.StrokeStrongBrush),
                new TokenEntry(
                    "StrokeFocusBrush",
                    "Derived from Stroke, stronger still: the border of a control that has the focus.",
                    Joufflu.Brushes.StrokeFocusBrush),
            ]),

        new TokenGroup(
            "Text",
            "Foreground of text and icons. Dim it with MutedOpacity or DisabledOpacity rather than a lighter colour.",
            [
                new TokenEntry(
                    "Foreground",
                    "Default text and icon colour, on Base, Surface1 or Surface2.",
                    Joufflu.Brushes.ForegroundBrush),
            ]),

    ];

    /// <summary>
    /// The six accent families. Each one is rendered as its swatches so the relationship
    /// between them is seen rather than read.
    /// </summary>
    public IReadOnlyList<AccentEntry> AccentTokens { get; } =
    [
        new AccentEntry(
            "Primary",
            "Main call to action: the default filled button, the selected tab or navigation entry.",
            Joufflu.Brushes.PrimaryBrush,
            Joufflu.Brushes.PrimaryElevatedBrush,
            Joufflu.Brushes.PrimaryContentBrush,
            Joufflu.Brushes.PrimarySoftBrush,
            Joufflu.Brushes.PrimarySoftElevatedBrush),
        new AccentEntry(
            "Secondary",
            "Neutral filled action, for a button that must read as a button without competing with Primary.",
            Joufflu.Brushes.SecondaryBrush,
            Joufflu.Brushes.SecondaryElevatedBrush,
            Joufflu.Brushes.SecondaryContentBrush,
            Joufflu.Brushes.SecondarySoftBrush,
            Joufflu.Brushes.SecondarySoftElevatedBrush),
        new AccentEntry(
            "Success",
            "Confirmation: success toast, valid state, positive badge.",
            Joufflu.Brushes.SuccessBrush,
            Joufflu.Brushes.SuccessElevatedBrush,
            Joufflu.Brushes.SuccessContentBrush,
            Joufflu.Brushes.SuccessSoftBrush,
            Joufflu.Brushes.SuccessSoftElevatedBrush),
        new AccentEntry(
            "Info",
            "Neutral information: info toast, informative badge.",
            Joufflu.Brushes.InfoBrush,
            Joufflu.Brushes.InfoElevatedBrush,
            Joufflu.Brushes.InfoContentBrush,
            Joufflu.Brushes.InfoSoftBrush,
            Joufflu.Brushes.InfoSoftElevatedBrush),
        new AccentEntry(
            "Warning",
            "Something needs attention but nothing is broken yet.",
            Joufflu.Brushes.WarningBrush,
            Joufflu.Brushes.WarningElevatedBrush,
            Joufflu.Brushes.WarningContentBrush,
            Joufflu.Brushes.WarningSoftBrush,
            Joufflu.Brushes.WarningSoftElevatedBrush),
        new AccentEntry(
            "Danger",
            "Destructive action and validation errors - a delete button, the border of a field in error.",
            Joufflu.Brushes.DangerBrush,
            Joufflu.Brushes.DangerElevatedBrush,
            Joufflu.Brushes.DangerContentBrush,
            Joufflu.Brushes.DangerSoftBrush,
            Joufflu.Brushes.DangerSoftElevatedBrush),
    ];

    /// <summary>The odd ones out of <c>Joufflu.Colors</c>: numbers rather than colours.</summary>
    public IReadOnlyList<TokenGroup> StateTokens { get; } =
    [
        new TokenGroup(
            "State",
            "Not colours, but they live in Colors because they only exist to dim one.",
            [
                new TokenEntry(
                    "DisabledOpacity",
                    "Opacity applied to a whole control while IsEnabled is False (0.5). Disabled controls "
                    + "fade rather than switch to a dedicated palette, so a single key covers every control.",
                    null),
                new TokenEntry(
                    "MutedOpacity",
                    "Opacity of muted text: placeholders, hints, descriptions, captions (0.65). Muted text is "
                    + "dimmed rather than given its own colour, so it follows the foreground it sits in.",
                    null),
            ]),
    ];

    /// <summary>Every key of <c>Joufflu.Dimensions</c>.</summary>
    public IReadOnlyList<TokenGroup> DimensionTokens { get; } =
    [
        new TokenGroup(
            "Shape",
            "A scalar plus the ready made Thickness / CornerRadius built from it. Bind the scalar through "
            + "Derive when you need only some sides or corners, the composite one otherwise.",
            [
                new TokenEntry("Thickness", "double - base border width (1)."),
                new TokenEntry("BorderThickness", "Thickness - Thickness on all four sides, for a uniform border."),
                new TokenEntry("Radius", "double - base corner radius (4)."),
                new TokenEntry(
                    "CornerRadius",
                    "CornerRadius - Radius on all four corners, for a uniformly rounded border."),
            ]),

        new TokenGroup(
            "Layout",
            "Distances between things rather than inside them.",
            [
                new TokenEntry(
                    "Spacing",
                    "double - standard gap between sibling elements (12). Feed it to Spacing.Gap."),
                new TokenEntry("SpacingThickness", "Thickness - Spacing on all four sides, the standard page padding."),
                new TokenEntry("TitleBarHeight", "double - height of the custom window title bar (30)."),
                new TokenEntry(
                    "TitleBarHeightOffset",
                    "Thickness - Top only, used as a margin to push content below an overlapping title bar."),
            ]),

        new TokenGroup(
            "Control heights",
            "Vertical size of single line controls. Sizing.Size picks one of the four; md is the default "
            + "written in the base styles, so only xs / sm / lg appear in the size triggers.",
            [
                new TokenEntry("HeightXs", "24 - dense tables and toolbars."),
                new TokenEntry("HeightSm", "28"),
                new TokenEntry("HeightMd", "32 - the default."),
                new TokenEntry("HeightLg", "40 - prominent, touch friendly controls."),
                new TokenEntry(
                    "HeightEmbedded",
                    "20 - 0.625 of HeightMd, kept in sync by the theme customizer. The small clear button "
                    + "embedded inside a search box, combo box or numeric input."),
            ]),

        new TokenGroup(
            "Font sizes",
            "A TextBlock picks one through Sizing.Size (xs to xl; md keeps the inherited body size), like the controls do, "
            + "so restyling the scale restyles the text.",
            [
                new TokenEntry("FontSizeXs", "11"),
                new TokenEntry("FontSizeSm", "12"),
                new TokenEntry("FontSizeMd", "13 - the default body size."),
                new TokenEntry("FontSizeLg", "16"),
                new TokenEntry("FontSizeXl", "24"),
            ]),

        new TokenGroup(
            "Paddings",
            "Space inside a control, one Thickness per size. Two families: buttons and anything content "
            + "shaped use Padding*, text inputs use InputPadding*, whose halved horizontal keeps the caret "
            + "closer to the border.",
            [
                new TokenEntry("PaddingXs", "6,2"),
                new TokenEntry("PaddingSm", "9,3"),
                new TokenEntry("PaddingMd", "12,4 - the default."),
                new TokenEntry("PaddingLg", "18,6"),
                new TokenEntry("InputPaddingXs", "3,2"),
                new TokenEntry("InputPaddingSm", "4.5,3"),
                new TokenEntry("InputPaddingMd", "6,4 - the default."),
                new TokenEntry("InputPaddingLg", "9,6"),
            ]),
    ];

    public string UsageCode =>
        "<!-- Always DynamicResource: a theme swap, or the customizer editing a -->\n" +
        "<!-- dimension, has to reach controls already on screen.               -->\n" +
        "<Border\n" +
        "    Background=\"{DynamicResource {x:Static joufflu:Brushes.Surface1Brush}}\"\n" +
        "    BorderBrush=\"{DynamicResource {x:Static joufflu:Brushes.StrokeBrush}}\"\n" +
        "    BorderThickness=\"{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}\"\n" +
        "    CornerRadius=\"{DynamicResource {x:Static joufflu:Dimensions.CornerRadius}}\" />\n\n" +
        "<!-- The raw Color is there for what a brush cannot express -->\n" +
        "<GradientStop Color=\"{DynamicResource {x:Static joufflu:Colors.PrimaryColor}}\" Offset=\"0\" />";
}
