using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

public class FontIconSamplesViewModel : ObservableObject
{
    public string Code =>
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Home}\" />\n" +
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Bell}\" />\n" +
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Settings}\" />\n" +
        "<fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Trash}\" />";

    public string SizesCode =>
        "<fonts:FontIcon toolkit:Sizing.Size=\"xs\" Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<fonts:FontIcon toolkit:Sizing.Size=\"sm\" Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<fonts:FontIcon toolkit:Sizing.Size=\"md\" Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<fonts:FontIcon toolkit:Sizing.Size=\"lg\" Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />";

    public string ColourCode =>
        "<fonts:FontIcon Foreground=\"{DynamicResource {x:Static joufflu:Brushes.PrimaryBrush}}\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<fonts:FontIcon Foreground=\"{DynamicResource {x:Static joufflu:Brushes.SuccessBrush}}\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<fonts:FontIcon Foreground=\"{DynamicResource {x:Static joufflu:Brushes.WarningBrush}}\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "<fonts:FontIcon Foreground=\"{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}\"\n" +
        "                Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />";

    public string ChevronCode =>
        "<!-- A tree row's expander: Leading chevron, right then down -->\n" +
        "<ToggleButton IsChecked=\"{Binding IsExpanded}\" Style=\"{StaticResource ChevronToggleButton}\" />\n" +
        "<!-- After a header: down then up -->\n" +
        "<fonts:Chevron IsExpanded=\"{Binding IsExpanded}\" Placement=\"Trailing\" />";
}
