using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
using Joufflu.Navigation;
using Joufflu.Samples.Controls;
using Joufflu.Samples.Views;
using Joufflu.Samples.Views.Data;
using Joufflu.Samples.Views.Feedback;
using Joufflu.Samples.Views.FileExplorer;
using Joufflu.Samples.Views.Inputs;
using Joufflu.Samples.Views.Natives.Actions;
using Joufflu.Samples.Views.Natives.DataDisplay;
using Joufflu.Samples.Views.Natives.DataInput;
using Joufflu.Samples.Views.Natives.Feedback;
using Joufflu.Samples.Views.Natives.Layout;
using Joufflu.Samples.Views.Natives.Navigation;
using Joufflu.Samples.Views.Navigation;
using Joufflu.Samples.Views.Navigation.Views;
using Joufflu.Samples.Views.Themes;
using Joufflu.Samples.Views.Toolkit;

namespace Joufflu.Samples.ViewModels;

/// <summary>
/// Shell view model: owns the shared navigation services and the pages the side menu can reach.
/// The menu items are declared in XAML and point at a page through its type; the
/// <see cref="Navigator"/> turns that type into the page instance via <see cref="ResolvePage"/>.
/// <para>
/// It is where the application is put together, so unlike the sample view models it knows the
/// views: each page is registered with the view drawing it, see <see cref="ViewTemplates"/>.
/// </para>
/// </summary>
public class ShellViewModel : ObservableObject, IDisposable
{
    public Overlayer Overlays { get; } = new();

    public ToastService Toasts { get; } = new();

    /// <summary>How to build each page, keyed by its own type, which is what the menu's <c>NavigationItem</c>s target.</summary>
    private readonly Dictionary<Type, Func<object>> _factories = new();

    /// <summary>Pages built so far: each is created the first time it is navigated to, then kept.</summary>
    private readonly Dictionary<Type, object> _pages = new();

    /// <summary>Every registered page as the search lists it, in the order of the side menu.</summary>
    private readonly List<PageEntry> _pageEntries = new();

    public Navigator Navigator { get; }

    /// <summary>The title bar search over the registered pages.</summary>
    public PageSearchViewModel PageSearch { get; }

    /// <summary>
    /// The view drawing each registered view model, as implicit <see cref="DataTemplate"/>s. Merged into the
    /// application resources so a page or an overlay content is drawn wherever it is shown.
    /// </summary>
    public ResourceDictionary ViewTemplates { get; } = new();

    /// <summary>
    /// Kept accessible so the window's <c>ToastContainer</c> can follow the corner its
    /// position sample picks.
    /// </summary>
    public ToastSamplesViewModel ToastSamples { get; }

    public ShellViewModel()
    {
        ToastSamples = new ToastSamplesViewModel(Toasts);

        // Registered in the order of the side menu, which the search keeps between equally good matches.
        // The titles repeat the menu labels, prefixed with their menu group when they have one.
        Register("Home", "What Joufflu is, its packages and how to set it up.",
            () => new HomeSamples());

        // Themes
        Register("Application shell", "The themed window, its title bar colours and the overlay and toast containers.",
            () => new ShellSamples());
        Register<ThemeSamplesViewModel, ThemeSamples>("Theme", "Switch between light, dark and custom themes at runtime.",
            () => new());
        Register<ThemeTokensViewModel, ThemeTokens>("Design tokens", "Every colour, brush and dimension a theme is made of.",
            () => new());

        // Natives
        Register<TypographySamplesViewModel, TypographySamples>("Typography", "Text sizes, headings, muted text and the named text styles.",
            () => new());
        Register<FontIconSamplesViewModel, FontIconSamples>("Font icon", "Lucide icons drawn as text, their sizes and colours, and the chevron.",
            () => new());
        Register("Card", "The card surfaces, built on the display tokens.",
            () => new CardSamples());

        // Native controls
        Register<ButtonSamplesViewModel, ButtonSamples>("Native controls / Buttons", "Solid, soft, outline and icon buttons, in every size.",
            () => new());
        Register<ToggleButtonSamplesViewModel, ToggleButtonSamples>("Native controls / Toggle buttons", "The toggle twins of the button styles and the icon toggle button.",
            () => new());

        Register<TextBoxSamplesViewModel, TextBoxSamples>("Native controls / Text box", "Text input in every size, with validation, password and multi-line.",
            () => new());
        Register<ComboBoxSamplesViewModel, ComboBoxSamples>("Native controls / Combo box", "Selection from a drop down list, editable or not.",
            () => new());
        Register<CheckBoxSamplesViewModel, CheckBoxSamples>("Native controls / Check box", "Checked, unchecked, indeterminate and disabled check boxes.",
            () => new());
        Register("Native controls / Radio button", "Groups of mutually exclusive options.",
            () => new RadioButtonSamples());
        Register<SliderSamplesViewModel, SliderSamples>("Native controls / Slider", "Pick a value on a track, with or without ticks.",
            () => new());
        Register<DatePickerSamplesViewModel, DatePickerSamples>("Native controls / Date picker", "Pick a date from a text box with a calendar popup.",
            () => new());
        Register<CalendarSamplesViewModel, CalendarSamples>("Native controls / Calendar", "An inline calendar to pick a date.",
            () => new());
        Register<ListBoxSamplesViewModel, ListBoxSamples>("Native controls / List box", "Lists with single or multiple selection.",
            () => new());

        Register("Native controls / Label", "Labels naming the field next to them.",
            () => new LabelSamples());
        Register<ListViewSamplesViewModel, ListViewSamples>("Native controls / List view", "A list shown as a table with grid view columns.",
            () => new());
        Register<TreeViewSamplesViewModel, TreeViewSamples>("Native controls / Tree view", "A hierarchy of expandable nodes, in every size.",
            () => new());
        Register<DataGridSamplesViewModel, DataGridSamples>("Native controls / Data grid", "Editable tables with column types, a compact size and row details.",
            () => new());

        Register<ProgressBarSamplesViewModel, ProgressBarSamples>("Native controls / Progress bar", "Determinate and indeterminate progress.",
            () => new());
        Register("Native controls / Status bar", "A bar of status items at the bottom of a window.",
            () => new StatusBarSamples());

        Register("Native controls / Group box", "Content grouped under a header in a frame.",
            () => new GroupBoxSamples());
        Register("Native controls / Expander", "A collapsible section opened from its header.",
            () => new ExpanderSamples());
        Register("Native controls / Scroll viewer", "Scrollable content with the themed scroll bars.",
            () => new ScrollViewerSamples());
        Register("Native controls / Grid splitter", "Panes resized by dragging the splitter between them.",
            () => new GridSplitterSamples());

        Register("Native controls / Menu", "A menu bar and a context menu.",
            () => new MenuSamples());
        Register("Native controls / Tab control", "Content split into tabs.",
            () => new TabControlSamples());
        Register("Native controls / Tool bar", "A row of compact commands.",
            () => new ToolBarSamples());
        Register("Native controls / Hyperlink", "Links inline in a text.",
            () => new HyperlinkSamples());

        // Toolkit
        Register<SizingSamplesViewModel, SizingSamples>("Sizing", "Size controls and text from xs to xl, and square icon buttons.",
            () => new());
        Register<SpacingSamplesViewModel, SpacingSamples>("Spacing", "Even gaps between the children of a panel.",
            () => new());
        Register<TooltipSamplesViewModel, TooltipSamples>("Tooltip & popup", "Themed tooltips, and any popup, placed on a side of their element and aligned on it.",
            () => new());
        Register<AnimateSamplesViewModel, AnimateSamples>("Animate", "Make any element bounce or spin.",
            () => new());
        Register<DropTargetSamplesViewModel, DropTargetSamples>("Drag and drop", "Turn any element into a drop target or a drag source.",
            () => new());
        // Its page links to the Design tokens one; the navigator is created below, before any page is resolved.
        Register<CustomControlSamplesViewModel, CustomControlSamples>("Create a custom control", "The tokens and states a control of your own needs to match the Joufflu ones.",
            () => new(() => Navigator?.Navigate(typeof(ThemeTokensViewModel))));

        // Feedback (Joufflu.Feedback library)
        Register<BadgeSamplesViewModel, BadgeSamples>("Feedback / Badge", "A pill in the semantic colours, to tag or count.",
            () => new());
        Register<SpinnerSamplesViewModel, SpinnerSamples>("Feedback / Spinner", "An indeterminate loading indicator.",
            () => new());
        Register<ToastSamplesViewModel, ToastSamples>("Feedback / Toasts", "Transient notifications, sticky or updated while loading.",
            () => ToastSamples);

        // Inputs (Joufflu.Inputs library)
        Register<NumericInputsSamplesViewModel, NumericInputsSamples>("Inputs / Numeric & format inputs", "Numeric, decimal and time span inputs with up and down buttons.",
            () => new());
        Register<SearchSamplesViewModel, SearchSamples>("Inputs / Search", "A search box reporting the query once typing settles.",
            () => new());
        Register<ComboBoxSearchSamplesViewModel, ComboBoxSearchSamples>("Inputs / Search combo box", "An editable combo box filtering its items as you type.",
            () => new());
        Register<ComboBoxTagsSamplesViewModel, ComboBoxTagsSamples>("Inputs / Combo box tags", "Pick several items, shown as removable tags.",
            () => new());
        Register<TextEditableSamplesViewModel, TextEditableSamples>("Inputs / Text editable", "A value that turns into a text box when edited.",
            () => new());
        Register<FilePickerSamplesViewModel, FilePickerSamples>("Inputs / File picker", "Select a file through the system dialog or a typed path.",
            () => new());
        Register<ColorPickerSamplesViewModel, ColorPickerSamples>("Inputs / Color picker", "A hex field with a saturation, hue and alpha popup.",
            () => new());
        Register<DropdownSamplesViewModel, DropdownSamples>("Inputs / Dropdown", "Turn any toggle button into a dropdown with a themed popup.",
            () => new());

        // Navigation (Joufflu.Navigation library)
        Register<NavigationMenuSamplesViewModel, NavigationMenuSamples>("Navigation / Navigation menu", "A collapsible side menu with groups, driven by a navigator.",
            () => new());
        Register<OverlaySamplesViewModel, OverlaySamples>("Navigation / Overlays", "Stacked modal dialogs, confirmations and awaited results.",
            () => new(Overlays, Toasts));
        Register<PagingSamplesViewModel, PagingSamples>("Navigation / Paging", "A page selector to page through a list or a data grid.",
            () => new());
        // Contents the overlay sample opens: not pages, so they only need their view.
        RegisterView<ConfirmViewModel, ConfirmView>();
        RegisterView<SampleFormViewModel, SampleFormView>();

        // Data (Joufflu.Data library)
        Register<DataFillSamplesViewModel, DataFillSamples>("Data / Data fill", "Fill in JSON data built from a schema.",
            () => new());
        Register<DataEditSamplesViewModel, DataEditSamples>("Data / Data edit", "Build JSON data from scratch as a tree.",
            () => new());
        Register<DataDisplaySamplesViewModel, DataDisplaySamples>("Data / Data display", "Show JSON data read only, a large file included.",
            () => new());

        // File explorer (Joufflu.FileExplorer library)
        Register<ExplorerListSamplesViewModel, ExplorerListSamples>("File explorer / List", "The content of a folder as a list, with extra columns and a filter.",
            () => new(Toasts));
        Register<ExplorerTreeSamplesViewModel, ExplorerTreeSamples>("File explorer / Tree", "The folder hierarchy as a tree.",
            () => new(Toasts));
        Register<ExplorerSamplesViewModel, ExplorerSamples>("File explorer / Explorer", "The complete explorer: control bar, tree, list and status bar.",
            () => new(Toasts));
        Register<ExplorerSourcesSamplesViewModel, ExplorerSourcesSamples>("File explorer / Sources", "Feed the explorer from the file system, a live watcher or nodes of your own.",
            () => new(Toasts));

        Navigator = new Navigator(ResolvePage);
        PageSearch = new PageSearchViewModel(_pageEntries, Navigator);

        Navigator.Navigate(typeof(HomeSamples));
    }

    /// <summary>
    /// Registers a page that is its own view, keyed by the type its factory returns so key and instance cannot
    /// disagree, and lists it in the search under [title] and [summary].
    /// </summary>
    private void Register<TPage>(string title, string summary, Func<TPage> create) where TPage : notnull
    {
        _factories[typeof(TPage)] = () => create();
        _pageEntries.Add(new PageEntry(typeof(TPage), title, summary));
    }

    /// <summary>Registers a page view model along with the view drawing it.</summary>
    private void Register<TViewModel, TView>(string title, string summary, Func<TViewModel> create)
        where TViewModel : notnull
        where TView : FrameworkElement, new()
    {
        Register(title, summary, create);
        RegisterView<TViewModel, TView>();
    }

    /// <summary>Draws every <typeparamref name="TViewModel"/> with a new <typeparamref name="TView"/>.</summary>
    private void RegisterView<TViewModel, TView>() where TView : FrameworkElement, new()
    {
        var template = new DataTemplate(typeof(TViewModel))
        {
            VisualTree = new FrameworkElementFactory(typeof(TView)),
        };
        ViewTemplates.Add(template.DataTemplateKey, template);
    }

    /// <summary>Maps a menu item's target type to its page instance, building it on first use.</summary>
    private object ResolvePage(Type target)
    {
        if (_pages.TryGetValue(target, out object? page))
            return page;

        if (!_factories.TryGetValue(target, out Func<object>? create))
            throw new InvalidOperationException($"No sample page registered for {target.Name}.");

        page = create();
        _pages[target] = page;
        return page;
    }

    /// <summary>Disposes the pages built so far that hold resources, a watched directory for instance.</summary>
    public void Dispose()
    {
        foreach (IDisposable page in _pages.Values.OfType<IDisposable>())
            page.Dispose();
        _pages.Clear();
    }
}
