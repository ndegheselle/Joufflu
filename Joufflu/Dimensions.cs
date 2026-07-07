using System.Windows;

namespace Joufflu;

public static class Dimensions
{
    public static ComponentResourceKey CornerRadius => new(typeof(Dimensions), "CornerRadius");
    public static ComponentResourceKey BorderThickness => new(typeof(Dimensions), "BorderThickness");
    public static ComponentResourceKey Spacing => new(typeof(Dimensions), "Spacing");

    public static ComponentResourceKey ControlHeightXs => new(typeof(Dimensions), "ControlHeightXs");
    public static ComponentResourceKey ControlHeightSm => new(typeof(Dimensions), "ControlHeightSm");
    public static ComponentResourceKey ControlHeightMd => new(typeof(Dimensions), "ControlHeightMd");
    public static ComponentResourceKey ControlHeightLg => new(typeof(Dimensions), "ControlHeightLg");

    public static ComponentResourceKey ControlFontSizeXs => new(typeof(Dimensions), "ControlFontSizeXs");
    public static ComponentResourceKey ControlFontSizeSm => new(typeof(Dimensions), "ControlFontSizeSm");
    public static ComponentResourceKey ControlFontSizeMd => new(typeof(Dimensions), "ControlFontSizeMd");
    public static ComponentResourceKey ControlFontSizeLg => new(typeof(Dimensions), "ControlFontSizeLg");

    public static ComponentResourceKey ControlPaddingXs => new(typeof(Dimensions), "ControlPaddingXs");
    public static ComponentResourceKey ControlPaddingSm => new(typeof(Dimensions), "ControlPaddingSm");
    public static ComponentResourceKey ControlPaddingMd => new(typeof(Dimensions), "ControlPaddingMd");
    public static ComponentResourceKey ControlPaddingLg => new(typeof(Dimensions), "ControlPaddingLg");
}
