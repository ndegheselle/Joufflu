using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer;

/// <summary>
/// Shared surface of the explorer controls (<see cref="ListExplorer"/> and <see cref="TreeExplorer"/>).
/// Used by <see cref="Explorer"/> to handle the parts that don't depend on the presentation
/// (context menus resolution, commands, opening of a node).
/// </summary>
public interface IExplorerControl
{
    /// <summary>Root element displayed by the control.</summary>
    IExplorerFolder? Root { get; }

    /// <summary>
    /// Folder the actions apply to when they don't target a node (paste, new folder, right click on
    /// the empty space of the control).
    /// </summary>
    IExplorerFolder? CurrentFolder { get; }

    /// <summary>Selected nodes (a single one for the controls that don't support multi selection).</summary>
    IReadOnlyList<IExplorerNode> SelectedNodes { get; }

    /// <summary>Context menu used for every node, before falling back on the default resources.</summary>
    ContextMenu? ItemContextMenu { get; }

    /// <summary>Context menu used when more than one node is selected.</summary>
    ContextMenu? SelectionContextMenu { get; }

    /// <summary>Resolve a context menu per node, takes precedence over every other menu.</summary>
    ExplorerContextMenuSelector? ItemContextMenuSelector { get; }

    /// <summary>Handles the <see cref="ExplorerCommands"/> (defaults to <see cref="FileSystemCommandHandler"/>).</summary>
    IExplorerCommandHandler? CommandHandler { get; }

    /// <summary>
    /// Open a node : navigate to it or expand it if it is a folder, notify the app otherwise.
    /// </summary>
    void OpenNode(IExplorerNode node);
}

/// <summary>
/// Node that brings its own context menu.
/// </summary>
public interface IExplorerContextMenuNode : IExplorerNode
{
    ContextMenu? ContextMenu { get; }
}

/// <summary>
/// Resolve the context menu of the right clicked nodes, the same way a
/// <see cref="System.Windows.Controls.DataTemplateSelector"/> resolves a template.
/// </summary>
public abstract class ExplorerContextMenuSelector
{
    /// <param name="nodes">Nodes the menu will apply to (the selection, or the right clicked node).</param>
    /// <param name="explorer">Control the nodes are displayed in.</param>
    /// <returns>The menu to show, or <c>null</c> to let the explorer resolve it.</returns>
    public abstract ContextMenu? SelectContextMenu(IReadOnlyList<IExplorerNode> nodes, IExplorerControl explorer);
}

public class ExplorerNodeEventArgs : EventArgs
{
    public IExplorerNode Node { get; }

    public ExplorerNodeEventArgs(IExplorerNode node) { Node = node; }
}

/// <summary>
/// Everything a <see cref="IExplorerCommandHandler"/> needs to run a command.
/// </summary>
public class ExplorerCommandContext
{
    /// <summary>Command being executed (one of the <see cref="ExplorerCommands"/>).</summary>
    public RoutedUICommand Command { get; }

    /// <summary>Control the command comes from.</summary>
    public IExplorerControl Explorer { get; }

    /// <summary>
    /// Folder the command applies to : the selected folder if there is only one selected, the
    /// <see cref="IExplorerControl.CurrentFolder"/> otherwise.
    /// </summary>
    public IExplorerFolder? Folder { get; }

    /// <summary>Nodes the command applies to (may be empty).</summary>
    public IReadOnlyList<IExplorerNode> Nodes { get; }

    /// <summary>Parameter of the command.</summary>
    public object? Parameter { get; }

    public ExplorerCommandContext(
        RoutedUICommand command,
        IExplorerControl explorer,
        IExplorerFolder? folder,
        IReadOnlyList<IExplorerNode> nodes,
        object? parameter)
    {
        Command = command;
        Explorer = explorer;
        Folder = folder;
        Nodes = nodes;
        Parameter = parameter;
    }
}

/// <summary>
/// Handle the <see cref="ExplorerCommands"/> of an explorer control.
/// </summary>
public interface IExplorerCommandHandler
{
    bool CanExecute(ExplorerCommandContext context);

    void Execute(ExplorerCommandContext context);
}

/// <summary>
/// Commands shared by the explorer controls, they can be used in any custom context menu.
/// <see cref="ExplorerCommands.Open"/> is handled by the controls themselves, every other command is
/// forwarded to the <see cref="IExplorerControl.CommandHandler"/>.
/// </summary>
public static class ExplorerCommands
{
    public static RoutedUICommand Open { get; } = Create("Open", nameof(Open), new KeyGesture(Key.Enter));

    public static RoutedUICommand Cut { get; } = Create("Cut", nameof(Cut), new KeyGesture(Key.X, ModifierKeys.Control));

    public static RoutedUICommand Copy { get; } = Create("Copy", nameof(Copy), new KeyGesture(Key.C, ModifierKeys.Control));

    public static RoutedUICommand Paste { get; } = Create("Paste", nameof(Paste), new KeyGesture(Key.V, ModifierKeys.Control));

    public static RoutedUICommand Delete { get; } = Create("Delete", nameof(Delete), new KeyGesture(Key.Delete));

    /// <summary>Has no default implementation, handle it to show your own renaming UI.</summary>
    public static RoutedUICommand Rename { get; } = Create("Rename", nameof(Rename), new KeyGesture(Key.F2));

    public static RoutedUICommand NewFolder { get; } = Create("New folder", nameof(NewFolder), new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift));

    /// <summary>Every command of the explorer controls.</summary>
    public static IReadOnlyList<RoutedUICommand> All { get; }
        = new RoutedUICommand[] { Open, Cut, Copy, Paste, Delete, Rename, NewFolder };

    private static RoutedUICommand Create(string text, string name, InputGesture gesture)
    {
        return new RoutedUICommand(text, name, typeof(ExplorerCommands), new InputGestureCollection { gesture });
    }
}

/// <summary>
/// Resource keys of the default resources of the explorer controls, every one of them can be
/// overriden by redefining the key in the resources of the application.
/// </summary>
public static class ExplorerResources
{
    /// <summary>Converter from a path (or a <see cref="IExplorerPathNode"/>) to its windows icon.</summary>
    public static ComponentResourceKey SystemIconConverter => new(typeof(ExplorerResources), nameof(SystemIconConverter));

    /// <summary>Content of a file node (icon + name).</summary>
    public static ComponentResourceKey FileTemplate => new(typeof(ExplorerResources), nameof(FileTemplate));

    /// <summary>Content of a folder node (icon + name).</summary>
    public static ComponentResourceKey FolderTemplate => new(typeof(ExplorerResources), nameof(FolderTemplate));

    /// <summary>Default context menu of a file node.</summary>
    public static ComponentResourceKey FileContextMenu => new(typeof(ExplorerResources), nameof(FileContextMenu));

    /// <summary>Default context menu of a folder node.</summary>
    public static ComponentResourceKey FolderContextMenu => new(typeof(ExplorerResources), nameof(FolderContextMenu));

    /// <summary>Default context menu when more than one node is selected.</summary>
    public static ComponentResourceKey SelectionContextMenu => new(typeof(ExplorerResources), nameof(SelectionContextMenu));
}

/// <summary>
/// Behaviors shared by the explorer controls : commands, selection and context menus handling.
/// Also exposes the informations of the opened context menu (<see cref="OwnerProperty"/> and
/// <see cref="NodesProperty"/>) so that custom menus can bind to them.
/// </summary>
public static class Explorer
{
    #region Context menu informations
    /// <summary>Explorer control the opened context menu comes from.</summary>
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.RegisterAttached(
        "Owner",
        typeof(IExplorerControl),
        typeof(Explorer),
        new PropertyMetadata(null));

    public static IExplorerControl? GetOwner(DependencyObject obj) => (IExplorerControl?)obj.GetValue(OwnerProperty);

    public static void SetOwner(DependencyObject obj, IExplorerControl? value) => obj.SetValue(OwnerProperty, value);

    /// <summary>Nodes the opened context menu applies to.</summary>
    public static readonly DependencyProperty NodesProperty = DependencyProperty.RegisterAttached(
        "Nodes",
        typeof(IReadOnlyList<IExplorerNode>),
        typeof(Explorer),
        new PropertyMetadata(null));

    public static IReadOnlyList<IExplorerNode>? GetNodes(DependencyObject obj)
        => (IReadOnlyList<IExplorerNode>?)obj.GetValue(NodesProperty);

    public static void SetNodes(DependencyObject obj, IReadOnlyList<IExplorerNode>? value)
        => obj.SetValue(NodesProperty, value);

    /// <summary>
    /// Marks the menu items whose command target is handled by the explorer, so that a target set by
    /// the user is never overriden.
    /// </summary>
    private static readonly DependencyProperty IsCommandTargetHandledProperty = DependencyProperty.RegisterAttached(
        "IsCommandTargetHandled",
        typeof(bool),
        typeof(Explorer),
        new PropertyMetadata(false));
    #endregion

    /// <summary>
    /// Plug the shared behaviors on a control, to be called in the constructor of the explorer controls.
    /// </summary>
    internal static void Initialize(Control control)
    {
        foreach (RoutedUICommand command in ExplorerCommands.All)
        {
            RoutedUICommand current = command;
            control.CommandBindings.Add(
                new CommandBinding(
                    current,
                    (_, e) => OnExecuted(control, current, e),
                    (_, e) => OnCanExecute(control, current, e)));

            foreach (InputGesture gesture in current.InputGestures)
                control.InputBindings.Add(new InputBinding(current, gesture));
        }

        control.PreviewMouseRightButtonDown += (_, e) => OnPreviewMouseRightButtonDown(control, e);
        control.MouseDoubleClick += (_, e) => OnMouseDoubleClick(control, e);
        control.ContextMenuOpening += (_, e) => OnContextMenuOpening(control, e);
    }

    #region Commands
    private static void OnExecuted(Control control, RoutedUICommand command, ExecutedRoutedEventArgs e)
    {
        IExplorerControl explorer = (IExplorerControl)control;

        // Opening is a presentation concern (navigate, expand, ...), the controls handle it themselves
        if (command == ExplorerCommands.Open)
        {
            IExplorerNode? node = GetTargetNode(explorer, e.Parameter);
            if (node == null)
                return;
            explorer.OpenNode(node);
            e.Handled = true;
            return;
        }

        ExplorerCommandContext context = CreateContext(explorer, command, e.Parameter);
        IExplorerCommandHandler? handler = explorer.CommandHandler;
        if (handler == null || !handler.CanExecute(context))
            return;

        handler.Execute(context);
        e.Handled = true;
    }

    private static void OnCanExecute(Control control, RoutedUICommand command, CanExecuteRoutedEventArgs e)
    {
        IExplorerControl explorer = (IExplorerControl)control;

        if (command == ExplorerCommands.Open)
        {
            e.CanExecute = GetTargetNode(explorer, e.Parameter) != null;
            return;
        }

        e.CanExecute = explorer.CommandHandler?.CanExecute(CreateContext(explorer, command, e.Parameter)) == true;
    }

    private static ExplorerCommandContext CreateContext(
        IExplorerControl explorer,
        RoutedUICommand command,
        object? parameter)
    {
        IReadOnlyList<IExplorerNode> nodes = explorer.SelectedNodes;
        if (parameter is IExplorerNode target)
            nodes = new[] { target };

        // Pasting or creating applies to the selected folder, or to the displayed one
        IExplorerFolder? folder = nodes.Count == 1 && nodes[0] is IExplorerFolder selected
            ? selected
            : explorer.CurrentFolder;

        return new ExplorerCommandContext(command, explorer, folder, nodes, parameter);
    }

    private static IExplorerNode? GetTargetNode(IExplorerControl explorer, object? parameter)
    {
        if (parameter is IExplorerNode node)
            return node;
        IReadOnlyList<IExplorerNode> selection = explorer.SelectedNodes;
        return selection.Count == 1 ? selection[0] : null;
    }
    #endregion

    #region Mouse
    private static void OnPreviewMouseRightButtonDown(Control control, MouseButtonEventArgs e)
    {
        FrameworkElement? container = FindItemContainer(e.OriginalSource as DependencyObject, control);

        switch (container)
        {
            // Right clicking outside of the selection makes the clicked item the new selection
            case ListBoxItem listItem when !listItem.IsSelected:
                if (control is ListBox { SelectionMode: not SelectionMode.Single } listBox)
                    listBox.SelectedItems.Clear();
                listItem.IsSelected = true;
                break;
            case TreeViewItem treeItem:
                treeItem.IsSelected = true;
                break;
        }
    }

    private static void OnMouseDoubleClick(Control control, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        FrameworkElement? container = FindItemContainer(e.OriginalSource as DependencyObject, control);
        if (container?.DataContext is not IExplorerNode node)
            return;

        ((IExplorerControl)control).OpenNode(node);
        e.Handled = true;
    }
    #endregion

    #region Context menu
    private static void OnContextMenuOpening(Control control, ContextMenuEventArgs e)
    {
        // Let a handler of the app (or of an item) take over
        if (e.Handled)
            return;

        IExplorerControl explorer = (IExplorerControl)control;
        FrameworkElement? container = FindItemContainer(e.OriginalSource as DependencyObject, control);
        IExplorerNode? node = container?.DataContext as IExplorerNode;
        IReadOnlyList<IExplorerNode> nodes;
        ContextMenu? menu;

        if (node == null)
        {
            // Right click outside of any item : the menu applies to the displayed folder
            node = explorer.CurrentFolder;
            nodes = node == null ? Array.Empty<IExplorerNode>() : new[] { node };
            menu = control.ContextMenu ?? ResolveContextMenu(explorer, nodes);
        }
        else
        {
            IReadOnlyList<IExplorerNode> selection = explorer.SelectedNodes;
            nodes = selection.Contains(node) ? selection : new[] { node };
            menu = ResolveContextMenu(explorer, nodes);
        }

        // WPF resolves the menu to open from the ContextMenu properties of the tree *after* this event,
        // so the explorer opens the resolved menu by itself.
        e.Handled = true;
        if (menu == null || node == null)
            return;

        menu.DataContext = node;
        SetOwner(menu, explorer);
        SetNodes(menu, nodes);
        // Commands are routed from the menu (its own visual tree) to the explorer control
        SetCommandTargets(menu, control);

        // Same placements as the ones used by WPF : on the mouse, or centered when opened with the keyboard
        menu.Placement = e.CursorLeft == -1 && e.CursorTop == -1
            ? PlacementMode.Center
            : PlacementMode.MousePoint;
        menu.PlacementTarget = container ?? control;
        menu.IsOpen = true;
    }

    /// <summary>
    /// Resolve the context menu of the given nodes :
    /// <see cref="IExplorerControl.ItemContextMenuSelector"/>, then
    /// <see cref="IExplorerControl.SelectionContextMenu"/> for a multi selection, then
    /// <see cref="IExplorerContextMenuNode.ContextMenu"/>, then
    /// <see cref="IExplorerControl.ItemContextMenu"/> and finally the default menus resources.
    /// </summary>
    public static ContextMenu? ResolveContextMenu(IExplorerControl explorer, IReadOnlyList<IExplorerNode> nodes)
    {
        ContextMenu? selected = explorer.ItemContextMenuSelector?.SelectContextMenu(nodes, explorer);
        if (selected != null)
            return selected;

        if (nodes.Count == 0)
            return null;

        if (nodes.Count > 1)
            return explorer.SelectionContextMenu ?? FindMenuResource(explorer, ExplorerResources.SelectionContextMenu);

        if (nodes[0] is IExplorerContextMenuNode { ContextMenu: not null } custom)
            return custom.ContextMenu;

        if (explorer.ItemContextMenu != null)
            return explorer.ItemContextMenu;

        return FindMenuResource(
            explorer,
            nodes[0] is IExplorerFolder ? ExplorerResources.FolderContextMenu : ExplorerResources.FileContextMenu);
    }

    private static ContextMenu? FindMenuResource(IExplorerControl explorer, ComponentResourceKey key)
        => (explorer as FrameworkElement)?.TryFindResource(key) as ContextMenu;

    /// <summary>
    /// Menu items live in the visual tree of the menu, they need a target inside the explorer for their
    /// commands to reach its command bindings.
    /// </summary>
    private static void SetCommandTargets(ItemsControl menu, IInputElement target)
    {
        foreach (object? item in menu.Items)
        {
            if (item is not MenuItem menuItem)
                continue;

            bool isSetByUser = menuItem.ReadLocalValue(MenuItem.CommandTargetProperty) != DependencyProperty.UnsetValue
                && !(bool)menuItem.GetValue(IsCommandTargetHandledProperty);
            if (!isSetByUser)
            {
                menuItem.CommandTarget = target;
                menuItem.SetValue(IsCommandTargetHandledProperty, true);
            }

            SetCommandTargets(menuItem, target);
        }
    }
    #endregion

    /// <summary>
    /// Walk up from a clicked element to the item container that holds it (stops at the explorer control).
    /// </summary>
    private static FrameworkElement? FindItemContainer(DependencyObject? source, DependencyObject root)
    {
        while (source != null && source != root)
        {
            if (source is ListBoxItem or TreeViewItem)
                return (FrameworkElement)source;

            source = source switch
            {
                Visual or System.Windows.Media.Media3D.Visual3D => VisualTreeHelper.GetParent(source),
                FrameworkContentElement content => content.Parent,
                _ => null
            };
        }
        return null;
    }
}
