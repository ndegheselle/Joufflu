using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Navigation.Controls;

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
/// An expandable entry hosted by a <see cref="NavigationMenu"/>. It looks like a
/// <see cref="NavigationItem"/> (its <see cref="HeaderedItemsControl.Header"/> and <see cref="Icon"/>
/// form the row) but instead of navigating it toggles <see cref="IsExpanded"/> to reveal its child
/// items. Children may themselves be <see cref="NavigationItem"/>s, <see cref="NavigationTitle"/>s or
/// nested <see cref="NavigationGroup"/>s.
/// </summary>
public class NavigationGroup : HeaderedItemsControl
{
    static NavigationGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationGroup),
            new FrameworkPropertyMetadata(typeof(NavigationGroup)));
    }

    public NavigationGroup()
    {
        ToggleCommand = new RelayCommand(() => IsExpanded = !IsExpanded);
    }

    /// <summary>Flips <see cref="IsExpanded"/>.</summary>
    public ICommand ToggleCommand { get; }

    /// <summary>Content shown when the menu is collapsed (typically a <c>FontIcon</c>).</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(object), typeof(NavigationGroup), new PropertyMetadata(null));

    /// <summary>Whether the child items are shown. Closed by default.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(NavigationGroup), new PropertyMetadata(false));
}

/// <summary>
/// Side menu that plugs into an <see cref="INavigator"/>. Its <see cref="NavigationItem"/>,
/// <see cref="NavigationGroup"/> and <see cref="NavigationTitle"/> children are declared directly in
/// XAML. Items point at a page
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

    /// <summary>
    /// Optional content shown at the top of the menu (typically a logo or title). Hidden when the
    /// menu is collapsed. Left unset, the slot takes no space.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(NavigationMenu), new PropertyMetadata(null));

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

    private void UpdateSelection(object? currentPage) => UpdateSelection(Items, currentPage);

    /// <summary>
    /// Walks the item tree, flagging the item that resolves to <paramref name="currentPage"/> and
    /// expanding any group that contains it. Returns whether the selected item was found in this subtree.
    /// </summary>
    private bool UpdateSelection(IEnumerable items, object? currentPage)
    {
        bool containsSelection = false;
        foreach (object? element in items)
        {
            switch (element)
            {
                case NavigationItem item:
                    item.IsSelected = currentPage != null && ReferenceEquals(ResolvePage(item), currentPage);
                    containsSelection |= item.IsSelected;
                    break;
                case NavigationGroup group when UpdateSelection(group.Items, currentPage):
                    group.IsExpanded = true;
                    containsSelection = true;
                    break;
            }
        }
        return containsSelection;
    }
}
