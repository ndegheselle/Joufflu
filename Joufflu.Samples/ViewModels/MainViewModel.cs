using CommunityToolkit.Mvvm.ComponentModel;
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
            ["Buttons"] = new ButtonSamplesViewModel(),
            ["Toggle buttons"] = new ToggleButtonSamplesViewModel(),

            ["Text box"] = new TextBoxSamplesViewModel(),
            ["Combo box"] = new ComboBoxSamplesViewModel(),
            ["Check box"] = new CheckBoxSamplesViewModel(),
            ["Radio button"] = new RadioButtonSamples(),
            ["Slider"] = new SliderSamplesViewModel(),
            ["Date picker"] = new DatePickerSamplesViewModel(),
            ["Calendar"] = new CalendarSamplesViewModel(),
            ["List box"] = new ListBoxSamplesViewModel(),

            ["Typography"] = new TypographySamplesViewModel(),
            ["Label"] = new LabelSamples(),
            ["List view"] = new ListViewSamplesViewModel(),
            ["Tree view"] = new TreeViewSamplesViewModel(),
            ["Data grid"] = new DataGridSamplesViewModel(),

            ["Progress bar"] = new ProgressBarSamplesViewModel(),
            ["Status bar"] = new StatusBarSamples(),

            ["Card"] = new CardSamples(),
            ["Group box"] = new GroupBoxSamples(),
            ["Expander"] = new ExpanderSamples(),
            ["Scroll viewer"] = new ScrollViewerSamples(),
            ["Grid splitter"] = new GridSplitterSamples(),

            ["Menu"] = new MenuSamples(),
            ["Tab control"] = new TabControlSamples(),
            ["Tool bar"] = new ToolBarSamples(),
            ["Hyperlink"] = new HyperlinkSamples(),

            // Custom controls
            ["Font icon"] = new FontIconSamplesViewModel(),
            ["Badge"] = new BadgeSamplesViewModel(),
            ["Spinner"] = new SpinnerSamplesViewModel(),
            ["Toasts"] = new ToastSamplesViewModel(Toasts),
            ["Navigation menu"] = new NavigationMenuSamplesViewModel(),
            ["Overlays"] = new OverlaySamplesViewModel(Overlays, Toasts),

            // Toolkit
            ["Sizing"] = new SizingSamplesViewModel(),
            ["Spacing"] = new SpacingSamplesViewModel(),
            ["Application shell"] = new ShellSamples(),
        };

        ResolveTarget = ResolvePage;

        if (ResolvePage("Buttons") is { } home)
            Navigator.Navigate(home);
    }

    /// <summary>Maps a menu item's text target to its page (view model), or null when unknown.</summary>
    private object? ResolvePage(string target) =>
        _pages.TryGetValue(target, out object? page) ? page : null;
}
