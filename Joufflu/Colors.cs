using System.Windows;

namespace Joufflu;

public static class Colors
{
    public static ComponentResourceKey ForegroundColor => new ComponentResourceKey(typeof(Colors), "ForegroundColor");
    public static ComponentResourceKey Foreground100Color => new ComponentResourceKey(typeof(Colors), "Foreground100Color");
    public static ComponentResourceKey Foreground200Color => new ComponentResourceKey(typeof(Colors), "Foreground200Color");

    public static ComponentResourceKey BackgroundColor => new ComponentResourceKey(typeof(Colors), "BackgroundColor");
    public static ComponentResourceKey Background100Color => new ComponentResourceKey(typeof(Colors), "Background100Color"); // Elevated
    public static ComponentResourceKey Background200Color => new ComponentResourceKey(typeof(Colors), "Background200Color"); // Selected

    public static ComponentResourceKey BorderColor => new ComponentResourceKey(typeof(Colors), "BorderColor");
    public static ComponentResourceKey Border100Color => new ComponentResourceKey(typeof(Colors), "Border100Color");

    // Primary
    public static ComponentResourceKey PrimaryColor => new ComponentResourceKey(typeof(Colors), "PrimaryColor");
    public static ComponentResourceKey Primary100Color => new ComponentResourceKey(typeof(Colors), "Primary100Color");
    public static ComponentResourceKey PrimaryContentColor => new ComponentResourceKey(typeof(Colors), "PrimaryContentColor");

    // Secondary
    public static ComponentResourceKey SecondaryColor => new ComponentResourceKey(typeof(Colors), "SecondaryColor");
    public static ComponentResourceKey Secondary100Color => new ComponentResourceKey(typeof(Colors), "Secondary100Color");
    public static ComponentResourceKey SecondaryContentColor => new ComponentResourceKey(typeof(Colors), "SecondaryContentColor");

    // Success
    public static ComponentResourceKey SuccessColor => new ComponentResourceKey(typeof(Colors), "SuccessColor");
    public static ComponentResourceKey Success100Color => new ComponentResourceKey(typeof(Colors), "Success100Color");
    public static ComponentResourceKey SuccessContentColor => new ComponentResourceKey(typeof(Colors), "SuccessContentColor");

    // Info
    public static ComponentResourceKey InfoColor => new ComponentResourceKey(typeof(Colors), "InfoColor");
    public static ComponentResourceKey Info100Color => new ComponentResourceKey(typeof(Colors), "Info100Color");
    public static ComponentResourceKey InfoContentColor => new ComponentResourceKey(typeof(Colors), "InfoContentColor");

    // Warning
    public static ComponentResourceKey WarningColor => new ComponentResourceKey(typeof(Colors), "WarningColor");
    public static ComponentResourceKey Warning100Color => new ComponentResourceKey(typeof(Colors), "Warning100Color");
    public static ComponentResourceKey WarningContentColor => new ComponentResourceKey(typeof(Colors), "WarningContentColor");

    // Danger
    public static ComponentResourceKey DangerColor => new ComponentResourceKey(typeof(Colors), "DangerColor");
    public static ComponentResourceKey Danger100Color => new ComponentResourceKey(typeof(Colors), "Danger100Color");
    public static ComponentResourceKey DangerContentColor => new ComponentResourceKey(typeof(Colors), "DangerContentColor");
}

public static class Brushes
{
    public static ComponentResourceKey ForegroundBrush => new ComponentResourceKey(typeof(Brushes), "ForegroundBrush");
    public static ComponentResourceKey Foreground100Brush => new ComponentResourceKey(typeof(Brushes), "Foreground100Brush");
    public static ComponentResourceKey Foreground200Brush => new ComponentResourceKey(typeof(Brushes), "Foreground200Brush");

    public static ComponentResourceKey BackgroundBrush => new ComponentResourceKey(typeof(Brushes), "BackgroundBrush");
    public static ComponentResourceKey Background100Brush => new ComponentResourceKey(typeof(Brushes), "Background100Brush");
    public static ComponentResourceKey Background200Brush => new ComponentResourceKey(typeof(Brushes), "Background200Brush");

    public static ComponentResourceKey BorderBrush => new ComponentResourceKey(typeof(Brushes), "BorderBrush");
    public static ComponentResourceKey Border100Brush => new ComponentResourceKey(typeof(Brushes), "Border100Brush");

    // Primary
    public static ComponentResourceKey PrimaryBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryBrush");
    public static ComponentResourceKey Primary100Brush => new ComponentResourceKey(typeof(Brushes), "Primary100Brush");
    public static ComponentResourceKey PrimaryContentBrush => new ComponentResourceKey(typeof(Brushes), "PrimaryContentBrush");

    // Secondary
    public static ComponentResourceKey SecondaryBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryBrush");
    public static ComponentResourceKey Secondary100Brush => new ComponentResourceKey(typeof(Brushes), "Secondary100Brush");
    public static ComponentResourceKey SecondaryContentBrush => new ComponentResourceKey(typeof(Brushes), "SecondaryContentBrush");

    // Success
    public static ComponentResourceKey SuccessBrush => new ComponentResourceKey(typeof(Brushes), "SuccessBrush");
    public static ComponentResourceKey Success100Brush => new ComponentResourceKey(typeof(Brushes), "Success100Brush");
    public static ComponentResourceKey SuccessContentBrush => new ComponentResourceKey(typeof(Brushes), "SuccessContentBrush");

    // Info
    public static ComponentResourceKey InfoBrush => new ComponentResourceKey(typeof(Brushes), "InfoBrush");
    public static ComponentResourceKey Info100Brush => new ComponentResourceKey(typeof(Brushes), "Info100Brush");
    public static ComponentResourceKey InfoContentBrush => new ComponentResourceKey(typeof(Brushes), "InfoContentBrush");

    // Warning
    public static ComponentResourceKey WarningBrush => new ComponentResourceKey(typeof(Brushes), "WarningBrush");
    public static ComponentResourceKey Warning100Brush => new ComponentResourceKey(typeof(Brushes), "Warning100Brush");
    public static ComponentResourceKey WarningContentBrush => new ComponentResourceKey(typeof(Brushes), "WarningContentBrush");

    // Danger
    public static ComponentResourceKey DangerBrush => new ComponentResourceKey(typeof(Brushes), "DangerBrush");
    public static ComponentResourceKey Danger100Brush => new ComponentResourceKey(typeof(Brushes), "Danger100Brush");
    public static ComponentResourceKey DangerContentBrush => new ComponentResourceKey(typeof(Brushes), "DangerContentBrush");
}