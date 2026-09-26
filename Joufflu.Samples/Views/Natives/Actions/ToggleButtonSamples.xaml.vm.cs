using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.Actions;

public class ToggleButtonSamplesViewModel : ObservableObject
{
    public string VariantsCode =>
        "<ToggleButton>Default</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource PrimaryToggleButton}\">Primary</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource SecondaryToggleButton}\">Secondary</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource GhostToggleButton}\">Ghost</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource SuccessToggleButton}\">Success</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource InfoToggleButton}\">Info</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource WarningToggleButton}\">Warning</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource DangerToggleButton}\">Danger</ToggleButton>";

    public string SoftCode =>
        "<!-- Tinted background, solid when checked -->\n" +
        "<ToggleButton Style=\"{StaticResource SoftPrimaryToggleButton}\">Primary</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource SoftSuccessToggleButton}\">Success</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource SoftInfoToggleButton}\">Info</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource SoftWarningToggleButton}\">Warning</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource SoftDangerToggleButton}\">Danger</ToggleButton>";

    public string OutlineCode =>
        "<!-- Coloured border and text, solid when checked -->\n" +
        "<ToggleButton Style=\"{StaticResource OutlinePrimaryToggleButton}\">Primary</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource OutlineSuccessToggleButton}\">Success</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource OutlineInfoToggleButton}\">Info</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource OutlineWarningToggleButton}\">Warning</ToggleButton>\n" +
        "<ToggleButton Style=\"{StaticResource OutlineDangerToggleButton}\">Danger</ToggleButton>";

    public string IconCode =>
        "<ToggleButton Style=\"{StaticResource IconToggleButton}\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Bell}\" />\n" +
        "</ToggleButton>";

    public string SizesCode =>
        "<ToggleButton toolkit:Sizing.Size=\"xs\">XS</ToggleButton>\n" +
        "<ToggleButton toolkit:Sizing.Size=\"sm\">SM</ToggleButton>\n" +
        "<ToggleButton toolkit:Sizing.Size=\"md\">MD</ToggleButton>\n" +
        "<ToggleButton toolkit:Sizing.Size=\"lg\">LG</ToggleButton>";
}
