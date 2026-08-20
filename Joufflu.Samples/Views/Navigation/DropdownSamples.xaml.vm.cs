using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Navigation;

public class DropdownSamplesViewModel : ObservableObject
{
    public string Code =>
        "<nav:Dropdown Header=\"Actions\">\n" +
        "    <StackPanel joufflu:Spacing.Gap=\"4\">\n" +
        "        <Button Content=\"Rename\" Style=\"{StaticResource GhostButton}\" />\n" +
        "        <Button Content=\"Duplicate\" Style=\"{StaticResource GhostButton}\" />\n" +
        "    </StackPanel>\n" +
        "</nav:Dropdown>";

    public string PlacementCode =>
        "<!-- BottomLeft (default), BottomRight, TopLeft, TopRight -->\n" +
        "<nav:Dropdown Header=\"Filter\" PopupPlacement=\"BottomRight\">\n" +
        "    <TextBlock Text=\"Opens down, right aligned.\" />\n" +
        "</nav:Dropdown>";

    public string StyleCode =>
        "<nav:Dropdown ButtonStyle=\"{StaticResource IconToggleButton}\" PopupPlacement=\"BottomRight\">\n" +
        "    <nav:Dropdown.Header>\n" +
        "        <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.EllipsisVertical}\" />\n" +
        "    </nav:Dropdown.Header>\n" +
        "    <TextBlock Text=\"An icon only dropdown.\" />\n" +
        "</nav:Dropdown>";
}
