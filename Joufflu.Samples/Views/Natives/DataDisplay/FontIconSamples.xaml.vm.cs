using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

public class FontIconSamplesViewModel : ObservableObject
{
    public string Code =>
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<!-- Size flows from the inherited Sizing.Size -->\n" +
        "<fonts:FontIcon toolkit:Sizing.Size=\"lg\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />";

    public string ChevronCode =>
        "<!-- A tree row's expander: Leading chevron, right then down -->\n" +
        "<ToggleButton IsChecked=\"{Binding IsExpanded}\" Style=\"{StaticResource ChevronToggleButton}\" />\n" +
        "<!-- After a header: down then up -->\n" +
        "<fonts:Chevron IsExpanded=\"{Binding IsExpanded}\" Placement=\"Trailing\" />";
}
