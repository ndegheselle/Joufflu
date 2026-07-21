using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.Actions;

public class ButtonSamplesViewModel : ObservableObject
{
    public string VariantsCode =>
        "<Button>Default</Button>\n" +
        "<Button Style=\"{StaticResource Primary}\">Primary</Button>\n" +
        "<Button Style=\"{StaticResource Secondary}\">Secondary</Button>\n" +
        "<Button Style=\"{StaticResource Ghost}\">Ghost</Button>\n" +
        "<Button Style=\"{StaticResource Success}\">Success</Button>\n" +
        "<Button Style=\"{StaticResource Danger}\">Danger</Button>";

    public string SoftCode =>
        "<!-- Tinted background, semantic hue as text -->\n" +
        "<Button Style=\"{StaticResource SoftPrimary}\">Primary</Button>\n" +
        "<Button Style=\"{StaticResource SoftSuccess}\">Success</Button>\n" +
        "<Button Style=\"{StaticResource SoftInfo}\">Info</Button>\n" +
        "<Button Style=\"{StaticResource SoftWarning}\">Warning</Button>\n" +
        "<Button Style=\"{StaticResource SoftDanger}\">Danger</Button>";

    public string OutlineCode =>
        "<!-- Coloured border and text, transparent fill -->\n" +
        "<Button Style=\"{StaticResource OutlinePrimary}\">Primary</Button>\n" +
        "<Button Style=\"{StaticResource OutlineSuccess}\">Success</Button>\n" +
        "<Button Style=\"{StaticResource OutlineInfo}\">Info</Button>\n" +
        "<Button Style=\"{StaticResource OutlineWarning}\">Warning</Button>\n" +
        "<Button Style=\"{StaticResource OutlineDanger}\">Danger</Button>";

    public string IconCode =>
        "<Button joufflu:ControlProperties.IsSquare=\"True\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Plus}\" />\n" +
        "</Button>";

    public string SizesCode =>
        "<Button joufflu:ControlProperties.Size=\"xs\">XS</Button>\n" +
        "<Button joufflu:ControlProperties.Size=\"sm\">SM</Button>\n" +
        "<Button joufflu:ControlProperties.Size=\"md\">MD</Button>\n" +
        "<Button joufflu:ControlProperties.Size=\"lg\">LG</Button>";
}
