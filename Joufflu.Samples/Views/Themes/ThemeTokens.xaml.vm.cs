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
/// hover and pressed fill, the foreground drawn on top of both, then the soft tint and its hover.
/// </summary>
public sealed record AccentEntry(
    string Name,
    string Usage,
    ComponentResourceKey Fill,
    ComponentResourceKey Hover,
    ComponentResourceKey Foreground,
    ComponentResourceKey Soft,
    ComponentResourceKey SoftInteraction);

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
            "Bases",
            "Three colours a theme defines for the neutrals. The controls do not read them directly but through the three categories below, whose base falls back to them unless the theme gives a category a base of its own.",
            [
                new TokenEntry(
                    "Base0",
                    "The page, and by default what an action is filled with, so on a display it reads as set into it. An action is not put directly on the page.",
                    Joufflu.Brushes.Base0Brush),
                new TokenEntry(
                    "Base1",
                    "A card, and by default the base of a display and of an input.",
                    Joufflu.Brushes.Base1Brush),
                new TokenEntry(
                    "Base2",
                    "A contrast inside a card, for an app's own layout.",
                    Joufflu.Brushes.Base2Brush),
            ]),

        new TokenGroup(
            "Display",
            "What shows content without being interacted with: card, popup, tooltip, expander, group box, and the frame of a list, tree, grid, menu or tab control. No interaction of its own, Interaction is there for an app's informative hover. Its items (rows, menu items, tabs) are actions without a fill at rest.",
            [
                new TokenEntry("DisplayBase", "The background (falls back to Base1). The only colour of the category.", Joufflu.Brushes.DisplayBaseBrush),
                new TokenEntry("DisplayBorder", "Derived: the base mixed 10% toward the Foreground. The border, and the parts that sit on the base (a header, a track, a tag).", Joufflu.Brushes.DisplayBorderBrush),
                new TokenEntry("DisplayElevated", "Derived: the border mixed 10% toward the Foreground. A border that must stand out from a Border background, and the border of an interacted control.", Joufflu.Brushes.DisplayElevatedBrush),
                new TokenEntry("DisplayInteraction", "Derived: the base mixed 10% toward the Foreground. The background of an interacted control.", Joufflu.Brushes.DisplayInteractionBrush),
            ]),

        new TokenGroup(
            "Input",
            "What takes a value: text box, combo box, check box, radio button, slider. No hover; focus turns the border to Elevated and the background to Interaction. Checked is the primary.",
            [
                new TokenEntry("InputBase", "The background (falls back to Base1). The only colour of the category.", Joufflu.Brushes.InputBaseBrush),
                new TokenEntry("InputBorder", "Derived: the base mixed 10% toward the Foreground. The border, and the parts that sit on the base (a header, a track, a tag).", Joufflu.Brushes.InputBorderBrush),
                new TokenEntry("InputElevated", "Derived: the border mixed 10% toward the Foreground. A border that must stand out from a Border background, and the border of an interacted control.", Joufflu.Brushes.InputElevatedBrush),
                new TokenEntry("InputInteraction", "Derived: the base mixed 10% toward the Foreground. The background of an interacted control.", Joufflu.Brushes.InputInteractionBrush),
            ]),

        new TokenGroup(
            "Action",
            "What runs something: button, toggle button. Hover turns the border to Elevated and the background to Interaction; pressed is a translation down, not a colour; no focus border. A grip (scroll bar thumb, grid splitter, slider thumb) turns Elevated when hovered or dragged. Checked and selected are the primary.",
            [
                new TokenEntry("ActionBase", "The background (falls back to Base0). The only colour of the category.", Joufflu.Brushes.ActionBaseBrush),
                new TokenEntry("ActionBorder", "Derived: the base mixed 10% toward the Foreground. The border, and the parts that sit on the base (a header, a track, a tag).", Joufflu.Brushes.ActionBorderBrush),
                new TokenEntry("ActionElevated", "Derived: the border mixed 10% toward the Foreground. A border that must stand out from a Border background, and the border of an interacted control.", Joufflu.Brushes.ActionElevatedBrush),
                new TokenEntry("ActionInteraction", "Derived: the base mixed 10% toward the Foreground. The background of an interacted control.", Joufflu.Brushes.ActionInteractionBrush),
            ]),

        new TokenGroup(
            "Text",
            "Foreground of text and icons. Dim it with MutedOpacity or DisabledOpacity rather than a lighter colour.",
            [
                new TokenEntry(
                    "Foreground",
                    "Default text and icon colour, on any base.",
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
            Joufflu.Brushes.PrimaryInteractionBrush,
            Joufflu.Brushes.PrimaryForegroundBrush,
            Joufflu.Brushes.PrimarySoftBrush,
            Joufflu.Brushes.PrimarySoftInteractionBrush),
        new AccentEntry(
            "Secondary",
            "Neutral filled action, for a button that must read as a button without competing with Primary.",
            Joufflu.Brushes.SecondaryBrush,
            Joufflu.Brushes.SecondaryInteractionBrush,
            Joufflu.Brushes.SecondaryForegroundBrush,
            Joufflu.Brushes.SecondarySoftBrush,
            Joufflu.Brushes.SecondarySoftInteractionBrush),
        new AccentEntry(
            "Success",
            "Confirmation: success toast, valid state, positive badge.",
            Joufflu.Brushes.SuccessBrush,
            Joufflu.Brushes.SuccessInteractionBrush,
            Joufflu.Brushes.SuccessForegroundBrush,
            Joufflu.Brushes.SuccessSoftBrush,
            Joufflu.Brushes.SuccessSoftInteractionBrush),
        new AccentEntry(
            "Info",
            "Neutral information: info toast, informative badge.",
            Joufflu.Brushes.InfoBrush,
            Joufflu.Brushes.InfoInteractionBrush,
            Joufflu.Brushes.InfoForegroundBrush,
            Joufflu.Brushes.InfoSoftBrush,
            Joufflu.Brushes.InfoSoftInteractionBrush),
        new AccentEntry(
            "Warning",
            "Something needs attention but nothing is broken yet.",
            Joufflu.Brushes.WarningBrush,
            Joufflu.Brushes.WarningInteractionBrush,
            Joufflu.Brushes.WarningForegroundBrush,
            Joufflu.Brushes.WarningSoftBrush,
            Joufflu.Brushes.WarningSoftInteractionBrush),
        new AccentEntry(
            "Danger",
            "Destructive action and validation errors - a delete button, the border of a field in error.",
            Joufflu.Brushes.DangerBrush,
            Joufflu.Brushes.DangerInteractionBrush,
            Joufflu.Brushes.DangerForegroundBrush,
            Joufflu.Brushes.DangerSoftBrush,
            Joufflu.Brushes.DangerSoftInteractionBrush),
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
            "What a Border takes. Bind it through Derive when you need only some sides or corners.",
            [
                new TokenEntry("Thickness", "Thickness - the global border (1), for an app's own layout."),
                new TokenEntry("Radius", "CornerRadius - the global corner radius (4), for an app's own layout."),
                new TokenEntry(
                    "DisplayRadius, InputRadius, ActionRadius",
                    "CornerRadius - the radius of each category. The controls clamp it to half their height, so a large one gives a pill."),
                new TokenEntry(
                    "DisplayThickness, InputThickness, ActionThickness",
                    "Thickness - the border of each category."),
                new TokenEntry(
                    "DisplayPadding",
                    "Thickness - the padding inside a display (Spacing). An action is padded by ActionPaddingXs..Lg, an input by InputPaddingXs..Lg, horizontally only : their height comes from the size."),
                new TokenEntry(
                    "DisplayPaddingSm",
                    "Thickness - the padding inside a compact display: a popup, a menu, a tooltip, a badge (a third of DisplayPadding, 4)."),
            ]),

        new TokenGroup(
            "Layout",
            "Distances between things rather than inside them.",
            [
                new TokenEntry(
                    "Spacing",
                    "Thickness - standard gap between sibling elements and page padding (12). Feed it to Spacing.Gap."),
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
            + "shaped use ActionPadding*, text inputs use InputPadding*, whose halved horizontal keeps the caret "
            + "closer to the border.",
            [
                new TokenEntry("ActionPaddingXs", "6,2"),
                new TokenEntry("ActionPaddingSm", "9,3"),
                new TokenEntry("ActionPaddingMd", "12,4 - the default."),
                new TokenEntry("ActionPaddingLg", "18,6"),
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
        "    Background=\"{DynamicResource {x:Static joufflu:Brushes.DisplayBaseBrush}}\"\n" +
        "    BorderBrush=\"{DynamicResource {x:Static joufflu:Brushes.DisplayBorderBrush}}\"\n" +
        "    BorderThickness=\"{DynamicResource {x:Static joufflu:Dimensions.DisplayThickness}}\"\n" +
        "    CornerRadius=\"{DynamicResource {x:Static joufflu:Dimensions.DisplayRadius}}\" />\n\n" +
        "<!-- The raw Color is there for what a brush cannot express -->\n" +
        "<GradientStop Color=\"{DynamicResource {x:Static joufflu:Colors.PrimaryColor}}\" Offset=\"0\" />";
}
