using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.Actions;

public class ToggleButtonSamplesViewModel : ObservableObject
{
    public string VariantsCode =>
        "<ToggleButton>Default</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource TogglePrimary}\">Primary</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource ToggleSecondary}\">Secondary</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource ToggleGhost}\">Ghost</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource ToggleSuccess}\">Success</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource ToggleDanger}\">Danger</ToggleButton>";

    public string IconCode =>
        "<ToggleButton Style=\"{StaticResource IconToggleButton}\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Bell}\" />\n" +
        "</ToggleButton>";

    public string SizesCode =>
        "<ToggleButton joufflu:ControlProperties.Size=\"xs\">XS</ToggleButton>\n" +
        "<ToggleButton joufflu:ControlProperties.Size=\"sm\">SM</ToggleButton>\n" +
        "<ToggleButton joufflu:ControlProperties.Size=\"md\">MD</ToggleButton>\n" +
        "<ToggleButton joufflu:ControlProperties.Size=\"lg\">LG</ToggleButton>";
}
