using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;
using Joufflu.Helpers;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// Dragging nodes out of the control, and taking a drop in : from another explorer, from the Windows file explorer,
/// or from the control itself.
/// </summary>
public abstract partial class ExplorerNodesControl
{
    /// <summary>
    /// Format announcing that the drag comes from an explorer. It carries a token and nothing else, the nodes
    /// themselves living in <see cref="_draggedPayload"/> : since .NET 9 the clipboard and drag and drop refuse to
    /// serialize arbitrary objects, and a drag is single at any moment so a static field is enough.
    /// </summary>
    private const string NodesFormat = "Joufflu.FileExplorer.Nodes";

    private const double AutoScrollMargin = 24;

    private static DragPayload? _draggedPayload;

    /// <summary>Where the left button went down, and what was selected then. Null when no drag may start.</summary>
    private Point? _dragOrigin;
    private IReadOnlyList<IExplorerNode> _dragNodes = [];

    private ExplorerDropAdorner? _dropAdorner;
    private UIElement? _dropAdornedElement;

    #region Dependency Property
    public static readonly DependencyProperty AllowDragProperty = DependencyProperty.Register(
        nameof(AllowDrag),
        typeof(bool),
        typeof(ExplorerNodesControl),
        new PropertyMetadata(true));

    private static readonly DependencyPropertyKey IsDropTargetPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsDropTarget),
        typeof(bool),
        typeof(ExplorerNodesControl),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsDropTargetProperty = IsDropTargetPropertyKey.DependencyProperty;
    #endregion

    /// <summary>Whether nodes can be dragged out of the control, true by default.</summary>
    public bool AllowDrag
    {
        get => (bool)GetValue(AllowDragProperty);
        set => SetValue(AllowDragProperty, value);
    }

    /// <summary>
    /// True while a drag the control would accept is hovering it, for a template trigger. Taking drops at all is
    /// <see cref="UIElement.AllowDrop"/>, whose default this control overrides to true.
    /// </summary>
    public bool IsDropTarget => (bool)GetValue(IsDropTargetProperty);

    private void AttachDragDrop(ItemsControl host)
    {
        host.PreviewMouseLeftButtonDown += OnDragSourceMouseLeftButtonDown;
        host.PreviewMouseMove += OnDragSourceMouseMove;
        host.PreviewMouseLeftButtonUp += OnDragSourceMouseLeftButtonUp;
        host.QueryContinueDrag += OnDragSourceQueryContinueDrag;

        host.AllowDrop = true;
        host.DragEnter += OnHostDragOver;
        host.DragOver += OnHostDragOver;
        host.DragLeave += OnHostDragLeave;
        host.Drop += OnHostDrop;
    }

    private void DetachDragDrop(ItemsControl host)
    {
        host.PreviewMouseLeftButtonDown -= OnDragSourceMouseLeftButtonDown;
        host.PreviewMouseMove -= OnDragSourceMouseMove;
        host.PreviewMouseLeftButtonUp -= OnDragSourceMouseLeftButtonUp;
        host.QueryContinueDrag -= OnDragSourceQueryContinueDrag;

        host.DragEnter -= OnHostDragOver;
        host.DragOver -= OnHostDragOver;
        host.DragLeave -= OnHostDragLeave;
        host.Drop -= OnHostDrop;

        ClearDropTarget();
    }

    #region Dragging out
    /// <summary>
    /// Remembers where the drag would start from, and what it would carry.
    /// </summary>
    /// <remarks>
    /// The selection is captured here, on the preview, because an item container collapses a multiple selection to the
    /// clicked row as soon as the button goes down : reading it later would only ever drag one node. The visible
    /// selection still collapses, which the file explorer avoids by deferring it to the button going up.
    /// </remarks>
    private void OnDragSourceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ClearDragOrigin();

        if (!AllowDrag || ItemsHost == null || EditingNode != null)
            return;

        if (GetContainerAt(e.OriginalSource as DependencyObject)?.DataContext is not IExplorerNode node)
            return;

        var selection = GetSelectedNodes().ToList();
        _dragNodes = selection.Contains(node) ? selection : [node];
        _dragOrigin = e.GetPosition(ItemsHost);
    }

    private void OnDragSourceMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragOrigin is not { } origin || ItemsHost == null || e.LeftButton != MouseButtonState.Pressed)
            return;

        var position = e.GetPosition(ItemsHost);
        if (Math.Abs(position.X - origin.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(position.Y - origin.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        var nodes = _dragNodes;
        ClearDragOrigin();
        StartDrag(nodes);
    }

    private void OnDragSourceMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ClearDragOrigin();

    private void OnDragSourceQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
        if (e.EscapePressed)
            e.Action = DragAction.Cancel;
    }

    private void ClearDragOrigin()
    {
        _dragOrigin = null;
        _dragNodes = [];
    }

    private void StartDrag(IReadOnlyList<IExplorerNode> nodes)
    {
        if (nodes.Count == 0 || ItemsHost == null || Session == null)
            return;

        var data = new DataObject();
        data.SetData(NodesFormat, NodesFormat);

        // FileDrop only when every node has a path on this machine : that is what makes a drop into the Windows file
        // explorer work, and what makes a node existing nowhere show the "no drop" cursor instead of doing nothing.
        string[] paths = nodes.OfType<IPhysicalExplorerNode>().Select(node => node.FileSystemPath).ToArray();
        if (paths.Length == nodes.Count)
        {
            var files = new StringCollection();
            files.AddRange(paths);
            data.SetFileDropList(files);
        }

        _draggedPayload = new DragPayload(Session, nodes);
        try
        {
            var result = DragDrop.DoDragDrop(ItemsHost, data, DragDropEffects.Copy | DragDropEffects.Move);

            // Whoever took the drop has already moved the files ; only what we show is out of date.
            if (result == DragDropEffects.Move)
                _ = Session.RefreshAsync(null);
        }
        catch (COMException)
        {
            // Another application holding the drag and drop, or refusing it.
        }
        catch (InvalidOperationException)
        {
            // The drag was started while the tree was rebuilding under it.
        }
        finally
        {
            _draggedPayload = null;
            ClearDropTarget();
        }
    }
    #endregion

    #region Taking a drop
    private void OnHostDragOver(object sender, DragEventArgs e)
    {
        // Always handled : an unhandled DragOver lets an ancestor decide, which would show a cursor that does not
        // match what this control would really do.
        e.Handled = true;

        var (transfer, target, effects) = ResolveDrop(e);
        e.Effects = effects;

        if (effects == DragDropEffects.None || transfer == null || target == null)
        {
            ClearDropTarget();
            return;
        }

        ShowDropTarget(target);
        AutoScroll(e);
    }

    private void OnHostDragLeave(object sender, DragEventArgs e)
    {
        ClearDropTarget();
        e.Handled = true;
    }

    private void OnHostDrop(object sender, DragEventArgs e)
    {
        e.Handled = true;

        var (transfer, target, effects) = ResolveDrop(e);

        // Set before anything else : DoDragDrop reads it as soon as this handler returns.
        e.Effects = effects;
        ClearDropTarget();

        if (effects == DragDropEffects.None || transfer == null || target == null || Session == null)
            return;

        var session = Session;

        // Never awaited here : a drag runs a nested message loop, and resuming a continuation inside it while the
        // loop is unwinding is a re-entrancy trap. Background priority lets the drag finish first.
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            () => _ = session.AcceptAsync(transfer, target));
    }

    /// <summary>
    /// What the drop would do : where it would land, what it would carry, and whether it would copy or move. Entirely
    /// synchronous and free of I/O, being called for every mouse move of the drag.
    /// </summary>
    private (ExplorerTransfer? Transfer, IExplorerDirectory? Target, DragDropEffects Effects) ResolveDrop(
        DragEventArgs e)
    {
        var none = ((ExplorerTransfer?)null, (IExplorerDirectory?)null, DragDropEffects.None);

        if (!AllowDrop || Session == null)
            return none;

        var target = GetDropTarget(e);
        if (target == null)
            return none;

        // The origin decides the default : staying inside one explorer moves, crossing to another one copies, which is
        // what the file explorer does between two drives.
        var origin = e.Data.GetDataPresent(NodesFormat) ? _draggedPayload?.Session : null;

        var wanted = (e.KeyStates & DragDropKeyStates.ControlKey) != 0
            ? DragDropEffects.Copy
            : (e.KeyStates & DragDropKeyStates.ShiftKey) != 0
                ? DragDropEffects.Move
                : ReferenceEquals(origin, Session) ? DragDropEffects.Move : DragDropEffects.Copy;

        if ((wanted & e.AllowedEffects) == 0)
        {
            // What was asked isn't allowed : fall back on copy rather than refusing outright.
            wanted = (e.AllowedEffects & DragDropEffects.Copy) != 0 ? DragDropEffects.Copy : DragDropEffects.None;
            if (wanted == DragDropEffects.None)
                return none;
        }

        var transfer = BuildTransfer(e, wanted == DragDropEffects.Move);
        if (transfer == null || !Session.CanAccept(transfer, target))
            return none;

        return (transfer, target, wanted);
    }

    /// <summary>
    /// Content being dropped. The private format is read first, so that a drag coming from an explorer keeps its
    /// nodes and its origin even though it also carries their paths.
    /// </summary>
    private static ExplorerTransfer? BuildTransfer(DragEventArgs e, bool isMove)
    {
        if (e.Data.GetDataPresent(NodesFormat) && _draggedPayload is { } payload)
            return ExplorerTransfer.FromNodes(payload.Nodes, isMove, payload.Session);

        if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            return ExplorerTransfer.FromPaths(paths, isMove);

        return null;
    }

    /// <summary>
    /// Directory a drop would land in : the hovered node when it is one, its parent when it is a file, and the opened
    /// directory when the cursor is over the empty space.
    /// </summary>
    protected IExplorerDirectory? GetDropTarget(DragEventArgs e)
    {
        if (ItemsHost == null)
            return null;

        return GetContainerAt(e.OriginalSource as DependencyObject)?.DataContext switch
        {
            IExplorerDirectory directory => directory,
            IExplorerNode node => node.Parent ?? Session?.Current,
            _ => Session?.Current
        };
    }

    private void ShowDropTarget(IExplorerDirectory target)
    {
        SetValue(IsDropTargetPropertyKey, true);

        // The container of the target when it is displayed here, the whole control when the target is the opened
        // directory and so has no row of its own.
        UIElement? element = FindContainer(target) ?? ItemsHost;
        if (element == null || ReferenceEquals(element, _dropAdornedElement))
            return;

        RemoveDropAdorner();
        _dropAdornedElement = element;
        _dropAdorner = ExplorerDropAdorner.Attach(element);
    }

    private void ClearDropTarget()
    {
        SetValue(IsDropTargetPropertyKey, false);
        RemoveDropAdorner();
    }

    private void RemoveDropAdorner()
    {
        _dropAdorner?.Detach();
        _dropAdorner = null;
        _dropAdornedElement = null;
    }

    /// <summary>
    /// Scrolls while the cursor lingers near an edge, so a drag can reach what is out of view.
    /// </summary>
    private void AutoScroll(DragEventArgs e)
    {
        if (ItemsHost == null)
            return;

        var scroll = MoreVisualTreeHelper.GetChild<ScrollViewer>(ItemsHost, true);
        if (scroll == null)
            return;

        double y = e.GetPosition(scroll).Y;
        if (y < AutoScrollMargin)
            scroll.LineUp();
        else if (y > scroll.ActualHeight - AutoScrollMargin)
            scroll.LineDown();
    }
    #endregion

    /// <summary>
    /// Nodes of the drag in flight. Kept aside rather than put in the data object, which can only carry values that
    /// survive being serialized.
    /// </summary>
    private sealed record DragPayload(ExplorerSession Session, IReadOnlyList<IExplorerNode> Nodes);
}
