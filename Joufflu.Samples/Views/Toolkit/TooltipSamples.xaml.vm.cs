using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class TooltipSamplesViewModel : ObservableObject
{
    public string PlacementCode =>
        "<!-- Tooltip.Content wires up a themed tooltip on any element -->\n" +
        "<!-- Tooltip.Placement picks the side: Top / Bottom / Left / Right -->\n" +
        "<Button Content=\"Top\"\n" +
        "        feedback:Tooltip.Content=\"Placed above\"\n" +
        "        feedback:Tooltip.Placement=\"Top\" />\n\n" +
        "<Button Content=\"Right\"\n" +
        "        feedback:Tooltip.Content=\"Placed right\"\n" +
        "        feedback:Tooltip.Placement=\"Right\" />";

    public string ContentCode =>
        "<!-- Tooltip.Content is an object: pass arbitrary XAML -->\n" +
        "<Button Content=\"Rich tooltip\" feedback:Tooltip.Placement=\"Bottom\">\n" +
        "    <feedback:Tooltip.Content>\n" +
        "        <StackPanel Orientation=\"Horizontal\" joufflu:Spacing.Gap=\"8\">\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Info}\" />\n" +
        "            <TextBlock Text=\"Arbitrary content, icons included.\" />\n" +
        "        </StackPanel>\n" +
        "    </feedback:Tooltip.Content>\n" +
        "</Button>";
}
