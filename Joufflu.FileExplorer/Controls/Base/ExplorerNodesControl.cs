using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;

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
/// Base of the explorer controls : the session whose content they display and through which they navigate. Several
/// controls sharing the same session all show the same opened directory.
/// </summary>
public abstract class ExplorerControl : Control
{
    #region Dependency Property
    public static readonly DependencyProperty SessionProperty = DependencyProperty.Register(
        nameof(Session),
        typeof(ExplorerSession),
        typeof(ExplorerControl),
        new PropertyMetadata(null));
    #endregion

    public ExplorerSession? Session
    {
        get => (ExplorerSession?)GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }
}

/// <summary>
/// Behaviour shared by the controls displaying explorer nodes (<see cref="ExplorerList"/>,
/// <see cref="ExplorerTree"/>) : the selection, activating a node, the keyboard shortcuts, the context menu, renaming
/// and drag and drop. A derived control only provides the <see cref="ItemsControl"/> template part displaying the
/// nodes and reads its selection.
/// </summary>
/// <remarks>
/// Split across several files by concern : the context menu, the name editor and drag and drop each have their own.
/// They are parts of this class rather than collaborators because each of them needs the template parts, the session
/// and the hit testing of the control, which would all have to be handed over.
/// </remarks>
[TemplatePart(Name = PartItemsHost, Type = typeof(ItemsControl))]
[TemplatePart(Name = PartRenameEditor, Type = typeof(System.Windows.Controls.Primitives.Popup))]
[TemplatePart(Name = PartRenameEditorBox, Type = typeof(TextBox))]
public abstract partial class ExplorerNodesControl : ExplorerControl
{
    protected const string PartItemsHost = "PART_ItemsHost";

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

    #region Routed Event
    /// <summary>
    /// Raised when the user activates a node : a double click, or Enter on the selection. Handle it to take over, a
    /// directory being opened and a file handed to its application otherwise.
    /// </summary>
    public static readonly RoutedEvent NodeActivatedEvent = EventManager.RegisterRoutedEvent(
        nameof(NodeActivated),
        RoutingStrategy.Bubble,
        typeof(EventHandler<ExplorerNodeEventArgs>),
        typeof(ExplorerNodesControl));

    public event EventHandler<ExplorerNodeEventArgs> NodeActivated
    {
        add => AddHandler(NodeActivatedEvent, value);
        remove => RemoveHandler(NodeActivatedEvent, value);
    }
    #endregion

    static ExplorerNodesControl()
    {
        // The controls take drops by default ; a consumer opts out with AllowDrop="False".
        AllowDropProperty.OverrideMetadata(typeof(ExplorerNodesControl), new FrameworkPropertyMetadata(true));
    }

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
            DetachContextMenu(ItemsHost);
            DetachDragDrop(ItemsHost);
        }

        ItemsHost = GetTemplateChild(PartItemsHost) as ItemsControl;
        ApplyEditorTemplateParts();

        if (ItemsHost != null)
        {
            // An item container handles the click to select itself, hence handledEventsToo.
            ItemsHost.AddHandler(
                MouseLeftButtonDownEvent,
                (MouseButtonEventHandler)OnItemsHostMouseLeftButtonDown,
                true);
            AttachContextMenu(ItemsHost);
            AttachDragDrop(ItemsHost);
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // While a name is being edited the box owns the keyboard.
        if (e.Handled || Session == null || EditingNode != null)
            return;

        var nodes = SelectedNodes;
        bool isControl = Keyboard.Modifiers == ModifierKeys.Control;

        switch (e.Key)
        {
            case Key.F2 when nodes.Count == 1:
                BeginRename(nodes[0]);
                e.Handled = true;
                break;
            case Key.Delete when Keyboard.Modifiers == ModifierKeys.Shift:
                e.Handled = TryExecute(Session.DeletePermanentlyCommand, nodes);
                break;
            case Key.Delete:
                e.Handled = TryExecute(Session.DeleteCommand, nodes);
                break;
            case Key.C when isControl:
                e.Handled = TryExecute(Session.CopyCommand, nodes);
                break;
            case Key.X when isControl:
                e.Handled = TryExecute(Session.CutCommand, nodes);
                break;
            case Key.V when isControl:
                Session.RefreshClipboardState();
                e.Handled = TryExecute(Session.PasteCommand, Session.Current);
                break;
            case Key.Enter when nodes.Count == 1:
                OnNodeActivated(nodes[0]);
                e.Handled = true;
                break;
            case Key.Back:
                e.Handled = TryExecute(Session.OpenParentCommand, null);
                break;
            case Key.F5:
                e.Handled = TryExecute(Session.RefreshCommand, null);
                break;
        }
    }

    /// <summary>
    /// Runs a command when it can, and reports whether it did so the key can be marked handled. CanExecute is asked
    /// first because a command refusing its parameter throws rather than answering false.
    /// </summary>
    private static bool TryExecute(ICommand command, object? parameter)
    {
        if (!command.CanExecute(parameter))
            return false;

        command.Execute(parameter);
        return true;
    }
    #endregion

    /// <summary>
    /// Reacts to a double click on a node, activating it by default. Returns whether the double click has been
    /// handled.
    /// </summary>
    /// <param name="container">Item container displaying <paramref name="node"/>.</param>
    protected virtual bool OnNodeDoubleClick(IExplorerNode node, FrameworkElement container)
    {
        OnNodeActivated(node);
        return true;
    }

    /// <summary>
    /// Raises <see cref="NodeActivated"/> and, unless a handler took over, opens a directory or hands a file to the
    /// application it is associated with.
    /// </summary>
    protected virtual void OnNodeActivated(IExplorerNode node)
    {
        var args = new ExplorerNodeEventArgs(NodeActivatedEvent, node) { Source = this };
        RaiseEvent(args);

        if (args.Handled || Session == null)
            return;

        if (node is IExplorerDirectory directory)
            // Not awaited : the session reports its own failures through ExplorerSession.LastError.
            _ = Session.OpenAsync(directory);
        else
            Session.OpenWithDefaultApplication(node);
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
