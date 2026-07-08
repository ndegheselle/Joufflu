using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Assets.Fonts;
using Joufflu.Navigation;
using Joufflu.Samples.Views.Controls.DataDisplay;
using Joufflu.Samples.Views.Controls.Feedback;
using Joufflu.Samples.Views.Controls.Navigation;
using Joufflu.Samples.Views.Natives.Actions;
using Joufflu.Samples.Views.Natives.DataDisplay;
using Joufflu.Samples.Views.Natives.DataInput;
using Joufflu.Samples.Views.Natives.Feedback;
using Joufflu.Samples.Views.Natives.Layout;
using Joufflu.Samples.Views.Natives.Navigation;
using Joufflu.Samples.Views.Toolkit;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.ViewModels;

/// <summary>
/// Shell view model: owns the shared navigation services and builds the side menu.
/// Menu items target a page's view model when it has one, otherwise the view itself
/// (static pages need no view model).
/// </summary>
public class MainViewModel : ObservableObject
{
    public Navigator Navigator { get; } = new();

    public OverlayService Overlays { get; } = new();

    public ToastService Toasts { get; } = new();

    public ObservableCollection<NavigationMenuEntry> Menu { get; } = new();

    public MainViewModel()
    {
        var buttons = new ButtonSamplesViewModel();

        // Native controls
        Menu.Add(new NavigationMenuTitle("Actions"));
        Menu.Add(Item(LucideFontIcons.MousePointerClick, "Buttons", buttons));
        Menu.Add(Item(LucideFontIcons.ToggleLeft, "Toggle buttons", new ToggleButtonSamplesViewModel()));

        Menu.Add(new NavigationMenuTitle("Data input"));
        Menu.Add(Item(LucideFontIcons.TextCursorInput, "Text box", new TextBoxSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.List, "Combo box", new ComboBoxSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.CheckSquare, "Check box", new CheckBoxSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.CircleDot, "Radio button", new RadioButtonSamples()));
        Menu.Add(Item(LucideFontIcons.SlidersHorizontal, "Slider", new SliderSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Calendar, "Date picker", new DatePickerSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.CalendarDays, "Calendar", new CalendarSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.LayoutList, "List box", new ListBoxSamplesViewModel()));

        Menu.Add(new NavigationMenuTitle("Data display"));
        Menu.Add(Item(LucideFontIcons.Type, "Typography", new TypographySamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Tag, "Label", new LabelSamples()));
        Menu.Add(Item(LucideFontIcons.Table, "List view", new ListViewSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Layers2, "Tree view", new TreeViewSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Table2, "Data grid", new DataGridSamplesViewModel()));

        Menu.Add(new NavigationMenuTitle("Feedback"));
        Menu.Add(Item(LucideFontIcons.Loader, "Progress bar", new ProgressBarSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.PanelBottom, "Status bar", new StatusBarSamples()));

        Menu.Add(new NavigationMenuTitle("Layout"));
        Menu.Add(Item(LucideFontIcons.Square, "Card", new CardSamples()));
        Menu.Add(Item(LucideFontIcons.Component, "Group box", new GroupBoxSamples()));
        Menu.Add(Item(LucideFontIcons.PanelTop, "Expander", new ExpanderSamples()));
        Menu.Add(Item(LucideFontIcons.Move, "Scroll viewer", new ScrollViewerSamples()));
        Menu.Add(Item(LucideFontIcons.StretchHorizontal, "Grid splitter", new GridSplitterSamples()));

        Menu.Add(new NavigationMenuTitle("Navigation"));
        Menu.Add(Item(LucideFontIcons.Menu, "Menu", new MenuSamples()));
        Menu.Add(Item(LucideFontIcons.GalleryHorizontal, "Tab control", new TabControlSamples()));
        Menu.Add(Item(LucideFontIcons.Wrench, "Tool bar", new ToolBarSamples()));
        Menu.Add(Item(LucideFontIcons.Link, "Hyperlink", new HyperlinkSamples()));

        // Custom controls
        Menu.Add(new NavigationMenuTitle("Custom controls"));
        Menu.Add(Item(LucideFontIcons.Sparkles, "Font icon", new FontIconSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Badge, "Badge", new BadgeSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Circle, "Spinner", new SpinnerSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Bell, "Toasts", new ToastSamplesViewModel(Toasts)));
        Menu.Add(Item(LucideFontIcons.PanelLeft, "Navigation menu", new NavigationMenuSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.SquareStack, "Overlays", new OverlaySamplesViewModel(Overlays, Toasts)));

        // Toolkit
        Menu.Add(new NavigationMenuTitle("Toolkit"));
        Menu.Add(Item(LucideFontIcons.Scaling, "Sizing", new SizingSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.Space, "Spacing", new SpacingSamplesViewModel()));
        Menu.Add(Item(LucideFontIcons.AppWindow, "Application shell", new ShellSamples()));

        Navigator.Navigate(buttons);
    }

    private static NavigationMenuItem Item(string icon, string title, object target) =>
        new() { Icon = icon, Title = title, Target = target };
}
