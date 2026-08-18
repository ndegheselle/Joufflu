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
public class NavigationTitle : ContentControl
{
    static NavigationTitle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationTitle),
            new FrameworkPropertyMetadata(typeof(NavigationTitle)));
    }

    public NavigationTitle()
    { }
}

/// <summary>
/// A clickable entry hosted by a <see cref="NavigationMenu"/>. Its <see cref="ContentControl.Content"/>
/// is the expanded content (title, badges, …); the <see cref="Icon"/> is shown when the menu is
/// collapsed to an icons-only rail. Selecting it navigates to the page resolved from <see cref="TargetType"/>.
/// </summary>
public class NavigationItem : ContentControl
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(object), typeof(NavigationItem), new PropertyMetadata(null));

    public static readonly DependencyProperty TargetTypeProperty = DependencyProperty.Register(
        nameof(TargetType), typeof(Type), typeof(NavigationItem), new PropertyMetadata(null));

    private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsSelected), typeof(bool), typeof(NavigationItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

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

    /// <summary>
    /// Type of the page (view model) this item navigates to. The <see cref="INavigator"/> turns it
    /// into the actual page instance, and the item shows as selected while a page of that type is current.
    /// </summary>
    public Type? TargetType
    {
        get => (Type?)GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        internal set => SetValue(IsSelectedPropertyKey, value);
    }
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
/// XAML. Items point at a page through a <see cref="NavigationItem.TargetType"/>, which the
/// <see cref="Navigator"/> maps to the actual view model. The menu can collapse to an icons-only rail.
/// </summary>
public partial class NavigationMenu : ItemsControl
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(NavigationMenu), new PropertyMetadata(null));

    public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register(
        nameof(IsCollapsed), typeof(bool), typeof(NavigationMenu), new PropertyMetadata(false));

    public static readonly DependencyProperty NavigatorProperty = DependencyProperty.Register(
        nameof(Navigator), typeof(INavigator), typeof(NavigationMenu),
        new PropertyMetadata(null, OnNavigatorChanged));

    static NavigationMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NavigationMenu),
            new FrameworkPropertyMetadata(typeof(NavigationMenu)));
    }

    public NavigationMenu()
    {
        ToggleCollapseCommand = new RelayCommand(() => IsCollapsed = !IsCollapsed);

        // Release the Navigator subscription while off the visual tree so a long-lived Navigator
        // cannot keep a removed menu (and its subtree) alive.
        Loaded += (_, _) => Attach(Navigator);
        Unloaded += (_, _) => Detach(Navigator);
    }

    /// <summary>
    /// Optional content shown at the top of the menu (typically a logo or title). Hidden when the
    /// menu is collapsed. Left unset, the slot takes no space.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Flips <see cref="IsCollapsed"/>.</summary>
    public ICommand ToggleCollapseCommand { get; }

    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public INavigator? Navigator
    {
        get => (INavigator?)GetValue(NavigatorProperty);
        set => SetValue(NavigatorProperty, value);
    }

    private static void OnNavigatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var menu = (NavigationMenu)d;
        menu.Detach(e.OldValue as INavigator);
        menu.Attach(e.NewValue as INavigator);
    }

    /// <summary>
    /// Subscribes to <paramref name="navigator"/> and syncs the selection. Idempotent, so the
    /// property change and the <c>Loaded</c> event can both call it without doubling the handler.
    /// </summary>
    private void Attach(INavigator? navigator)
    {
        if (navigator == null)
            return;

        navigator.Navigated -= OnNavigated;
        navigator.Navigated += OnNavigated;
        UpdateSelection(Items, navigator.CurrentPage);
    }

    private void Detach(INavigator? navigator)
    {
        if (navigator != null)
            navigator.Navigated -= OnNavigated;
    }

    private void OnNavigated(object? sender, object? page) => UpdateSelection(Items, page);

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateSelection(Items, Navigator?.CurrentPage);
    }

    [RelayCommand]
    private void Select(NavigationItem? item)
    {
        if (item?.TargetType == null)
            return;
        Navigator?.Navigate(item.TargetType);
    }

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
                    item.IsSelected = item.TargetType == currentPage?.GetType();
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
