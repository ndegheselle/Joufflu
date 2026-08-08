using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// Kinds of node a control displays, combinable so a control can show files, directories or both.
/// </summary>
[Flags]
public enum ExplorerNodeKinds
{
    None = 0,
    Files = 1,
    Directories = 2,
    All = Files | Directories
}

public static class ExplorerNodeKindsExtensions
{
    /// <summary>Whether <paramref name="node"/> is one of the kinds in <paramref name="kinds"/>.</summary>
    public static bool Includes(this ExplorerNodeKinds kinds, IExplorerNode node)
        => node is IExplorerDirectory
            ? kinds.HasFlag(ExplorerNodeKinds.Directories)
            : kinds.HasFlag(ExplorerNodeKinds.Files);
}

/// <summary>
/// Base of the explorer controls : the loader whose content they display and through which they navigate. Several
/// controls share the same loader, so they all show the same opened directory.
/// </summary>
public abstract class ExplorerControl : Control
{
    #region Dependency Property
    public static readonly DependencyProperty LoaderProperty = DependencyProperty.Register(
        nameof(Loader),
        typeof(IExplorerLoader),
        typeof(ExplorerControl),
        new PropertyMetadata(null));
    #endregion

    public IExplorerLoader? Loader
    {
        get => (IExplorerLoader?)GetValue(LoaderProperty);
        set => SetValue(LoaderProperty, value);
    }
}

/// <summary>
/// Behaviour shared by the controls displaying explorer nodes (<see cref="ExplorerList"/>,
/// <see cref="ExplorerTree"/>) : opening a directory on double click, the context menu of a node and its
/// selection. A derived control only provides the <see cref="ItemsControl"/> template part displaying the nodes
/// and reads its selection.
/// </summary>
[TemplatePart(Name = PartItemsHost, Type = typeof(ItemsControl))]
public abstract class ExplorerNodesControl : ExplorerControl
{
    protected const string PartItemsHost = "PART_ItemsHost";

    /// <summary>
    /// Single menu instance filled on opening : WPF captures the <see cref="FrameworkElement.ContextMenu"/> value
    /// before raising <see cref="FrameworkElement.ContextMenuOpening"/>, so the instance can't be replaced there.
    /// </summary>
    private readonly ContextMenu _contextMenu = new();

    #region Dependency Property
    private static readonly DependencyPropertyKey SelectedNodesPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SelectedNodes),
        typeof(IReadOnlyList<IExplorerNode>),
        typeof(ExplorerNodesControl),
        new PropertyMetadata(Array.Empty<IExplorerNode>()));

    public static readonly DependencyProperty SelectedNodesProperty
        = SelectedNodesPropertyKey.DependencyProperty;

    public static readonly DependencyProperty VisibleNodesProperty = DependencyProperty.Register(
        nameof(VisibleNodes),
        typeof(ExplorerNodeKinds),
        typeof(ExplorerNodesControl),
        new FrameworkPropertyMetadata(ExplorerNodeKinds.All, OnVisibleNodesChanged));
    #endregion

    /// <summary>
    /// Kinds of node the control shows, <see cref="ExplorerNodeKinds.All"/> by default. Set it to
    /// <see cref="ExplorerNodeKinds.Directories"/> or <see cref="ExplorerNodeKinds.Files"/> to display only one.
    /// </summary>
    public ExplorerNodeKinds VisibleNodes
    {
        get => (ExplorerNodeKinds)GetValue(VisibleNodesProperty);
        set => SetValue(VisibleNodesProperty, value);
    }

    private static void OnVisibleNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ExplorerNodesControl)d).OnVisibleNodesChanged();

    /// <summary>Re-applies <see cref="VisibleNodes"/> when it changes ; a derived control overrides it as needed.</summary>
    protected virtual void OnVisibleNodesChanged() { }

    /// <summary>
    /// Control displaying the nodes, taken from the <see cref="PartItemsHost"/> template part.
    /// </summary>
    protected ItemsControl? ItemsHost { get; private set; }

    /// <summary>
    /// Nodes selected in <see cref="ItemsHost"/>, empty when nothing is selected. Bindable, so that a status bar
    /// can show how many of them there are.
    /// </summary>
    public IReadOnlyList<IExplorerNode> SelectedNodes
        => (IReadOnlyList<IExplorerNode>)GetValue(SelectedNodesProperty);

    /// <summary>
    /// Reads the selection of <see cref="ItemsHost"/>, which only a derived control knows how to reach.
    /// </summary>
    protected abstract IEnumerable<IExplorerNode> GetSelectedNodes();

    /// <summary>
    /// Publishes the selection of <see cref="ItemsHost"/> in <see cref="SelectedNodes"/>. A derived control calls
    /// it whenever its host reports a selection change.
    /// </summary>
    protected void UpdateSelectedNodes() => SetValue(SelectedNodesPropertyKey, GetSelectedNodes().ToArray());

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (ItemsHost != null)
        {
            ItemsHost.RemoveHandler(
                MouseLeftButtonDownEvent,
                (MouseButtonEventHandler)OnItemsHostMouseLeftButtonDown);
            ItemsHost.ContextMenuOpening -= OnItemsHostContextMenuOpening;
            ItemsHost.ContextMenu = null;
        }

        ItemsHost = GetTemplateChild(PartItemsHost) as ItemsControl;

        if (ItemsHost != null)
        {
            // An item container handles the click to select itself, hence handledEventsToo.
            ItemsHost.AddHandler(
                MouseLeftButtonDownEvent,
                (MouseButtonEventHandler)OnItemsHostMouseLeftButtonDown,
                true);
            ItemsHost.ContextMenuOpening += OnItemsHostContextMenuOpening;
            ItemsHost.ContextMenu = _contextMenu;
            _contextMenu.PlacementTarget = ItemsHost;
        }

        UpdateSelectedNodes();
    }

    #region UI events
    /// <summary>
    /// Detects a double click on a node.
    /// </summary>
    /// <remarks>
    /// <see cref="Control.MouseDoubleClick"/> is not used : every <see cref="Control"/> of the route raises its
    /// own, so a single double click would be reported once per ancestor container.
    /// </remarks>
    private void OnItemsHostMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2)
            return;

        var container = GetContainerAt(e.OriginalSource as DependencyObject);
        if (container?.DataContext is not IExplorerNode node)
            return;

        e.Handled = OnNodeDoubleClick(node, container);
    }

    private void OnItemsHostContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        _contextMenu.Items.Clear();
        _contextMenu.DataContext = null;

        var node = GetNodeAt(e.OriginalSource as DependencyObject);
        if (node == null)
        {
            e.Handled = true;
            return;
        }

        var nodes = GetMenuNodes(node);
        var template = FindContextMenuTemplate(
            node.GetType(),
            nodes.Count > 1 ? MenuScope.Multiple : MenuScope.Single);

        if (template?.LoadContent() is not ContextMenu menu)
        {
            e.Handled = true;
            return;
        }

        MoveItems(menu, _contextMenu);
        _contextMenu.DataContext = new ExplorerMenuContext(Loader, nodes);
    }
    #endregion

    /// <summary>
    /// Reacts to a double click on a node, opening a folder by default. Returns whether the double click has been
    /// handled.
    /// </summary>
    /// <param name="container">Item container displaying <paramref name="node"/>.</param>
    protected virtual bool OnNodeDoubleClick(IExplorerNode node, FrameworkElement container)
    {
        if (node is not IExplorerDirectory directory)
            return false;

        Loader?.Open(directory);
        return true;
    }

    /// <summary>
    /// Moves the items of the menu loaded from a template to the persistent menu.
    /// </summary>
    private static void MoveItems(ContextMenu source, ContextMenu destination)
    {
        while (source.Items.Count > 0)
        {
            var item = source.Items[0];
            source.Items.RemoveAt(0);
            destination.Items.Add(item);
        }
    }

    /// <summary>
    /// Selected nodes with the one the menu was opened on first, or only that node when it isn't selected.
    /// </summary>
    private List<IExplorerNode> GetMenuNodes(IExplorerNode node)
    {
        var nodes = GetSelectedNodes().ToList();
        if (!nodes.Remove(node))
            return [node];

        nodes.Insert(0, node);
        return nodes;
    }

    /// <summary>
    /// Searches the context menu template of a node type, from the most specific type to <see cref="object"/>.
    /// </summary>
    private DataTemplate? FindContextMenuTemplate(Type nodeType, MenuScope scope)
    {
        foreach (var type in GetTypeCandidates(nodeType))
        {
            if (TryFindResource(new ContextMenuTemplateKey(type) { Scope = scope }) is DataTemplate template)
                return template;
        }

        return null;
    }

    private static IEnumerable<Type> GetTypeCandidates(Type nodeType)
    {
        for (Type? type = nodeType; type != null && type != typeof(object); type = type.BaseType)
            yield return type;

        foreach (var interfaceType in nodeType.GetInterfaces())
            yield return interfaceType;

        yield return typeof(object);
    }

    /// <summary>
    /// Node displayed by the item container of <paramref name="source"/>, whatever its nesting level.
    /// </summary>
    private IExplorerNode? GetNodeAt(DependencyObject? source)
        => GetContainerAt(source)?.DataContext as IExplorerNode;

    /// <summary>
    /// Item container displaying a node, from an element inside of it, whatever its nesting level.
    /// </summary>
    /// <remarks>
    /// <see cref="ItemsControl.ContainerFromElement(ItemsControl, DependencyObject)"/> is not used : it only
    /// recognizes the containers generated by <see cref="ItemsHost"/> itself, and in a hierarchy a container is
    /// generated by its parent item, so a nested node would resolve to its top level ancestor.
    /// </remarks>
    private FrameworkElement? GetContainerAt(DependencyObject? source)
    {
        if (ItemsHost == null)
            return null;

        for (DependencyObject? element = source; element != null && element != ItemsHost;
            element = GetParent(element))
        {
            if (element is FrameworkElement { DataContext: IExplorerNode } container
                && ItemsControl.ItemsControlFromItemContainer(element) != null)
                return container;
        }

        return null;
    }

    /// <summary>
    /// Parent of <paramref name="element"/> in the visual tree, falling back to the logical one for the elements
    /// that aren't part of it (the inline content of a text, ...).
    /// </summary>
    private static DependencyObject? GetParent(DependencyObject element)
        => element is Visual ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element);
}
