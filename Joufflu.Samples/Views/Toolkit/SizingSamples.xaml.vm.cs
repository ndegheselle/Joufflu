using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class SizingSamplesViewModel : ObservableObject
{
    public string SizeCode =>
        "<!-- Attached property drives height, font size and padding -->\n" +
        "<Button joufflu:ControlProperties.Size=\"xs\" />\n" +
        "<Button joufflu:ControlProperties.Size=\"sm\" />\n" +
        "<Button joufflu:ControlProperties.Size=\"md\" />  <!-- default -->\n" +
        "<Button joufflu:ControlProperties.Size=\"lg\" />\n\n" +
        "<!-- Size is inherited, so a panel sets it for every child -->\n" +
        "<StackPanel joufflu:ControlProperties.Size=\"lg\">\n" +
        "    <TextBox /> <ComboBox /> <Button>OK</Button>\n" +
        "</StackPanel>";

    public string SquareCode =>
        "<Button joufflu:ControlProperties.IsSquare=\"True\"\n" +
        "        joufflu:ControlProperties.Size=\"lg\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "</Button>";
}
