using System.Windows.Controls;
using System.Windows.Media;
using MediaColors = System.Windows.Media.Colors;

namespace Joufflu.Samples.Views.Themes;

/// <summary>
/// A laboratory for the neutral colours: the layers (page, card, elevated surface, popup) and every state of a
/// control on each of them, drawn from five colours and what is derived from them. A control is filled with the
/// page colour and lives on a card. The palette picker swaps the five colours
/// so a derivation can be judged on loud colours, where the layers are easy to tell apart, and on realistic ones.
/// </summary>
public partial class ColorSystemPreview : UserControl
{
    private sealed record Palette(string Name, Color Foreground, Color Base, Color Surface1, Color Surface2, Color Stroke);

    private static readonly Palette[] Palettes =
    [
        new("Test colours", MediaColors.Orange, MediaColors.Red, MediaColors.Blue, MediaColors.Cyan, MediaColors.DarkCyan),
        new("Light", Rgb("#18181b"), Rgb("#f1f1f3"), Rgb("#ffffff"), Rgb("#f8f8f9"), Rgb("#d4d4d8")),
        new("Dark", Rgb("#fafafa"), Rgb("#0a0a0a"), Rgb("#141414"), Rgb("#1c1c1c"), Rgb("#2e2e2e")),

        // daisyUI "light" (@plugin "daisyui/theme", color-scheme light). Only what the preview defines is taken, and
        // daisyUI's base-100 is the page with base-200 and base-300 the darker elevations, so:
        // Base = base-100 oklch(100% 0 0), Surface1 = base-200 oklch(98% 0 0), Surface2 = base-300 oklch(95% 0 0),
        // Foreground = base-content oklch(21% 0.006 285.885), Stroke = base-content mixed 20% into base-100 (the
        // border daisyUI gives an input). The accents, the radii and the sizes are not part of the preview yet.
        new("daisyUI light", Rgb("#18181b"), Rgb("#ffffff"), Rgb("#f8f8f8"), Rgb("#eeeeee"), Rgb("#d1d1d1")),

        // daisyUI "dark" (@plugin "daisyui/theme", color-scheme dark), mapped the same way:
        // Base = base-100 oklch(25.33% 0.016 252.42), Surface1 = base-200 oklch(23.26% 0.014 253.1),
        // Surface2 = base-300 oklch(21.15% 0.012 254.09), Foreground = base-content oklch(97.807% 0.029 256.847),
        // Stroke = base-content mixed 20% into base-100.
        new("daisyUI dark", Rgb("#ecf9ff"), Rgb("#1d232a"), Rgb("#191e24"), Rgb("#15191e"), Rgb("#464e55")),
    ];

    public ColorSystemPreview()
    {
        InitializeComponent();
        PaletteChoice.ItemsSource = Palettes;
        PaletteChoice.SelectedIndex = 0;
    }

    private void PaletteChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PaletteChoice.SelectedItem is not Palette palette)
            return;

        // The brushes and the shades read these through DynamicResource: redefining them redraws everything.
        Resources["PreviewForeground"] = palette.Foreground;
        Resources["PreviewBase"] = palette.Base;
        Resources["PreviewSurface1"] = palette.Surface1;
        Resources["PreviewSurface2"] = palette.Surface2;
        Resources["PreviewStroke"] = palette.Stroke;
    }

    private static Color Rgb(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
