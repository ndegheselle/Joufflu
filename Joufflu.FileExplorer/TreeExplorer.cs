using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer;

/// <summary>
/// Display a root element and its children as a tree. Opening a folder (double click, Enter or
/// <see cref="ExplorerCommands.Open"/>) expands it, opening anything else raises <see cref="NodeOpened"/>.
/// </summary>
/// <remarks>
/// Nodes are displayed with the data template matching their type, the default ones are provided for
/// <see cref="ExplorerFile"/> and <see cref="ExplorerFolder"/> (see
/// <see cref="ExplorerResources.FileTemplate"/> and <see cref="ExplorerResources.FolderTemplate"/>).
/// A custom folder template has to be a <see cref="HierarchicalDataTemplate"/> for the tree to be able
/// to display its children.
/// </remarks>
public class TreeExplorer : TreeView, IExplorerControl
{
    static TreeExplorer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TreeExplorer),
            new FrameworkPropertyMetadata(typeof(TreeExplorer)));
    }

    public TreeExplorer()
    {
        Explorer.Initialize(this);
    }

    /// <summary>Raised when a node is opened, before expanding it if it is a folder.</summary>
    public event EventHandler<ExplorerNodeEventArgs>? NodeOpened;

    #region Properties
    /// <summary>Root element of the explorer, displayed as the top level node of the tree.</summary>
    public IExplorerFolder? Root
    {
        get => (IExplorerFolder?)GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(
        nameof(Root),
        typeof(IExplorerFolder),
        typeof(TreeExplorer),
        new PropertyMetadata(null, OnRootChanged));

    /// <summary>Context menu used for every node, before falling back on the default resources.</summary>
    public ContextMenu? ItemContextMenu
    {
        get => (ContextMenu?)GetValue(ItemContextMenuProperty);
        set => SetValue(ItemContextMenuProperty, value);
    }

    public static readonly DependencyProperty ItemContextMenuProperty = DependencyProperty.Register(
        nameof(ItemContextMenu),
        typeof(ContextMenu),
        typeof(TreeExplorer),
        new PropertyMetadata(null));

    /// <summary>
    /// Context menu used when more than one node is selected, only used if the tree is given a multi
    /// selection behavior (<see cref="TreeView"/> only selects one node at a time).
    /// </summary>
    public ContextMenu? SelectionContextMenu
    {
        get => (ContextMenu?)GetValue(SelectionContextMenuProperty);
        set => SetValue(SelectionContextMenuProperty, value);
    }

    public static readonly DependencyProperty SelectionContextMenuProperty = DependencyProperty.Register(
        nameof(SelectionContextMenu),
        typeof(ContextMenu),
        typeof(TreeExplorer),
        new PropertyMetadata(null));

    /// <summary>Resolve a context menu per node, takes precedence over every other menu.</summary>
    public ExplorerContextMenuSelector? ItemContextMenuSelector
    {
        get => (ExplorerContextMenuSelector?)GetValue(ItemContextMenuSelectorProperty);
        set => SetValue(ItemContextMenuSelectorProperty, value);
    }

    public static readonly DependencyProperty ItemContextMenuSelectorProperty = DependencyProperty.Register(
        nameof(ItemContextMenuSelector),
        typeof(ExplorerContextMenuSelector),
        typeof(TreeExplorer),
        new PropertyMetadata(null));

    /// <summary>
    /// Handles the <see cref="ExplorerCommands"/>, defaults to <see cref="FileSystemCommandHandler"/>
    /// which applies them on the file system.
    /// </summary>
    public IExplorerCommandHandler? CommandHandler
    {
        get => (IExplorerCommandHandler?)GetValue(CommandHandlerProperty);
        set => SetValue(CommandHandlerProperty, value);
    }

    public static readonly DependencyProperty CommandHandlerProperty = DependencyProperty.Register(
        nameof(CommandHandler),
        typeof(IExplorerCommandHandler),
        typeof(TreeExplorer),
        new PropertyMetadata(FileSystemCommandHandler.Default));

    /// <summary>The tree has no navigation, the actions that need a folder apply to the root.</summary>
    public IExplorerFolder? CurrentFolder => Root;

    /// <summary>Selected node, as a list to share the behaviors of the other explorer controls.</summary>
    public IReadOnlyList<IExplorerNode> SelectedNodes
        => SelectedItem is IExplorerNode node ? new[] { node } : Array.Empty<IExplorerNode>();
    #endregion

    private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        TreeExplorer explorer = (TreeExplorer)d;
        explorer.ItemsSource = e.NewValue is IExplorerFolder root ? new[] { root } : null;
        explorer.ExpandRoot();
    }

    /// <summary>
    /// Expand the root as soon as its container exists, so that its children are visible right away.
    /// </summary>
    private void ExpandRoot()
    {
        if (ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem container)
        {
            container.IsExpanded = true;
            return;
        }

        ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;
        ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
    }

    private void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            return;

        ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;
        if (ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem container)
            container.IsExpanded = true;
    }

    public void OpenNode(IExplorerNode node)
    {
        NodeOpened?.Invoke(this, new ExplorerNodeEventArgs(node));

        if (node is not IExplorerFolder)
            return;

        // Only expand : a double click on the container may already have toggled it
        TreeViewItem? container = FindContainer(this, node);
        if (container != null)
            container.IsExpanded = true;
    }

    /// <summary>
    /// Find the (realized) container of a node, looking through the whole tree.
    /// </summary>
    private static TreeViewItem? FindContainer(ItemsControl parent, object node)
    {
        if (parent.ItemContainerGenerator.ContainerFromItem(node) is TreeViewItem container)
            return container;

        foreach (object? item in parent.Items)
        {
            if (item == null || parent.ItemContainerGenerator.ContainerFromItem(item) is not TreeViewItem child)
                continue;

            TreeViewItem? found = FindContainer(child, node);
            if (found != null)
                return found;
        }
        return null;
    }
}
