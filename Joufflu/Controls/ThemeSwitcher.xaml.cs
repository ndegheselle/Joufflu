using System.Windows.Controls;

namespace Joufflu.Controls;

/// <summary>
/// A drop-in segmented switcher (System / Light / Dark) bound to <see cref="Themes.ThemeManager"/>.
/// Requires <c>Themes.ThemeManager.Instance.Initialize()</c> to have run at startup.
/// </summary>
public partial class ThemeSwitcher : UserControl
{
    public ThemeSwitcher()
    {
        InitializeComponent();
    }
}
