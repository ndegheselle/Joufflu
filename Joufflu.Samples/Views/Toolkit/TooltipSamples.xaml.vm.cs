using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class TooltipSamplesViewModel : ObservableObject
{
    public string PlacementCode =>
        "<!-- ThemedTooltip.Content wires up a themed tooltip on any element -->\n" +
        "<!-- ThemedTooltip.Placement picks the side: Top / Bottom / Left / Right -->\n" +
        "<Button Content=\"Top\"\n" +
        "        toolkit:ThemedTooltip.Content=\"Placed above\"\n" +
        "        toolkit:ThemedTooltip.Placement=\"Top\" />\n\n" +
        "<Button Content=\"Right\"\n" +
        "        toolkit:ThemedTooltip.Content=\"Placed right\"\n" +
        "        toolkit:ThemedTooltip.Placement=\"Right\" />";

    public string ContentCode =>
        "<!-- ThemedTooltip.Content is an object: pass arbitrary XAML -->\n" +
        "<Button Content=\"Rich tooltip\" toolkit:ThemedTooltip.Placement=\"Bottom\">\n" +
        "    <toolkit:ThemedTooltip.Content>\n" +
        "        <StackPanel Orientation=\"Horizontal\" toolkit:Spacing.Gap=\"8\">\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Info}\" />\n" +
        "            <TextBlock Text=\"Arbitrary content, icons included.\" />\n" +
        "        </StackPanel>\n" +
        "    </toolkit:ThemedTooltip.Content>\n" +
        "</Button>";
}
