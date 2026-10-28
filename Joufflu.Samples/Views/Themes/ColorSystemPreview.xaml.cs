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
    private sealed record Palette(string Name, Color Foreground, Color Background, Color Surface1, Color Surface2, Color Stroke);

    private static readonly Palette[] Palettes =
    [
        new("Test colours", MediaColors.Orange, MediaColors.Red, MediaColors.Blue, MediaColors.Cyan, MediaColors.DarkCyan),
        new("Light", Rgb("#18181b"), Rgb("#f1f1f3"), Rgb("#ffffff"), Rgb("#f8f8f9"), Rgb("#d4d4d8")),
        new("Dark", Rgb("#fafafa"), Rgb("#0a0a0a"), Rgb("#141414"), Rgb("#1c1c1c"), Rgb("#2e2e2e")),
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
        Resources["PreviewBackground"] = palette.Background;
        Resources["PreviewSurface1"] = palette.Surface1;
        Resources["PreviewSurface2"] = palette.Surface2;
        Resources["PreviewStroke"] = palette.Stroke;
    }

    private static Color Rgb(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
