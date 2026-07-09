using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.Navigation;

/// <summary>
/// A section title hosted by a <see cref="NavigationMenu"/>. Rendered as a label when the menu
/// is expanded and as a simple separator line when it is collapsed.
/// </summary>
public class NavigationTitle : Control
{
    static NavigationTitle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationTitle),
            new FrameworkPropertyMetadata(typeof(NavigationTitle)));
    }

    public NavigationTitle()
    { }

    public NavigationTitle(string title) => Title = title;

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(NavigationTitle), new PropertyMetadata(""));
}

/// <summary>
/// A clickable entry hosted by a <see cref="NavigationMenu"/>. Its <see cref="ContentControl.Content"/>
/// is the expanded content (title, badges, …); the <see cref="Icon"/> is shown when the menu is
/// collapsed to an icons-only rail. Selecting it navigates to the page resolved from <see cref="Target"/>.
/// </summary>
public class NavigationItem : ContentControl
{
    static NavigationItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationItem),
            new FrameworkPropertyMetadata(typeof(NavigationItem)));
    }

    /// <summary>Content shown when the menu is collapsed (typically a <c>FontIcon</c>).</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(object), typeof(NavigationItem), new PropertyMetadata(null));

    /// <summary>Text key resolved to a page (view model) through <see cref="NavigationMenu.TargetResolver"/>.</summary>
    public string? Target
    {
        get => (string?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
        nameof(Target), typeof(string), typeof(NavigationItem), new PropertyMetadata(null));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        internal set => SetValue(IsSelectedPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsSelected), typeof(bool), typeof(NavigationItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;
}

/// <summary>
/// Side menu that plugs into an <see cref="INavigator"/>. Its <see cref="NavigationItem"/> and
/// <see cref="NavigationTitle"/> children are declared directly in XAML. Items point at a page
/// through a text <see cref="NavigationItem.Target"/>, mapped to the actual view model by
/// <see cref="TargetResolver"/>. The menu can collapse to an icons-only rail.
/// </summary>
public class NavigationMenu : ItemsControl
{
    /// <summary>Pages resolved from each item's target, cached so selection stays stable.</summary>
    private readonly Dictionary<NavigationItem, object?> _resolvedPages = new();

    static NavigationMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationMenu),
            new FrameworkPropertyMetadata(typeof(NavigationMenu)));
    }

    public NavigationMenu()
    {
        SelectCommand = new RelayCommand<NavigationItem>(OnSelect);
        ToggleCollapseCommand = new RelayCommand(() => IsCollapsed = !IsCollapsed);
    }

    /// <summary>Selects (navigates to) the <see cref="NavigationItem"/> passed as parameter.</summary>
    public ICommand SelectCommand { get; }

    /// <summary>Flips <see cref="IsCollapsed"/>.</summary>
    public ICommand ToggleCollapseCommand { get; }

    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register(
        nameof(IsCollapsed), typeof(bool), typeof(NavigationMenu), new PropertyMetadata(false));

    public INavigator? Navigator
    {
        get => (INavigator?)GetValue(NavigatorProperty);
        set => SetValue(NavigatorProperty, value);
    }

    public static readonly DependencyProperty NavigatorProperty = DependencyProperty.Register(
        nameof(Navigator), typeof(INavigator), typeof(NavigationMenu),
        new PropertyMetadata(null, OnNavigatorChanged));

    /// <summary>
    /// Turns an item's text <see cref="NavigationItem.Target"/> into the page (view model) to
    /// navigate to. Usually bound to a method on the shell view model.
    /// </summary>
    public Func<string, object?>? TargetResolver
    {
        get => (Func<string, object?>?)GetValue(TargetResolverProperty);
        set => SetValue(TargetResolverProperty, value);
    }

    public static readonly DependencyProperty TargetResolverProperty = DependencyProperty.Register(
        nameof(TargetResolver), typeof(Func<string, object?>), typeof(NavigationMenu), new PropertyMetadata(null));

    private static void OnNavigatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var menu = (NavigationMenu)d;

        if (e.OldValue is INavigator oldNavigator)
            oldNavigator.Navigated -= menu.OnNavigated;

        if (e.NewValue is INavigator newNavigator)
        {
            newNavigator.Navigated += menu.OnNavigated;
            menu.UpdateSelection(newNavigator.CurrentPage);
        }
    }

    private void OnNavigated(object? sender, object? page) => UpdateSelection(page);

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        _resolvedPages.Clear();
        UpdateSelection(Navigator?.CurrentPage);
    }

    private void OnSelect(NavigationItem? item)
    {
        object? page = item == null ? null : ResolvePage(item);
        if (page != null)
            Navigator?.Navigate(page);
    }

    /// <summary>Resolves (and caches) the page a navigation item points at.</summary>
    private object? ResolvePage(NavigationItem item)
    {
        if (_resolvedPages.TryGetValue(item, out object? cached))
            return cached;

        object? page = item.Target is { Length: > 0 } target ? TargetResolver?.Invoke(target) : null;
        _resolvedPages[item] = page;
        return page;
    }

    private void UpdateSelection(object? currentPage)
    {
        foreach (NavigationItem item in Items.OfType<NavigationItem>())
            item.IsSelected = currentPage != null && ReferenceEquals(ResolvePage(item), currentPage);
    }
}
