using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class TooltipSamplesViewModel : ObservableObject
{
    public string PlacementCode =>
        "<!-- Tooltip.Content wires up a themed tooltip on any element -->\n" +
        "<!-- Tooltip.Placement picks the side: Top / Bottom / Left / Right -->\n" +
        "<Button Content=\"Top\"\n" +
        "        joufflu:Tooltip.Content=\"Placed above\"\n" +
        "        joufflu:Tooltip.Placement=\"Top\" />\n\n" +
        "<Button Content=\"Right\"\n" +
        "        joufflu:Tooltip.Content=\"Placed right\"\n" +
        "        joufflu:Tooltip.Placement=\"Right\" />";

    public string ContentCode =>
        "<!-- Tooltip.Content is an object: pass arbitrary XAML -->\n" +
        "<Button Content=\"Rich tooltip\" joufflu:Tooltip.Placement=\"Bottom\">\n" +
        "    <joufflu:Tooltip.Content>\n" +
        "        <StackPanel Orientation=\"Horizontal\" joufflu:Spacing.Gap=\"8\">\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Info}\" />\n" +
        "            <TextBlock Text=\"Arbitrary content, icons included.\" />\n" +
        "        </StackPanel>\n" +
        "    </joufflu:Tooltip.Content>\n" +
        "</Button>";
}
