using System.Windows;

namespace Joufflu;

public static class Colors
{
    public static ComponentResourceKey ForegroundColor => new ComponentResourceKey(typeof(Colors), "ForegroundColor");

    public static ComponentResourceKey BaseColor => new ComponentResourceKey(typeof(Colors), "BaseColor"); // The page, and what a button is filled with
    public static ComponentResourceKey Surface1Color => new ComponentResourceKey(typeof(Colors), "Surface1Color"); // Cards, popups, inputs
    public static ComponentResourceKey Surface2Color => new ComponentResourceKey(typeof(Colors), "Surface2Color"); // The contrast inside a control: a header, a card secondary, a track

    public static ComponentResourceKey StrokeColor => new ComponentResourceKey(typeof(Colors), "StrokeColor"); // Borders, separators, icon strokes

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

    public static ComponentResourceKey BaseBrush => new ComponentResourceKey(typeof(Brushes), "BaseBrush");
    public static ComponentResourceKey BaseHoverBrush => new ComponentResourceKey(typeof(Brushes), "BaseHoverBrush");

    public static ComponentResourceKey Surface1Brush => new ComponentResourceKey(typeof(Brushes), "Surface1Brush");
    public static ComponentResourceKey Surface1HoverBrush => new ComponentResourceKey(typeof(Brushes), "Surface1HoverBrush");

    public static ComponentResourceKey Surface2Brush => new ComponentResourceKey(typeof(Brushes), "Surface2Brush");
    public static ComponentResourceKey Surface2HoverBrush => new ComponentResourceKey(typeof(Brushes), "Surface2HoverBrush");

    public static ComponentResourceKey StrokeBrush => new ComponentResourceKey(typeof(Brushes), "StrokeBrush");
    public static ComponentResourceKey StrokeStrongBrush => new ComponentResourceKey(typeof(Brushes), "StrokeStrongBrush");

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