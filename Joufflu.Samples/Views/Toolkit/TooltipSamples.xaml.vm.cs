using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class TooltipSamplesViewModel : ObservableObject
{
    public string PlacementCode =>
        "<!-- ToolTip as usual; ThemedTooltip.Placement picks the side: Top / Bottom / Left / Right -->\n" +
        "<Button Content=\"Top\"\n" +
        "        ToolTip=\"Placed above\"\n" +
        "        toolkit:ThemedTooltip.Placement=\"Top\" />\n\n" +
        "<Button Content=\"Right\"\n" +
        "        ToolTip=\"Placed right\"\n" +
        "        toolkit:ThemedTooltip.Placement=\"Right\" />";

    public string ContentCode =>
        "<!-- ToolTip is an object: pass arbitrary XAML -->\n" +
        "<Button Content=\"Rich tooltip\" toolkit:ThemedTooltip.Placement=\"Bottom\">\n" +
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
        "        ToolTip=\"The same long description, wrapped at a narrower max width.\" />";
}
