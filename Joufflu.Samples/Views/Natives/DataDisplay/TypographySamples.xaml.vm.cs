using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

public class TypographySamplesViewModel : ObservableObject
{
    public string SizesCode =>
        "<TextBlock toolkit:Sizing.Size=\"xl\" Text=\"xl\" />\n" +
        "<TextBlock toolkit:Sizing.Size=\"lg\" Text=\"lg\" />\n" +
        "<TextBlock Text=\"md, the inherited body size\" />\n" +
        "<TextBlock toolkit:Sizing.Size=\"sm\" Text=\"sm\" />\n" +
        "<TextBlock toolkit:Sizing.Size=\"xs\" Text=\"xs\" />";

    public string HeadingsCode =>
        "<TextBlock toolkit:Sizing.Size=\"xl\" FontWeight=\"Bold\" Text=\"Page title\" />\n" +
        "<TextBlock toolkit:Sizing.Size=\"lg\" FontWeight=\"SemiBold\" Text=\"Section\" />\n" +
        "<TextBlock FontWeight=\"SemiBold\" Text=\"Minor heading\" />";

    public string MutedCode =>
        "<TextBlock Text=\"Default body text\" />\n" +
        "<TextBlock Opacity=\"{DynamicResource {x:Static joufflu:Colors.MutedOpacity}}\"\n" +
        "           Text=\"Muted text\" />";
}
