using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Controls.DataDisplay;

public class BadgeSamplesViewModel : ObservableObject
{
    public string VariantsCode =>
        "<controls:Badge>Default</controls:Badge>\n" +
        "<controls:Badge Variant=\"Primary\">Primary</controls:Badge>\n" +
        "<controls:Badge Variant=\"Success\">Active</controls:Badge>\n" +
        "<controls:Badge Variant=\"Danger\">3</controls:Badge>";

    public string SizesCode =>
        "<controls:Badge Variant=\"Primary\" joufflu:ControlProperties.Size=\"xs\">xs</controls:Badge>\n" +
        "<controls:Badge Variant=\"Primary\" joufflu:ControlProperties.Size=\"lg\">lg</controls:Badge>";
}
