using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Controls.DataDisplay;

public class FontIconSamplesViewModel : ObservableObject
{
    public string Code =>
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<!-- Size flows from the inherited ControlProperties.Size -->\n" +
        "<fonts:FontIcon joufflu:ControlProperties.Size=\"lg\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />";
}
