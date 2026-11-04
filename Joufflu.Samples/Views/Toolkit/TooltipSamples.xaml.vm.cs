using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class TooltipSamplesViewModel : ObservableObject
{
    public string PlacementCode =>
        "<!-- ToolTip as usual; ThemedTooltip.Placement picks the side -->\n" +
        "<Button Content=\"Top\"    ToolTip=\"Placed above\" toolkit:ThemedTooltip.Placement=\"Top\" />\n" +
        "<Button Content=\"Bottom\" ToolTip=\"Placed below\" toolkit:ThemedTooltip.Placement=\"Bottom\" />\n" +
        "<Button Content=\"Left\"   ToolTip=\"Placed left\"  toolkit:ThemedTooltip.Placement=\"Left\" />\n" +
        "<Button Content=\"Right\"  ToolTip=\"Placed right\" toolkit:ThemedTooltip.Placement=\"Right\" />";

    public string ContentCode =>
        "<!-- ToolTip is an object: pass arbitrary XAML -->\n" +
        "<Button Content=\"Rich tooltip\">\n" +
        "    <Button.ToolTip>\n" +
        "        <StackPanel Orientation=\"Horizontal\" toolkit:Spacing.Gap=\"8\">\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Info}\" />\n" +
        "            <TextBlock Text=\"Arbitrary content, icons included.\" />\n" +
        "        </StackPanel>\n" +
        "    </Button.ToolTip>\n" +
        "</Button>";

    public string MaxWidthCode =>
        "<!-- Text wraps past ThemedTooltip.MaxWidth, 320 by default -->\n" +
        "<Button Content=\"Narrow\"\n" +
        "        toolkit:ThemedTooltip.MaxWidth=\"160\"\n" +
        "        ToolTip=\"A long description, wrapped at a narrower max width.\" />";
}
