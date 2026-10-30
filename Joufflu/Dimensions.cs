using System.Windows;

namespace Joufflu;

public static class Dimensions
{
    // What a Border takes: a Thickness, a CornerRadius. Every control uses its category's, these are for an app's own layout.
    public static ComponentResourceKey Thickness => new(typeof(Dimensions), "Thickness");
    public static ComponentResourceKey Radius => new(typeof(Dimensions), "Radius");

    // Gap between elements, a Thickness
    public static ComponentResourceKey Spacing => new(typeof(Dimensions), "Spacing");

    public static ComponentResourceKey TitleBarHeight => new(typeof(Dimensions), "TitleBarHeight");
    public static ComponentResourceKey TitleBarHeightOffset => new(typeof(Dimensions), "TitleBarHeightOffset");

    public static ComponentResourceKey HeightXs => new(typeof(Dimensions), "HeightXs");
    public static ComponentResourceKey HeightSm => new(typeof(Dimensions), "HeightSm");
    public static ComponentResourceKey HeightMd => new(typeof(Dimensions), "HeightMd");
    public static ComponentResourceKey HeightLg => new(typeof(Dimensions), "HeightLg");

    // 0.625 of HeightMd, kept in sync automatically by the theme customizer. Used by the small
    // "clear" close buttons embedded inside other controls (search, combobox, numeric input…).
    public static ComponentResourceKey HeightEmbedded => new(typeof(Dimensions), "HeightEmbedded");

    public static ComponentResourceKey FontSizeXs => new(typeof(Dimensions), "FontSizeXs");
    public static ComponentResourceKey FontSizeSm => new(typeof(Dimensions), "FontSizeSm");
    public static ComponentResourceKey FontSizeMd => new(typeof(Dimensions), "FontSizeMd");
    public static ComponentResourceKey FontSizeLg => new(typeof(Dimensions), "FontSizeLg");
    public static ComponentResourceKey FontSizeXl => new(typeof(Dimensions), "FontSizeXl");

    // Categories, defaulting to the global radius and thickness. The padding of an action is ActionPaddingXs..Lg,
    // of an input InputPaddingXs..Lg.
    public static ComponentResourceKey DisplayRadius => new(typeof(Dimensions), "DisplayRadius");
    public static ComponentResourceKey DisplayThickness => new(typeof(Dimensions), "DisplayThickness");

    public static ComponentResourceKey DisplayPadding => new(typeof(Dimensions), "DisplayPadding");
    // The inside of a compact display : a popup, a menu, a tooltip, a badge. A third of DisplayPadding by default.
    public static ComponentResourceKey DisplayPaddingSm => new(typeof(Dimensions), "DisplayPaddingSm");

    public static ComponentResourceKey ActionRadius => new(typeof(Dimensions), "ActionRadius");
    public static ComponentResourceKey ActionThickness => new(typeof(Dimensions), "ActionThickness");

    public static ComponentResourceKey ActionPaddingXs => new(typeof(Dimensions), "ActionPaddingXs");
    public static ComponentResourceKey ActionPaddingSm => new(typeof(Dimensions), "ActionPaddingSm");
    public static ComponentResourceKey ActionPaddingMd => new(typeof(Dimensions), "ActionPaddingMd");
    public static ComponentResourceKey ActionPaddingLg => new(typeof(Dimensions), "ActionPaddingLg");

    public static ComponentResourceKey InputRadius => new(typeof(Dimensions), "InputRadius");
    public static ComponentResourceKey InputThickness => new(typeof(Dimensions), "InputThickness");

    // Text inputs read tighter than a button, the horizontal padding is half a button padding so
    // that the text is more flused against the border
    public static ComponentResourceKey InputPaddingXs => new(typeof(Dimensions), "InputPaddingXs");
    public static ComponentResourceKey InputPaddingSm => new(typeof(Dimensions), "InputPaddingSm");
    public static ComponentResourceKey InputPaddingMd => new(typeof(Dimensions), "InputPaddingMd");
    public static ComponentResourceKey InputPaddingLg => new(typeof(Dimensions), "InputPaddingLg");
}
