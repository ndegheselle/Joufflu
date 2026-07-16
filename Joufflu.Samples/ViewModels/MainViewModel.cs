using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Controls;
using Joufflu.Navigation;
using Joufflu.Navigation.Controls;
using Joufflu.Samples.Views.Controls.DataDisplay;
using Joufflu.Samples.Views.Controls.Feedback;
using Joufflu.Samples.Views.Controls.Navigation;
using Joufflu.Samples.Views.Inputs;
using Joufflu.Samples.Views.Natives.Actions;
using Joufflu.Samples.Views.Natives.DataDisplay;
using Joufflu.Samples.Views.Natives.DataInput;
using Joufflu.Samples.Views.Natives.Feedback;
using Joufflu.Samples.Views.Natives.Layout;
using Joufflu.Samples.Views.Natives.Navigation;
using Joufflu.Samples.Views.Toolkit;

namespace Joufflu.Samples.ViewModels;

/// <summary>
/// Shell view model: owns the shared navigation services and the pages the side menu can reach.
/// The menu items are declared in XAML and point at a page through a text target; this view model
/// maps those targets to the actual pages via <see cref="ResolveTarget"/>.
/// </summary>
public class MainViewModel : ObservableObject
{
    public Navigator Navigator { get; } = new();

    public OverlayService Overlays { get; } = new();

    public ToastService Toasts { get; } = new();

    /// <summary>Pages keyed by the text target used on the menu's <c>NavigationItem</c>s.</summary>
    private readonly Dictionary<string, object> _pages;

    /// <summary>Bound to <c>NavigationMenu.TargetResolver</c> so the menu can resolve its text targets.</summary>
    public Func<string, object?> ResolveTarget { get; }

    public MainViewModel()
    {
        _pages = new()
        {
            // Native controls
            ["natives/buttons"] = new ButtonSamplesViewModel(),
            ["natives/toggle-buttons"] = new ToggleButtonSamplesViewModel(),

            ["natives/text-box"] = new TextBoxSamplesViewModel(),
            ["natives/combo-box"] = new ComboBoxSamplesViewModel(),
            ["natives/check-box"] = new CheckBoxSamplesViewModel(),
            ["natives/radio-button"] = new RadioButtonSamples(),
            ["natives/slider"] = new SliderSamplesViewModel(),
            ["natives/date-picker"] = new DatePickerSamplesViewModel(),
            ["natives/calendar"] = new CalendarSamplesViewModel(),
            ["natives/list-box"] = new ListBoxSamplesViewModel(),

            ["natives/typography"] = new TypographySamplesViewModel(),
            ["natives/label"] = new LabelSamples(),
            ["natives/list-view"] = new ListViewSamplesViewModel(),
            ["natives/tree-view"] = new TreeViewSamplesViewModel(),
            ["natives/data-grid"] = new DataGridSamplesViewModel(),

            ["natives/progress-bar"] = new ProgressBarSamplesViewModel(),
            ["natives/status-bar"] = new StatusBarSamples(),

            ["natives/card"] = new CardSamples(),
            ["natives/group-box"] = new GroupBoxSamples(),
            ["natives/expander"] = new ExpanderSamples(),
            ["natives/scroll-viewer"] = new ScrollViewerSamples(),
            ["natives/grid-splitter"] = new GridSplitterSamples(),

            ["natives/menu"] = new MenuSamples(),
            ["natives/tab-control"] = new TabControlSamples(),
            ["natives/tool-bar"] = new ToolBarSamples(),
            ["natives/hyperlink"] = new HyperlinkSamples(),

            // Inputs (Joufflu.Inputs library)
            ["inputs/numeric"] = new NumericInputsSamplesViewModel(),
            ["inputs/search"] = new SelectionInputsSamplesViewModel(),
            ["inputs/combo-box-tags"] = new ComboBoxTagsSamplesViewModel(),
            ["inputs/text-editable"] = new TextEditableSamplesViewModel(),
            ["inputs/file-picker"] = new FilePickerSamplesViewModel(),
            ["inputs/color-picker"] = new ColorPickerSamplesViewModel(),

            // Navigation (Joufflu.Navigation library)
            ["navigation/menu"] = new NavigationMenuSamplesViewModel(),
            ["navigation/overlays"] = new OverlaySamplesViewModel(Overlays, Toasts),

            // Custom controls
            ["controls/font-icon"] = new FontIconSamplesViewModel(),
            ["controls/badge"] = new BadgeSamplesViewModel(),
            ["controls/spinner"] = new SpinnerSamplesViewModel(),
            ["controls/toasts"] = new ToastSamplesViewModel(Toasts),

            // Toolkit
            ["toolkit/sizing"] = new SizingSamplesViewModel(),
            ["toolkit/spacing"] = new SpacingSamplesViewModel(),
            ["toolkit/theme"] = new ThemeSamplesViewModel(),
            ["toolkit/customize-theme"] = new ThemeCustomizerViewModel(),
            ["toolkit/application-shell"] = new ShellSamples(),
        };

        ResolveTarget = ResolvePage;

        if (ResolvePage("natives/buttons") is { } home)
            Navigator.Navigate(home);
    }

    /// <summary>Maps a menu item's text target to its page (view model), or null when unknown.</summary>
    private object? ResolvePage(string target) =>
        _pages.TryGetValue(target, out object? page) ? page : null;
}
