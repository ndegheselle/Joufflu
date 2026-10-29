using Joufflu.Controls;

namespace Joufflu.Samples.Views.Themes;

/// <summary>
/// The theme customizer in a window of its own, opened from the title bar of the gallery so that the whole gallery,
/// left in view behind it, is the preview of what it edits.
/// </summary>
public partial class ThemeCustomizerWindow : ThemedWindow
{
    public ThemeCustomizerWindow()
    {
        DataContext = new ThemeCustomizerViewModel();
        InitializeComponent();
    }
}
