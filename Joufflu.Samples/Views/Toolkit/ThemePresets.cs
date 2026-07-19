using System.Windows.Media;

namespace Joufflu.Samples.Views.Toolkit;

/// <summary>
/// A named, ready-to-apply colour palette selectable from the theme customizer.
/// <see cref="Colors"/> is keyed by the same resource names the editor uses (e.g. <c>PrimaryColor</c>),
/// while the brush properties expose a few swatches so the selector card can render itself
/// in the theme's own colours (à la daisyUI's theme list).
/// </summary>
public class ThemePreset
{
    public string Name { get; }

    /// <summary>Colour per <see cref="ThemeColorEntry.ResourceName"/>; covers all 26 editable keys.</summary>
    public IReadOnlyDictionary<string, Color> Colors { get; }

    public Brush Background { get; }
    public Brush Foreground { get; }
    public Brush Border { get; }
    public Brush Primary { get; }
    public Brush Secondary { get; }
    public Brush Accent { get; }
    public Brush Neutral { get; }

    public ThemePreset(string name, IReadOnlyDictionary<string, Color> colors)
    {
        Name = name;
        Colors = colors;
        Background = Swatch(colors["BackgroundColor"]);
        Foreground = Swatch(colors["ForegroundColor"]);
        Border = Swatch(colors["BorderColor"]);
        Primary = Swatch(colors["PrimaryColor"]);
        Secondary = Swatch(colors["SecondaryColor"]);
        Accent = Swatch(colors["InfoColor"]);
        Neutral = Swatch(colors["Background200Color"]);
    }

    private static SolidColorBrush Swatch(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}

/// <summary>The curated list of preset themes offered by the customizer.</summary>
public static class ThemePresets
{
    public static IReadOnlyList<ThemePreset> All { get; } = Build();

    private static IReadOnlyList<ThemePreset> Build() => new[]
    {
        // Built-ins — mirror Joufflu/Themes/Dark.xaml and Light.xaml exactly.
        Full("Dark",
            bg: "#0a0a0a", bg1: "#171717", bg2: "#282828", border: "#202020", border1: "#2e2e2e",
            fg: "#FAFAFA", fg1: "#FAFAFA", fg2: "#FAFAFA",
            primary: "#FAFAFA", primary1: "#E5E5E5", primaryC: "#18181B",
            secondary: "#27272A", secondary1: "#3F3F46", secondaryC: "#FAFAFA",
            success: "#00D390", success1: "#00BD80", successC: "#004C39",
            info: "#3B82F6", info1: "#2563EB", infoC: "#172554",
            warning: "#EAB308", warning1: "#CA8A04", warningC: "#422006",
            danger: "#FF627D", danger1: "#F54A6A", dangerC: "#4D0218"),

        Full("Light",
            bg: "#f5f5f5", bg1: "#ffffff", bg2: "#E6E6E6", border: "#E6E6E6", border1: "#C5C5C5",
            fg: "#0a0a0a", fg1: "#737373", fg2: "#737373",
            primary: "#FF18181B", primary1: "#FF27272A", primaryC: "#FFFAFAFA",
            secondary: "#FFE4E4E7", secondary1: "#FFD4D4D8", secondaryC: "#FF27272A",
            success: "#00ba7b", success1: "#00a76e", successC: "#002c21",
            info: "#FF3B82F6", info1: "#1d6ff4", infoC: "#FF172554",
            warning: "#eeaf00", warning1: "#d69d00", warningC: "#411e03",
            danger: "#ff627d", danger1: "#fd718a", dangerC: "#4d0218"),

        // Flavours inspired by daisyUI — hover/muted shades are derived.
        Derived("Cupcake",
            bg: "#FAF7F5", bg1: "#EFEAE6", bg2: "#E7E2DF", border: "#DCD4CD", fg: "#291334",
            primary: "#65C3C8", primaryC: "#093A3E", secondary: "#EF9FBC", secondaryC: "#45162E",
            success: "#00B795", successC: "#003A2E", info: "#EEAF3A", infoC: "#3A2A06",
            warning: "#F3CC30", warningC: "#3A3206", danger: "#FF6F70", dangerC: "#3A0E0E"),

        Derived("Emerald",
            bg: "#FFFFFF", bg1: "#F4F6F8", bg2: "#E5E7EB", border: "#E5E7EB", fg: "#333C4D",
            primary: "#66CC8A", primaryC: "#04160B", secondary: "#377CFB", secondaryC: "#FFFFFF",
            success: "#00A96E", successC: "#FFFFFF", info: "#F68067", infoC: "#2A0F08",
            warning: "#FFBE00", warningC: "#3A2C00", danger: "#F87272", dangerC: "#3A0E0E"),

        Derived("Corporate",
            bg: "#FFFFFF", bg1: "#F7F9FC", bg2: "#EEF2F6", border: "#E5E9F0", fg: "#181A2A",
            primary: "#4B6BFB", primaryC: "#FFFFFF", secondary: "#7B92B2", secondaryC: "#FFFFFF",
            success: "#36D399", successC: "#04160B", info: "#3ABFF8", infoC: "#06222E",
            warning: "#FBBD23", warningC: "#3A2C00", danger: "#F87272", dangerC: "#3A0E0E"),

        Derived("Nord",
            bg: "#ECEFF4", bg1: "#E5E9F0", bg2: "#D8DEE9", border: "#D8DEE9", fg: "#2E3440",
            primary: "#5E81AC", primaryC: "#FFFFFF", secondary: "#81A1C1", secondaryC: "#10151D",
            success: "#A3BE8C", successC: "#10201A", info: "#88C0D0", infoC: "#0A2027",
            warning: "#EBCB8B", warningC: "#2E2606", danger: "#BF616A", dangerC: "#FFFFFF"),

        Derived("Synthwave",
            bg: "#1B1235", bg1: "#241A4A", bg2: "#2D1B69", border: "#3A2A63", fg: "#F9F7FD",
            primary: "#E779C1", primaryC: "#2A0A20", secondary: "#58C7F3", secondaryC: "#06222E",
            success: "#71EAD2", successC: "#05261F", info: "#53C0F3", infoC: "#06222E",
            warning: "#E2D562", warningC: "#2E2905", danger: "#FD6F9C", dangerC: "#2E0716"),

        Derived("Dracula",
            bg: "#282A36", bg1: "#343746", bg2: "#424458", border: "#44475A", fg: "#F8F8F2",
            primary: "#FF79C6", primaryC: "#1A1423", secondary: "#BD93F9", secondaryC: "#1A1423",
            success: "#50FA7B", successC: "#05260F", info: "#8BE9FD", infoC: "#06222A",
            warning: "#F1FA8C", warningC: "#2E2E05", danger: "#FF5555", dangerC: "#2E0707"),
    };

    /// <summary>Builds a preset from a compact spec, deriving muted text and hover shades.</summary>
    private static ThemePreset Derived(string name, string bg, string bg1, string bg2, string border, string fg,
        string primary, string primaryC, string secondary, string secondaryC,
        string success, string successC, string info, string infoC,
        string warning, string warningC, string danger, string dangerC)
    {
        Color background = P(bg), foreground = P(fg);
        var colors = new Dictionary<string, Color>
        {
            ["BackgroundColor"] = background,
            ["Background100Color"] = P(bg1),
            ["Background200Color"] = P(bg2),
            ["BorderColor"] = P(border),
            ["Border100Color"] = Lerp(P(border), background, 0.5),
            ["ForegroundColor"] = foreground,
            ["Foreground100Color"] = Lerp(foreground, background, 0.35),
            ["Foreground200Color"] = Lerp(foreground, background, 0.55),
            ["PrimaryColor"] = P(primary),
            ["Primary100Color"] = Hover(P(primary)),
            ["PrimaryContentColor"] = P(primaryC),
            ["SecondaryColor"] = P(secondary),
            ["Secondary100Color"] = Hover(P(secondary)),
            ["SecondaryContentColor"] = P(secondaryC),
            ["SuccessColor"] = P(success),
            ["Success100Color"] = Hover(P(success)),
            ["SuccessContentColor"] = P(successC),
            ["InfoColor"] = P(info),
            ["Info100Color"] = Hover(P(info)),
            ["InfoContentColor"] = P(infoC),
            ["WarningColor"] = P(warning),
            ["Warning100Color"] = Hover(P(warning)),
            ["WarningContentColor"] = P(warningC),
            ["DangerColor"] = P(danger),
            ["Danger100Color"] = Hover(P(danger)),
            ["DangerContentColor"] = P(dangerC),
        };
        return new ThemePreset(name, colors);
    }

    /// <summary>Builds a preset with every shade specified explicitly (used for the built-ins).</summary>
    private static ThemePreset Full(string name,
        string bg, string bg1, string bg2, string border, string border1,
        string fg, string fg1, string fg2,
        string primary, string primary1, string primaryC,
        string secondary, string secondary1, string secondaryC,
        string success, string success1, string successC,
        string info, string info1, string infoC,
        string warning, string warning1, string warningC,
        string danger, string danger1, string dangerC)
    {
        var colors = new Dictionary<string, Color>
        {
            ["BackgroundColor"] = P(bg),
            ["Background100Color"] = P(bg1),
            ["Background200Color"] = P(bg2),
            ["BorderColor"] = P(border),
            ["Border100Color"] = P(border1),
            ["ForegroundColor"] = P(fg),
            ["Foreground100Color"] = P(fg1),
            ["Foreground200Color"] = P(fg2),
            ["PrimaryColor"] = P(primary),
            ["Primary100Color"] = P(primary1),
            ["PrimaryContentColor"] = P(primaryC),
            ["SecondaryColor"] = P(secondary),
            ["Secondary100Color"] = P(secondary1),
            ["SecondaryContentColor"] = P(secondaryC),
            ["SuccessColor"] = P(success),
            ["Success100Color"] = P(success1),
            ["SuccessContentColor"] = P(successC),
            ["InfoColor"] = P(info),
            ["Info100Color"] = P(info1),
            ["InfoContentColor"] = P(infoC),
            ["WarningColor"] = P(warning),
            ["Warning100Color"] = P(warning1),
            ["WarningContentColor"] = P(warningC),
            ["DangerColor"] = P(danger),
            ["Danger100Color"] = P(danger1),
            ["DangerContentColor"] = P(dangerC),
        };
        return new ThemePreset(name, colors);
    }

    /// <summary>Parses a hex string (#RGB shorthand not supported) into an opaque colour.</summary>
    private static Color P(string hex) => (Color)ColorConverter.ConvertFromString(hex)!;

    /// <summary>A subtle hover shade: the colour nudged 15% toward black.</summary>
    private static Color Hover(Color color) => Lerp(color, Color.FromRgb(0, 0, 0), 0.15);

    private static Color Lerp(Color a, Color b, double t) => Color.FromArgb(
        255,
        (byte)Math.Round(a.R + (b.R - a.R) * t),
        (byte)Math.Round(a.G + (b.G - a.G) * t),
        (byte)Math.Round(a.B + (b.B - a.B) * t));
}
