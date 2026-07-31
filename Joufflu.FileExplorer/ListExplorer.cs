using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer;

/// <summary>
/// Display the children of a folder as a list. Opening a folder (double click, Enter or
/// <see cref="ExplorerCommands.Open"/>) navigates into it, opening anything else raises
/// <see cref="NodeOpened"/>.
/// </summary>
/// <remarks>
/// Nodes are displayed with the data template matching their type, the default ones are provided for
/// <see cref="ExplorerFile"/> and <see cref="ExplorerFolder"/> (see
/// <see cref="ExplorerResources.FileTemplate"/> and <see cref="ExplorerResources.FolderTemplate"/>).
/// A single template for every node can also be set through <see cref="ItemsControl.ItemTemplate"/>.
/// </remarks>
public class ListExplorer : ListBox, IExplorerControl
{
    static ListExplorer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ListExplorer),
            new FrameworkPropertyMetadata(typeof(ListExplorer)));
    }

    public ListExplorer()
    {
        Explorer.Initialize(this);
    }

    /// <summary>Raised when a node is opened, before navigating if it is a folder.</summary>
    public event EventHandler<ExplorerNodeEventArgs>? NodeOpened;

    #region Properties
    /// <summary>Root element of the explorer, displayed until another folder is navigated to.</summary>
    public IExplorerFolder? Root
    {
        get => (IExplorerFolder?)GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    public static readonly DependencyProperty RootProperty = DependencyProperty.Register(
        nameof(Root),
        typeof(IExplorerFolder),
        typeof(ListExplorer),
        new PropertyMetadata(null, OnRootChanged));

    /// <summary>Folder whose children are displayed, the <see cref="Root"/> by default.</summary>
    public IExplorerFolder? CurrentFolder
    {
        get => (IExplorerFolder?)GetValue(CurrentFolderProperty);
        set => SetValue(CurrentFolderProperty, value);
    }

    public static readonly DependencyProperty CurrentFolderProperty = DependencyProperty.Register(
        nameof(CurrentFolder),
        typeof(IExplorerFolder),
        typeof(ListExplorer),
        new PropertyMetadata(null, OnCurrentFolderChanged));

    /// <summary>Context menu used for every node, before falling back on the default resources.</summary>
    public ContextMenu? ItemContextMenu
    {
        get => (ContextMenu?)GetValue(ItemContextMenuProperty);
        set => SetValue(ItemContextMenuProperty, value);
    }

    public static readonly DependencyProperty ItemContextMenuProperty = DependencyProperty.Register(
        nameof(ItemContextMenu),
        typeof(ContextMenu),
        typeof(ListExplorer),
        new PropertyMetadata(null));

    /// <summary>Context menu used when more than one node is selected.</summary>
    public ContextMenu? SelectionContextMenu
    {
        get => (ContextMenu?)GetValue(SelectionContextMenuProperty);
        set => SetValue(SelectionContextMenuProperty, value);
    }

    public static readonly DependencyProperty SelectionContextMenuProperty = DependencyProperty.Register(
        nameof(SelectionContextMenu),
        typeof(ContextMenu),
        typeof(ListExplorer),
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
        typeof(ListExplorer),
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
        typeof(ListExplorer),
        new PropertyMetadata(FileSystemCommandHandler.Default));

    /// <summary>Selected nodes, in selection order.</summary>
    public IReadOnlyList<IExplorerNode> SelectedNodes => SelectedItems.OfType<IExplorerNode>().ToList();
    #endregion

    private static void OnRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Changing the root resets the navigation
        ((ListExplorer)d).CurrentFolder = (IExplorerFolder?)e.NewValue;
    }

    private static void OnCurrentFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ListExplorer explorer = (ListExplorer)d;
        explorer.ItemsSource = (e.NewValue as IExplorerFolder)?.Children;
    }

    public void OpenNode(IExplorerNode node)
    {
        NodeOpened?.Invoke(this, new ExplorerNodeEventArgs(node));

        if (node is IExplorerFolder folder)
            CurrentFolder = folder;
    }
}
