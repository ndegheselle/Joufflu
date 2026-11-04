using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Feedback;

public class BadgeSamplesViewModel : ObservableObject
{
    public string VariantsCode =>
        "<feedback:Badge>Default</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Primary\">Primary</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Secondary\">Secondary</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Success\">Success</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Info\">Info</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Warning\">Warning</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Danger\">Danger</feedback:Badge>";

    public string SizesCode =>
        "<feedback:Badge Variant=\"Primary\" toolkit:Sizing.Size=\"xs\">xs</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Primary\" toolkit:Sizing.Size=\"sm\">sm</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Primary\">md</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Primary\" toolkit:Sizing.Size=\"lg\">lg</feedback:Badge>";
}
