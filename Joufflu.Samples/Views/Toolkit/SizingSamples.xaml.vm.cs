using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class SizingSamplesViewModel : ObservableObject
{
    public string SizeCode =>
        "<!-- Attached property drives height, font size and padding -->\n" +
        "<Button joufflu:Sizing.Size=\"xs\" />\n" +
        "<Button joufflu:Sizing.Size=\"sm\" />\n" +
        "<Button joufflu:Sizing.Size=\"md\" />  <!-- default -->\n" +
        "<Button joufflu:Sizing.Size=\"lg\" />\n\n" +
        "<!-- Size is inherited, so a panel sets it for every child -->\n" +
        "<StackPanel joufflu:Sizing.Size=\"lg\">\n" +
        "    <TextBox /> <ComboBox /> <Button>OK</Button>\n" +
        "</StackPanel>";

    public string SquareCode =>
        "<Button joufflu:Sizing.IsSquare=\"True\"\n" +
        "        joufflu:Sizing.Size=\"lg\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "</Button>";
}
