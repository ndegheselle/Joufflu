using System.Windows;

namespace Joufflu;

public static class Colors
{
    public static ComponentResourceKey ForegroundColor => new ComponentResourceKey(typeof(Colors), "ForegroundColor");

    public static ComponentResourceKey Base0Color => new ComponentResourceKey(typeof(Colors), "Base0Color"); // The page, and what a button is filled with
    public static ComponentResourceKey Base1Color => new ComponentResourceKey(typeof(Colors), "Base1Color"); // Cards, popups, inputs
    public static ComponentResourceKey Base2Color => new ComponentResourceKey(typeof(Colors), "Base2Color"); // A contrast inside a card, for an app's own layout. Icons should use foreground and opacity.

    // Categories. Optional: a theme that does not define one gets the global colour it falls back to (in brackets).
    // Only the base is a colour, the other brushes of a category are derived from it.
    // Display: what shows content without being interacted with (card, popup, expander, group box, the frame of a
    // list, tree, grid, menu or tab control).
    public static ComponentResourceKey DisplayBase0Color => new ComponentResourceKey(typeof(Colors), "DisplayBase0Color"); // (Base1)
    public static ComponentResourceKey DisplayBase1Color => new ComponentResourceKey(typeof(Colors), "DisplayBase1Color"); // (Base2) A contrast inside a display, for an app's own layout.

    // Input: what takes a value (text box, combo box, check box, radio button, slider).
    public static ComponentResourceKey InputBase0Color => new ComponentResourceKey(typeof(Colors), "InputBase0Color"); // (Base1)
    public static ComponentResourceKey InputBase1Color => new ComponentResourceKey(typeof(Colors), "InputBase1Color"); // (Base2) A contrast inside an input.

    // Action: what runs something (button, toggle button). The items of a display (rows, menu items, tabs) are
    // actions without a fill at rest. Filled with the page colour by default, so on a display it reads as set into it.
    public static ComponentResourceKey ActionBase0Color => new ComponentResourceKey(typeof(Colors), "ActionBase0Color"); // (Base0)

    // Primary
    public static ComponentResourceKey PrimaryColor => new ComponentResourceKey(typeof(Colors), "PrimaryColor");
    public static ComponentResourceKey PrimaryForegroundColor => new ComponentResourceKey(typeof(Colors), "PrimaryForegroundColor");

    // Secondary
    public static ComponentResourceKey SecondaryColor => new ComponentResourceKey(typeof(Colors), "SecondaryColor");
    public static ComponentResourceKey SecondaryForegroundColor => new ComponentResourceKey(typeof(Colors), "SecondaryForegroundColor");

    // Success
    public static ComponentResourceKey SuccessColor => new ComponentResourceKey(typeof(Colors), "SuccessColor");
    public static ComponentResourceKey SuccessForegroundColor => new ComponentResourceKey(typeof(Colors), "SuccessForegroundColor");

    // Info
    public static ComponentResourceKey InfoColor => new ComponentResourceKey(typeof(Colors), "InfoColor");
    public static ComponentResourceKey InfoForegroundColor => new ComponentResourceKey(typeof(Colors), "InfoForegroundColor");

    // Warning
    public static ComponentResourceKey WarningColor => new ComponentResourceKey(typeof(Colors), "WarningColor");
    public static ComponentResourceKey WarningForegroundColor => new ComponentResourceKey(typeof(Colors), "WarningForegroundColor");

    // Danger
    public static ComponentResourceKey DangerColor => new ComponentResourceKey(typeof(Colors), "DangerColor");
    public static ComponentResourceKey DangerForegroundColor => new ComponentResourceKey(typeof(Colors), "DangerForegroundColor");

    // State
    public static ComponentResourceKey DisabledOpacity => new ComponentResourceKey(typeof(Colors), "DisabledOpacity");
    public static ComponentResourceKey MutedOpacity => new ComponentResourceKey(typeof(Colors), "MutedOpacity");
}

public static class Brushes
{
    public static ComponentResourceKey ForegroundBrush => new ComponentResourceKey(typeof(Brushes), "ForegroundBrush");

    // The global neutrals, for an app's own layout. The controls use the category brushes below.
    public static ComponentResourceKey Base0Brush => new ComponentResourceKey(typeof(Brushes), "Base0Brush");
    public static ComponentResourceKey Base1Brush => new ComponentResourceKey(typeof(Brushes), "Base1Brush");
    public static ComponentResourceKey Base2Brush => new ComponentResourceKey(typeof(Brushes), "Base2Brush");

    // Every category has the same four, derived from its base:
    // - Base0: the background.
    // - Border0: the border, and the parts that sit on the base (a header, a track, a tag).
    // - Border1: derived from Border0, a border that must stand out from a Border0 background, and the border of an
    //   interacted control.
    // - Interaction: the background of an interacted control.
    // An interaction (an action hovered, an input focused) turns the border to Border1 and the background to Interaction.

    // Display: no interaction of its own, Interaction is there for an app's informative hover.
    public static ComponentResourceKey DisplayBase0Brush => new ComponentResourceKey(typeof(Brushes), "DisplayBase0Brush");
    // A contrast inside a display (Base2 by default), not derived from Base0 and with no brushes of its own.
    public static ComponentResourceKey DisplayBase1Brush => new ComponentResourceKey(typeof(Brushes), "DisplayBase1Brush");
    public static ComponentResourceKey DisplayBorder0Brush => new ComponentResourceKey(typeof(Brushes), "DisplayBorder0Brush");
    public static ComponentResourceKey DisplayBorder1Brush => new ComponentResourceKey(typeof(Brushes), "DisplayBorder1Brush");
    public static ComponentResourceKey DisplayInteractionBrush => new ComponentResourceKey(typeof(Brushes), "DisplayInteractionBrush");

    // Input: interacted when focused.
    public static ComponentResourceKey InputBase0Brush => new ComponentResourceKey(typeof(Brushes), "InputBase0Brush");
    // A contrast inside an input (Base2 by default), not derived from Base0 and with no brushes of its own.
    public static ComponentResourceKey InputBase1Brush => new ComponentResourceKey(typeof(Brushes), "InputBase1Brush");
    public static ComponentResourceKey InputBorder0Brush => new ComponentResourceKey(typeof(Brushes), "InputBorder0Brush");
    public static ComponentResourceKey InputBorder1Brush => new ComponentResourceKey(typeof(Brushes), "InputBorder1Brush");
    public static ComponentResourceKey InputInteractionBrush => new ComponentResourceKey(typeof(Brushes), "InputInteractionBrush");

    // Action: interacted when hovered, a translation when pressed. A grip (a scroll bar thumb, a grid splitter, a
    // slider thumb) is drawn with Border0 and turns Border1 when hovered or dragged.
    public static ComponentResourceKey ActionBase0Brush => new ComponentResourceKey(typeof(Brushes), "ActionBase0Brush");
    public static ComponentResourceKey ActionBorder0Brush => new ComponentResourceKey(typeof(Brushes), "ActionBorder0Brush");
    public static ComponentResourceKey ActionBorder1Brush => new ComponentResourceKey(typeof(Brushes), "ActionBorder1Brush");
    public static ComponentResourceKey ActionInteractionBrush => new ComponentResourceKey(typeof(Brushes), "ActionInteractionBrush");

    // Primary
    public static ComponentResourceKey PrimaryBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryBrush");
    public static ComponentResourceKey PrimaryInteractionBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryInteractionBrush");
    public static ComponentResourceKey PrimaryForegroundBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryForegroundBrush");
    public static ComponentResourceKey PrimarySoftBrush => new ComponentResourceKey(typeof(Brushes), "PrimarySoftBrush");
    public static ComponentResourceKey PrimarySoftInteractionBrush => new ComponentResourceKey(typeof(Brushes), "PrimarySoftInteractionBrush");

    // Secondary
    public static ComponentResourceKey SecondaryBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryBrush");
    public static ComponentResourceKey SecondaryInteractionBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryInteractionBrush");
    public static ComponentResourceKey SecondaryForegroundBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryForegroundBrush");
    public static ComponentResourceKey SecondarySoftBrush => new ComponentResourceKey(typeof(Brushes), "SecondarySoftBrush");
    public static ComponentResourceKey SecondarySoftInteractionBrush => new ComponentResourceKey(typeof(Brushes), "SecondarySoftInteractionBrush");

    // Success
    public static ComponentResourceKey SuccessBrush => new ComponentResourceKey(typeof(Brushes), "SuccessBrush");
    public static ComponentResourceKey SuccessInteractionBrush => new ComponentResourceKey(typeof(Brushes), "SuccessInteractionBrush");
    public static ComponentResourceKey SuccessForegroundBrush => new ComponentResourceKey(typeof(Brushes), "SuccessForegroundBrush");
    public static ComponentResourceKey SuccessSoftBrush => new ComponentResourceKey(typeof(Brushes), "SuccessSoftBrush");
    public static ComponentResourceKey SuccessSoftInteractionBrush => new ComponentResourceKey(typeof(Brushes), "SuccessSoftInteractionBrush");

    // Info
    public static ComponentResourceKey InfoBrush => new ComponentResourceKey(typeof(Brushes), "InfoBrush");
    public static ComponentResourceKey InfoInteractionBrush => new ComponentResourceKey(typeof(Brushes), "InfoInteractionBrush");
    public static ComponentResourceKey InfoForegroundBrush => new ComponentResourceKey(typeof(Brushes), "InfoForegroundBrush");
    public static ComponentResourceKey InfoSoftBrush => new ComponentResourceKey(typeof(Brushes), "InfoSoftBrush");
    public static ComponentResourceKey InfoSoftInteractionBrush => new ComponentResourceKey(typeof(Brushes), "InfoSoftInteractionBrush");

    // Warning
    public static ComponentResourceKey WarningBrush => new ComponentResourceKey(typeof(Brushes), "WarningBrush");
    public static ComponentResourceKey WarningInteractionBrush => new ComponentResourceKey(typeof(Brushes), "WarningInteractionBrush");
    public static ComponentResourceKey WarningForegroundBrush => new ComponentResourceKey(typeof(Brushes), "WarningForegroundBrush");
    public static ComponentResourceKey WarningSoftBrush => new ComponentResourceKey(typeof(Brushes), "WarningSoftBrush");
    public static ComponentResourceKey WarningSoftInteractionBrush => new ComponentResourceKey(typeof(Brushes), "WarningSoftInteractionBrush");

    // Danger
    public static ComponentResourceKey DangerBrush => new ComponentResourceKey(typeof(Brushes), "DangerBrush");
    public static ComponentResourceKey DangerInteractionBrush => new ComponentResourceKey(typeof(Brushes), "DangerInteractionBrush");
    public static ComponentResourceKey DangerForegroundBrush => new ComponentResourceKey(typeof(Brushes), "DangerForegroundBrush");
    public static ComponentResourceKey DangerSoftBrush => new ComponentResourceKey(typeof(Brushes), "DangerSoftBrush");
    public static ComponentResourceKey DangerSoftInteractionBrush => new ComponentResourceKey(typeof(Brushes), "DangerSoftInteractionBrush");
}
