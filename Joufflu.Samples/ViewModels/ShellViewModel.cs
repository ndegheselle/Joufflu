using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
using Joufflu.Navigation;
using Joufflu.Navigation.Controls;
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

    public Navigator Navigator { get; }

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

        // Native controls
        Register<ButtonSamplesViewModel, ButtonSamples>(() => new());
        Register<ToggleButtonSamplesViewModel, ToggleButtonSamples>(() => new());

        Register<TextBoxSamplesViewModel, TextBoxSamples>(() => new());
        Register<ComboBoxSamplesViewModel, ComboBoxSamples>(() => new());
        Register<CheckBoxSamplesViewModel, CheckBoxSamples>(() => new());
        Register(() => new RadioButtonSamples());
        Register<SliderSamplesViewModel, SliderSamples>(() => new());
        Register<DatePickerSamplesViewModel, DatePickerSamples>(() => new());
        Register<CalendarSamplesViewModel, CalendarSamples>(() => new());
        Register<ListBoxSamplesViewModel, ListBoxSamples>(() => new());

        Register<TypographySamplesViewModel, TypographySamples>(() => new());
        Register<FontIconSamplesViewModel, FontIconSamples>(() => new());
        Register(() => new LabelSamples());
        Register<ListViewSamplesViewModel, ListViewSamples>(() => new());
        Register<TreeViewSamplesViewModel, TreeViewSamples>(() => new());
        Register<DataGridSamplesViewModel, DataGridSamples>(() => new());

        Register<ProgressBarSamplesViewModel, ProgressBarSamples>(() => new());
        Register(() => new StatusBarSamples());

        Register(() => new CardSamples());
        Register(() => new GroupBoxSamples());
        Register(() => new ExpanderSamples());
        Register(() => new ScrollViewerSamples());
        Register(() => new GridSplitterSamples());

        Register(() => new MenuSamples());
        Register(() => new TabControlSamples());
        Register(() => new ToolBarSamples());
        Register(() => new HyperlinkSamples());

        // Inputs (Joufflu.Inputs library)
        Register<NumericInputsSamplesViewModel, NumericInputsSamples>(() => new());
        Register<SearchSamplesViewModel, SearchSamples>(() => new());
        Register<ComboBoxSearchSamplesViewModel, ComboBoxSearchSamples>(() => new());
        Register<ComboBoxTagsSamplesViewModel, ComboBoxTagsSamples>(() => new());
        Register<TextEditableSamplesViewModel, TextEditableSamples>(() => new());
        Register<FilePickerSamplesViewModel, FilePickerSamples>(() => new());
        Register<ColorPickerSamplesViewModel, ColorPickerSamples>(() => new());
        Register<DropdownSamplesViewModel, DropdownSamples>(() => new());

        // Navigation (Joufflu.Navigation library)
        Register<NavigationMenuSamplesViewModel, NavigationMenuSamples>(() => new());
        Register<OverlaySamplesViewModel, OverlaySamples>(() => new(Overlays, Toasts));
        Register<PagingSamplesViewModel, PagingSamples>(() => new());
        // Contents the overlay sample opens: not pages, so they only need their view.
        RegisterView<ConfirmViewModel, ConfirmView>();
        RegisterView<SampleFormViewModel, SampleFormView>();

        // Data (Joufflu.Data library)
        Register<DataFillSamplesViewModel, DataFillSamples>(() => new());
        Register<DataEditSamplesViewModel, DataEditSamples>(() => new());
        Register<DataDisplaySamplesViewModel, DataDisplaySamples>(() => new());

        // File explorer (Joufflu.FileExplorer library)
        Register<ExplorerSamplesViewModel, ExplorerSamples>(() => new(Toasts));
        Register<ExplorerListSamplesViewModel, ExplorerListSamples>(() => new(Toasts));
        Register<ExplorerTreeSamplesViewModel, ExplorerTreeSamples>(() => new(Toasts));
        Register<ExplorerSourcesSamplesViewModel, ExplorerSourcesSamples>(() => new(Toasts));

        // Custom controls
        Register<BadgeSamplesViewModel, BadgeSamples>(() => new());
        Register<SpinnerSamplesViewModel, SpinnerSamples>(() => new());
        Register<ToastSamplesViewModel, ToastSamples>(() => ToastSamples);
        Register<TooltipSamplesViewModel, TooltipSamples>(() => new());

        // Toolkit
        Register<SizingSamplesViewModel, SizingSamples>(() => new());
        Register<SpacingSamplesViewModel, SpacingSamples>(() => new());
        Register<DropTargetSamplesViewModel, DropTargetSamples>(() => new());
        Register<AnimateSamplesViewModel, AnimateSamples>(() => new());
        Register<CustomInputSamplesViewModel, CustomInputSamples>(() => new());

        // Themes
        Register(() => new ShellSamples());
        Register<ThemeSamplesViewModel, ThemeSamples>(() => new());
        Register<ThemeTokensViewModel, ThemeTokens>(() => new());
        Register<ThemeCustomizerViewModel, ThemeCustomizer>(() => new());

        Navigator = new Navigator(ResolvePage);

        Navigator.Navigate(typeof(ShellSamples));
    }

    /// <summary>Registers a page that is its own view, keyed by the type its factory returns so key and instance cannot disagree.</summary>
    private void Register<TPage>(Func<TPage> create) where TPage : notnull
        => _factories[typeof(TPage)] = () => create();

    /// <summary>Registers a page view model along with the view drawing it.</summary>
    private void Register<TViewModel, TView>(Func<TViewModel> create)
        where TViewModel : notnull
        where TView : FrameworkElement, new()
    {
        Register(create);
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
