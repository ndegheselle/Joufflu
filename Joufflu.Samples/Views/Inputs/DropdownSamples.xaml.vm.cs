using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class DropdownSamplesViewModel : ObservableObject
{
    public string Code =>
        "<inputs:Dropdown Header=\"Actions\">\n" +
        "    <StackPanel joufflu:Spacing.Gap=\"4\">\n" +
        "        <Button Content=\"Rename\" Style=\"{StaticResource GhostButton}\" />\n" +
        "        <Button Content=\"Duplicate\" Style=\"{StaticResource GhostButton}\" />\n" +
        "    </StackPanel>\n" +
        "</inputs:Dropdown>";

    public string PlacementCode =>
        "<!-- BottomLeft (default), BottomRight, TopLeft, TopRight -->\n" +
        "<inputs:Dropdown Header=\"Filter\" PopupPlacement=\"BottomRight\">\n" +
        "    <TextBlock Text=\"Opens down, right aligned.\" />\n" +
        "</inputs:Dropdown>";

    public string StyleCode =>
        "<inputs:Dropdown ButtonStyle=\"{StaticResource IconToggleButton}\" PopupPlacement=\"BottomRight\">\n" +
        "    <inputs:Dropdown.Header>\n" +
        "        <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.EllipsisVertical}\" />\n" +
        "    </inputs:Dropdown.Header>\n" +
        "    <TextBlock Text=\"An icon only dropdown.\" />\n" +
        "</inputs:Dropdown>";
}
