using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Controls.DataDisplay;

public class FontIconSamplesViewModel : ObservableObject
{
    public string Code =>
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<!-- Size flows from the inherited Sizing.Size -->\n" +
        "<fonts:FontIcon joufflu:Sizing.Size=\"lg\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />";
}
