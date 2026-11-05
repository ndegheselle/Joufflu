using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Toolkit;

namespace Joufflu.Samples.Views.Toolkit;

public partial class TooltipSamplesViewModel : ObservableObject
{
    public string PlacementCode =>
        "<!-- ToolTip as usual; Tooltip.Placement picks the side -->\n" +
        "<Button Content=\"Top\"    ToolTip=\"Placed above\" toolkit:Tooltip.Placement=\"Top\" />\n" +
        "<Button Content=\"Bottom\" ToolTip=\"Placed below\" toolkit:Tooltip.Placement=\"Bottom\" />\n" +
        "<Button Content=\"Left\"   ToolTip=\"Placed left\"  toolkit:Tooltip.Placement=\"Left\" />\n" +
        "<Button Content=\"Right\"  ToolTip=\"Placed right\" toolkit:Tooltip.Placement=\"Right\" />";

    public string AlignmentCode =>
        "<!-- Tooltip.Alignment lines it up along the side: Start, Center (default) or End -->\n" +
        "<Button Content=\"Start\"  ToolTip=\"Aligned on the left edge\"  toolkit:Tooltip.Alignment=\"Start\" />\n" +
        "<Button Content=\"Center\" ToolTip=\"Centred under the button\" toolkit:Tooltip.Alignment=\"Center\" />\n" +
        "<Button Content=\"End\"    ToolTip=\"Aligned on the right edge\" toolkit:Tooltip.Alignment=\"End\" />";

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
        "<!-- Text wraps past Tooltip.MaxWidth, 320 by default -->\n" +
        "<Button Content=\"Narrow\"\n" +
        "        toolkit:Tooltip.MaxWidth=\"160\"\n" +
        "        ToolTip=\"A long description, wrapped at a narrower max width.\" />";

    #region Popup placement
    public IReadOnlyList<PopupSide> PopupSides { get; } = Enum.GetValues<PopupSide>();

    public IReadOnlyList<PopupAlignment> PopupAlignments { get; } = Enum.GetValues<PopupAlignment>();

    [ObservableProperty]
    public partial PopupSide SelectedPopupSide { get; set; } = PopupSide.Bottom;

    [ObservableProperty]
    public partial PopupAlignment SelectedPopupAlignment { get; set; } = PopupAlignment.Start;

    public string PopupPlacementCode =>
        "<!-- Side: Top, Bottom, Left, Right; Alignment: Start, Center, End; Gap in DIPs, 2 by default -->\n" +
        "<Popup PlacementTarget=\"{Binding ElementName=Anchor}\"\n" +
        "       toolkit:PopupPlacement.Side=\"Bottom\"\n" +
        "       toolkit:PopupPlacement.Alignment=\"End\">\n" +
        "    <inputs:DropdownPopupHost>\n" +
        "        <TextBlock Text=\"Placed popup\" />\n" +
        "    </inputs:DropdownPopupHost>\n" +
        "</Popup>";
    #endregion
}
