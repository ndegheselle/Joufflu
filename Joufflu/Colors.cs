using System.Windows;

namespace Joufflu;

public static class Colors
{
    public static ComponentResourceKey ForegroundColor => new ComponentResourceKey(typeof(Colors), "ForegroundColor");

    public static ComponentResourceKey BaseColor => new ComponentResourceKey(typeof(Colors), "BaseColor"); // The page, and what a button is filled with
    public static ComponentResourceKey Surface1Color => new ComponentResourceKey(typeof(Colors), "Surface1Color"); // Cards, popups, inputs
    public static ComponentResourceKey Surface2Color => new ComponentResourceKey(typeof(Colors), "Surface2Color"); // The contrast inside a control: a header, a card secondary, a track

    public static ComponentResourceKey StrokeColor => new ComponentResourceKey(typeof(Colors), "StrokeColor"); // Borders, separators, icon strokes

    // Categories. Optional: a theme that does not define one gets the global colour it falls back to (in brackets).
    // Display: what shows content without being interacted with (card, popup, expander, group box, the frame of a
    // list, tree, grid, menu or tab control). Surface1 is its background, Surface2 an elevated part (a header).
    public static ComponentResourceKey DisplaySurface1Color => new ComponentResourceKey(typeof(Colors), "DisplaySurface1Color"); // (Surface1)
    public static ComponentResourceKey DisplaySurface2Color => new ComponentResourceKey(typeof(Colors), "DisplaySurface2Color"); // (Surface2)
    public static ComponentResourceKey DisplayBorderColor => new ComponentResourceKey(typeof(Colors), "DisplayBorderColor"); // (Stroke)

    // Input: what takes a value (text box, combo box, check box, radio button, slider). Surface2 is an elevated part
    // inside it (a tag, a track).
    public static ComponentResourceKey InputSurface1Color => new ComponentResourceKey(typeof(Colors), "InputSurface1Color"); // (Surface1)
    public static ComponentResourceKey InputSurface2Color => new ComponentResourceKey(typeof(Colors), "InputSurface2Color"); // (Surface2)
    public static ComponentResourceKey InputBorderColor => new ComponentResourceKey(typeof(Colors), "InputBorderColor"); // (Stroke)

    // Action: what runs something (button, toggle button). The items of a display (rows, menu items, tabs) are
    // actions without a fill at rest. Filled with the page colour by default, so on a display it reads as set into it.
    public static ComponentResourceKey ActionSurface1Color => new ComponentResourceKey(typeof(Colors), "ActionSurface1Color"); // (Base)
    public static ComponentResourceKey ActionSurface2Color => new ComponentResourceKey(typeof(Colors), "ActionSurface2Color"); // (Surface2)
    public static ComponentResourceKey ActionBorderColor => new ComponentResourceKey(typeof(Colors), "ActionBorderColor"); // (Stroke)

    // Primary
    public static ComponentResourceKey PrimaryColor => new ComponentResourceKey(typeof(Colors), "PrimaryColor");
    public static ComponentResourceKey PrimaryContentColor => new ComponentResourceKey(typeof(Colors), "PrimaryContentColor");

    // Secondary
    public static ComponentResourceKey SecondaryColor => new ComponentResourceKey(typeof(Colors), "SecondaryColor");
    public static ComponentResourceKey SecondaryContentColor => new ComponentResourceKey(typeof(Colors), "SecondaryContentColor");

    // Success
    public static ComponentResourceKey SuccessColor => new ComponentResourceKey(typeof(Colors), "SuccessColor");
    public static ComponentResourceKey SuccessContentColor => new ComponentResourceKey(typeof(Colors), "SuccessContentColor");

    // Info
    public static ComponentResourceKey InfoColor => new ComponentResourceKey(typeof(Colors), "InfoColor");
    public static ComponentResourceKey InfoContentColor => new ComponentResourceKey(typeof(Colors), "InfoContentColor");

    // Warning
    public static ComponentResourceKey WarningColor => new ComponentResourceKey(typeof(Colors), "WarningColor");
    public static ComponentResourceKey WarningContentColor => new ComponentResourceKey(typeof(Colors), "WarningContentColor");

    // Danger
    public static ComponentResourceKey DangerColor => new ComponentResourceKey(typeof(Colors), "DangerColor");
    public static ComponentResourceKey DangerContentColor => new ComponentResourceKey(typeof(Colors), "DangerContentColor");

    // State
    public static ComponentResourceKey DisabledOpacity => new ComponentResourceKey(typeof(Colors), "DisabledOpacity");
    public static ComponentResourceKey MutedOpacity => new ComponentResourceKey(typeof(Colors), "MutedOpacity");
}

public static class Brushes
{
    public static ComponentResourceKey ForegroundBrush => new ComponentResourceKey(typeof(Brushes), "ForegroundBrush");

    // The global neutrals, for an app's own layout. The controls use the category brushes below.
    public static ComponentResourceKey BaseBrush => new ComponentResourceKey(typeof(Brushes), "BaseBrush");
    public static ComponentResourceKey Surface1Brush => new ComponentResourceKey(typeof(Brushes), "Surface1Brush");
    public static ComponentResourceKey Surface2Brush => new ComponentResourceKey(typeof(Brushes), "Surface2Brush");
    public static ComponentResourceKey StrokeBrush => new ComponentResourceKey(typeof(Brushes), "StrokeBrush");

    // Display: no states.
    public static ComponentResourceKey DisplaySurface1Brush => new ComponentResourceKey(typeof(Brushes), "DisplaySurface1Brush");
    public static ComponentResourceKey DisplaySurface2Brush => new ComponentResourceKey(typeof(Brushes), "DisplaySurface2Brush");
    public static ComponentResourceKey DisplayBorderBrush => new ComponentResourceKey(typeof(Brushes), "DisplayBorderBrush");

    // Input: no hover, a strong border on focus.
    public static ComponentResourceKey InputSurface1Brush => new ComponentResourceKey(typeof(Brushes), "InputSurface1Brush");
    public static ComponentResourceKey InputSurface2Brush => new ComponentResourceKey(typeof(Brushes), "InputSurface2Brush");
    public static ComponentResourceKey InputBorderBrush => new ComponentResourceKey(typeof(Brushes), "InputBorderBrush");
    public static ComponentResourceKey InputBorderStrongBrush => new ComponentResourceKey(typeof(Brushes), "InputBorderStrongBrush");

    // Action: a stronger background on hover, a strong border on focus, a translation when pressed.
    public static ComponentResourceKey ActionSurface1Brush => new ComponentResourceKey(typeof(Brushes), "ActionSurface1Brush");
    public static ComponentResourceKey ActionSurface2Brush => new ComponentResourceKey(typeof(Brushes), "ActionSurface2Brush");
    public static ComponentResourceKey ActionBorderBrush => new ComponentResourceKey(typeof(Brushes), "ActionBorderBrush");
    public static ComponentResourceKey ActionHoverBrush => new ComponentResourceKey(typeof(Brushes), "ActionHoverBrush");
    public static ComponentResourceKey ActionBorderStrongBrush => new ComponentResourceKey(typeof(Brushes), "ActionBorderStrongBrush");

    // Primary
    public static ComponentResourceKey PrimaryBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryBrush");
    public static ComponentResourceKey PrimaryElevatedBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryElevatedBrush");
    public static ComponentResourceKey PrimaryContentBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryContentBrush");
    public static ComponentResourceKey PrimarySoftBrush => new ComponentResourceKey(typeof(Brushes), "PrimarySoftBrush");
    public static ComponentResourceKey PrimarySoftElevatedBrush => new ComponentResourceKey(typeof(Brushes), "PrimarySoftElevatedBrush");

    // Secondary
    public static ComponentResourceKey SecondaryBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryBrush");
    public static ComponentResourceKey SecondaryElevatedBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryElevatedBrush");
    public static ComponentResourceKey SecondaryContentBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryContentBrush");
    public static ComponentResourceKey SecondarySoftBrush => new ComponentResourceKey(typeof(Brushes), "SecondarySoftBrush");
    public static ComponentResourceKey SecondarySoftElevatedBrush => new ComponentResourceKey(typeof(Brushes), "SecondarySoftElevatedBrush");

    // Success
    public static ComponentResourceKey SuccessBrush => new ComponentResourceKey(typeof(Brushes), "SuccessBrush");
    public static ComponentResourceKey SuccessElevatedBrush => new ComponentResourceKey(typeof(Brushes), "SuccessElevatedBrush");
    public static ComponentResourceKey SuccessContentBrush => new ComponentResourceKey(typeof(Brushes), "SuccessContentBrush");
    public static ComponentResourceKey SuccessSoftBrush => new ComponentResourceKey(typeof(Brushes), "SuccessSoftBrush");
    public static ComponentResourceKey SuccessSoftElevatedBrush => new ComponentResourceKey(typeof(Brushes), "SuccessSoftElevatedBrush");

    // Info
    public static ComponentResourceKey InfoBrush => new ComponentResourceKey(typeof(Brushes), "InfoBrush");
    public static ComponentResourceKey InfoElevatedBrush => new ComponentResourceKey(typeof(Brushes), "InfoElevatedBrush");
    public static ComponentResourceKey InfoContentBrush => new ComponentResourceKey(typeof(Brushes), "InfoContentBrush");
    public static ComponentResourceKey InfoSoftBrush => new ComponentResourceKey(typeof(Brushes), "InfoSoftBrush");
    public static ComponentResourceKey InfoSoftElevatedBrush => new ComponentResourceKey(typeof(Brushes), "InfoSoftElevatedBrush");

    // Warning
    public static ComponentResourceKey WarningBrush => new ComponentResourceKey(typeof(Brushes), "WarningBrush");
    public static ComponentResourceKey WarningElevatedBrush => new ComponentResourceKey(typeof(Brushes), "WarningElevatedBrush");
    public static ComponentResourceKey WarningContentBrush => new ComponentResourceKey(typeof(Brushes), "WarningContentBrush");
    public static ComponentResourceKey WarningSoftBrush => new ComponentResourceKey(typeof(Brushes), "WarningSoftBrush");
    public static ComponentResourceKey WarningSoftElevatedBrush => new ComponentResourceKey(typeof(Brushes), "WarningSoftElevatedBrush");

    // Danger
    public static ComponentResourceKey DangerBrush => new ComponentResourceKey(typeof(Brushes), "DangerBrush");
    public static ComponentResourceKey DangerElevatedBrush => new ComponentResourceKey(typeof(Brushes), "DangerElevatedBrush");
    public static ComponentResourceKey DangerContentBrush => new ComponentResourceKey(typeof(Brushes), "DangerContentBrush");
    public static ComponentResourceKey DangerSoftBrush => new ComponentResourceKey(typeof(Brushes), "DangerSoftBrush");
    public static ComponentResourceKey DangerSoftElevatedBrush => new ComponentResourceKey(typeof(Brushes), "DangerSoftElevatedBrush");
}