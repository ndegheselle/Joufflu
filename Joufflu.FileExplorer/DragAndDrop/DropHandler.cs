using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.FileExplorer.DragAndDrop;

/// <summary>
/// Everything a drop needs to know, gathered once per event so implementations don't have to dig through
/// <see cref="DragEventArgs"/>.
/// </summary>
public record DropContext
{
    /// <summary>
    /// Object dragged from inside the application, null for a drop coming from the outside (see
    /// <see cref="FilePaths"/>).
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Paths of the files dropped from outside of the application, empty otherwise.
    /// </summary>
    public IReadOnlyList<string> FilePaths { get; init; } = [];

    /// <summary>
    /// Deepest element under the cursor.
    /// </summary>
    public FrameworkElement? TargetElement { get; init; }

    /// <summary>
    /// <c>DataContext</c> of <see cref="TargetElement"/>, the object dropped onto.
    /// </summary>
    public object? TargetData { get; init; }

    /// <summary>
    /// Cursor position, relative to the element the handler is attached to.
    /// </summary>
    public Point Position { get; init; }

    /// <summary>
    /// Effect chosen by <see cref="DropHandler.GetEffect"/>, <see cref="DragDropEffects.None"/> while it is not
    /// decided yet.
    /// </summary>
    public DragDropEffects Effect { get; init; }

    /// <summary>
    /// Modifier keys held during the drag, used to choose between copying and moving.
    /// </summary>
    public DragDropKeyStates KeyStates { get; init; }

    /// <summary>
    /// Gets <see cref="TargetData"/> if it is a <typeparamref name="TTarget"/>.
    /// </summary>
    public TTarget? GetTarget<TTarget>() where TTarget : class => TargetData as TTarget;
}

/// <summary>
/// Validates and applies a drop, the destination side of a drag and drop. Meant to be exposed by a view model and
/// bound with <see cref="Drop.HandlerProperty"/> : <c>dnd:Drop.Handler="{Binding DropHandler}"</c>.
/// </summary>
/// <remarks>
/// Override <see cref="CanDrop"/> to refuse a drop and <see cref="ApplyDrop"/> to apply its consequences. Set
/// <see cref="HoverContainerType"/> to <c>typeof(ListViewItem)</c> to highlight a row rather than the whole list, see
/// <see cref="Drop.IsHoveringProperty"/>.
/// </remarks>
public abstract partial class DropHandler : ObservableObject
{
    #region Properties

    /// <summary>
    /// Type of the container highlighted while the cursor passes over it, a row of a list for example. The element the
    /// handler is attached to is highlighted when it is null.
    /// </summary>
    public Type? HoverContainerType { get; set; }

    /// <summary>
    /// True while a drop this handler accepts hovers the element. Bindable, and mirrored on the element by
    /// <see cref="Drop.IsDropTargetProperty"/>.
    /// </summary>
    [ObservableProperty]
    private bool isDropTarget;

    private FrameworkElement? _element;
    private DependencyObject? _hoveredContainer;

    /// <summary>
    /// DragEnter and DragLeave are raised again for every child the cursor passes over, so a single leave doesn't mean
    /// the cursor left the element. Counting them does.
    /// <see href="https://stackoverflow.com/questions/5447301/wpf-drag-drop-when-does-dragleave-fire"/>
    /// </summary>
    private int _enterCount;

    #endregion

    #region Attachment

    /// <summary>
    /// Called by <see cref="Drop"/> when the handler is bound to an element.
    /// </summary>
    internal void Attach(FrameworkElement element) { _element = element; }

    /// <summary>
    /// Called by <see cref="Drop"/> when the handler is unbound from an element.
    /// </summary>
    internal void Detach(FrameworkElement element)
    {
        if (_element != element)
            return;

        ClearHover();
        _element = null;
    }

    #endregion

    #region UI events

    /// <summary>
    /// On <see cref="UIElement.DragEnter"/>.
    /// </summary>
    public void HandleDragEnter(object sender, DragEventArgs e)
    {
        _enterCount++;
        HandleDragOver(sender, e);
    }

    /// <summary>
    /// On <see cref="UIElement.DragOver"/>, decides the effect and highlights the container under the cursor.
    /// </summary>
    public void HandleDragOver(object sender, DragEventArgs e)
    {
        DropContext context = CreateContext(e);

        if (!CanDrop(context))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            ClearHover();
            return;
        }

        e.Effects = GetEffect(context, e.AllowedEffects);
        e.Handled = true;

        IsDropTarget = true;
        SetHover(GetHoverContainer(e.OriginalSource as DependencyObject));
        OnPassingOver(context with { Effect = e.Effects });
    }

    /// <summary>
    /// On <see cref="UIElement.DragLeave"/>, removes the highlight once the cursor really left the element.
    /// </summary>
    public void HandleDragLeave(object sender, DragEventArgs e)
    {
        _enterCount = Math.Max(0, _enterCount - 1);
        if (_enterCount == 0)
            ClearHover();
    }

    /// <summary>
    /// On <see cref="UIElement.Drop"/>, applies the drop when it is allowed.
    /// </summary>
    public void HandleDrop(object sender, DragEventArgs e)
    {
        DropContext context = CreateContext(e);
        bool canDrop = CanDrop(context);

        e.Effects = canDrop ? GetEffect(context, e.AllowedEffects) : DragDropEffects.None;
        e.Handled = true;

        _enterCount = 0;
        ClearHover();

        if (canDrop)
            ApplyDrop(context with { Effect = e.Effects });
    }

    #endregion

    #region Context

    private DropContext CreateContext(DragEventArgs e)
    {
        var target = e.OriginalSource as FrameworkElement;

        return new DropContext
        {
            Data = DragDropData.GetData<object>(e.Data),
            FilePaths = DragDropData.GetFilePaths(e.Data),
            TargetElement = target,
            TargetData = target?.DataContext,
            Position = _element != null ? e.GetPosition(_element) : default,
            KeyStates = e.KeyStates
        };
    }

    #endregion

    #region Hover

    private void SetHover(DependencyObject? container)
    {
        if (_hoveredContainer == container)
            return;

        ClearHover();
        _hoveredContainer = container;
        if (_hoveredContainer != null)
            Drop.SetIsHovering(_hoveredContainer, true);
    }

    private void ClearHover()
    {
        if (_hoveredContainer != null)
        {
            Drop.SetIsHovering(_hoveredContainer, false);
            _hoveredContainer = null;
        }

        IsDropTarget = false;
    }

    /// <summary>
    /// Walks up from the element under the cursor to the container to highlight, see
    /// <see cref="HoverContainerType"/>.
    /// </summary>
    protected virtual DependencyObject? GetHoverContainer(DependencyObject? origin)
    {
        if (HoverContainerType == null)
            return _element;

        for (DependencyObject? current = origin; current != null; current = GetParent(current))
        {
            if (HoverContainerType.IsInstanceOfType(current))
                return current;
        }

        return null;
    }

    private static DependencyObject? GetParent(DependencyObject element)
    {
        // ContentElement and FrameworkContentElement are not in the visual tree
        return element is Visual or System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetParent(element)
            : LogicalTreeHelper.GetParent(element);
    }

    #endregion

    #region Overridables

    /// <summary>
    /// Chooses the effect of the drop among the ones the source allows : Control copies, Shift moves, and a move is
    /// preferred by default.
    /// </summary>
    protected virtual DragDropEffects GetEffect(DropContext context, DragDropEffects allowed)
    {
        bool canCopy = allowed.HasFlag(DragDropEffects.Copy);
        bool canMove = allowed.HasFlag(DragDropEffects.Move);

        if (context.KeyStates.HasFlag(DragDropKeyStates.ControlKey) && canCopy)
            return DragDropEffects.Copy;
        if (context.KeyStates.HasFlag(DragDropKeyStates.ShiftKey) && canMove)
            return DragDropEffects.Move;

        if (canMove)
            return DragDropEffects.Move;
        if (canCopy)
            return DragDropEffects.Copy;
        return DragDropEffects.None;
    }

    /// <summary>
    /// Called on every move over a valid target, to apply an effect following the cursor.
    /// </summary>
    protected virtual void OnPassingOver(DropContext context)
    { }

    /// <summary>
    /// Checks the dropped data and the target : is the target the source, are the data of the expected type, ...
    /// </summary>
    protected abstract bool CanDrop(DropContext context);

    /// <summary>
    /// Applies the consequences of the drop (copy the files, move the node, ...).
    /// </summary>
    protected abstract void ApplyDrop(DropContext context);

    #endregion
}

/// <summary>
/// A <see cref="DropHandler"/> that only accepts a <typeparamref name="TData"/> dragged from inside the application.
/// </summary>
public abstract class DropHandler<TData> : DropHandler where TData : class
{
    /// <inheritdoc/>
    protected override bool CanDrop(DropContext context) =>
        context.Data is TData data && CanDrop(data, context);

    /// <inheritdoc/>
    protected override void ApplyDrop(DropContext context)
    {
        if (context.Data is TData data)
            ApplyDrop(data, context);
    }

    /// <inheritdoc cref="DropHandler.CanDrop"/>
    protected virtual bool CanDrop(TData data, DropContext context) => true;

    /// <inheritdoc cref="DropHandler.ApplyDrop"/>
    protected abstract void ApplyDrop(TData data, DropContext context);
}
