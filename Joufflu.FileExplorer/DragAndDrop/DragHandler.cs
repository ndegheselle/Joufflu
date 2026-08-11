using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.FileExplorer.DragAndDrop;

/// <summary>
/// Starts and follows a drag, the source side of a drag and drop. Meant to be exposed by a view model and bound with
/// <see cref="Drag.HandlerProperty"/> : <c>dnd:Drag.Handler="{Binding DragHandler}"</c>.
/// </summary>
/// <remarks>
/// A handler drives a single element, the one it is attached to. Override <see cref="GetData"/> to choose what is
/// dragged, <see cref="CanDrag"/> to refuse a drag, and set <see cref="AdornerTemplate"/> to show the dragged object
/// under the cursor.
/// </remarks>
public partial class DragHandler : ObservableObject
{
    #region Properties

    /// <summary>
    /// Wait for the cursor to move of the system minimum drag distance before starting the drag, to avoid starting one
    /// by mistake on a simple click. Set it to false when the element only exists to be dragged (a drag handle).
    /// </summary>
    public bool UseMinimumDistance { get; set; } = true;

    /// <summary>
    /// Effects the drop targets are allowed to choose from, see <see cref="DropHandler.GetEffect"/>.
    /// </summary>
    public DragDropEffects AllowedEffects { get; set; } = DragDropEffects.Copy | DragDropEffects.Move;

    /// <summary>
    /// Visual shown under the cursor while dragging, the dragged object being its <c>DataContext</c>. No adorner is
    /// shown when it is null, the drag and drop icon of the system is used instead.
    /// </summary>
    public DataTemplate? AdornerTemplate { get; set; }

    /// <summary>
    /// True from the moment a drag starts until the drop is applied or cancelled. Bindable, and mirrored on the
    /// dragged element by <see cref="Drag.IsDragSourceProperty"/>.
    /// </summary>
    [ObservableProperty]
    private bool isDragging;

    private FrameworkElement? _element;
    private Point _clickPosition;
    private Point _position;
    private bool _hasValidClick;

    private DragAdorner? _adorner;

    #endregion

    #region Attachment

    /// <summary>
    /// Called by <see cref="Drag"/> when the handler is bound to an element.
    /// </summary>
    internal void Attach(FrameworkElement element) { _element = element; }

    /// <summary>
    /// Called by <see cref="Drag"/> when the handler is unbound from an element.
    /// </summary>
    internal void Detach(FrameworkElement element)
    {
        if (_element != element)
            return;

        HideAdorner();
        _element = null;
    }

    #endregion

    #region UI events

    /// <summary>
    /// On <see cref="UIElement.MouseDown"/>, remembers where a drag could start from.
    /// </summary>
    public virtual void HandleMouseDown(object sender, MouseButtonEventArgs e)
    {
        // A double click is not a drag, and nothing can be dragged before the element is laid out
        _hasValidClick = e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1 && _element?.IsLoaded == true;

        if (_hasValidClick)
            _clickPosition = e.GetPosition(_element);
    }

    /// <summary>
    /// On <see cref="UIElement.MouseMove"/>, starts the drag once the cursor moved far enough.
    /// </summary>
    public virtual void HandleMouseMove(object sender, MouseEventArgs e)
    {
        if (!_hasValidClick || _element == null)
            return;

        // The button may have been released outside of the element, without a MouseUp
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            _hasValidClick = false;
            return;
        }

        _position = e.GetPosition(_element);
        if (UseMinimumDistance && !HasExceededMinimumDistance(_position))
            return;

        object? data = GetData(e.OriginalSource as FrameworkElement);
        if (data == null || !CanDrag(data))
        {
            // Don't test again until the next click, the state can't become valid while the button stays pressed
            _hasValidClick = false;
            return;
        }

        StartDrag(sender, data);
    }

    /// <summary>
    /// On <see cref="System.Windows.DragDrop.GiveFeedback"/>, replaces the cursor of the system.
    /// </summary>
    /// <remarks>
    /// Without this, <see cref="DragDropEffects.Move"/> shows the same barred cursor as
    /// <see cref="DragDropEffects.None"/>, making a valid move look refused.
    /// </remarks>
    public virtual void HandleGiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
        e.UseDefaultCursors = false;
        Mouse.SetCursor(GetCursor(e.Effects));
        e.Handled = true;

        // The drag loop raises this on every move, it is the only callback that reliably runs while DoDragDrop blocks
        MoveAdornerToCursor();
    }

    /// <summary>
    /// On <see cref="System.Windows.DragDrop.QueryContinueDrag"/>, cancels the drag on Escape.
    /// </summary>
    public virtual void HandleQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
        if (e.EscapePressed)
            e.Action = DragAction.Cancel;
    }

    #endregion

    #region Drag

    private bool HasExceededMinimumDistance(Point position)
    {
        return Math.Abs(position.X - _clickPosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _clickPosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
    }

    /// <summary>
    /// Runs the drag and drop, blocking until the drop is applied or cancelled.
    /// </summary>
    protected virtual void StartDrag(object sender, object data)
    {
        // Prevent starting a second drag from the same click
        _hasValidClick = false;
        IsDragging = true;

        try
        {
            ShowAdorner(data);
            System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, DragDropData.Pack(data), AllowedEffects);
        }
        catch (ExternalException)
        {
            // Drag and drop is a shared resource, it can be used by another process
        }
        catch (InvalidOperationException)
        {
            // FIXME : check why this exception can happen (TextBox inside a dragged element ?)
        }
        finally
        {
            HideAdorner();
            IsDragging = false;
            OnDragFinished();
        }
    }

    /// <summary>
    /// Called once the drag ended, whether it was dropped or cancelled.
    /// </summary>
    protected virtual void OnDragFinished()
    { }

    /// <summary>
    /// Moves the adorner under the cursor. <see cref="Mouse.GetPosition"/> is not usable here, it stays on the
    /// position the drag started from while <see cref="System.Windows.DragDrop.DoDragDrop"/> blocks, so the position
    /// is read from the system.
    /// </summary>
    private void MoveAdornerToCursor()
    {
        if (_adorner == null || _element?.IsLoaded != true || PresentationSource.FromVisual(_element) == null)
            return;

        if (!GetCursorPos(out NativePoint cursor))
            return;

        _position = _element.PointFromScreen(new Point(cursor.X, cursor.Y));
        _adorner.UpdatePosition(_position);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out NativePoint point);

    #endregion

    #region Adorner

    private void ShowAdorner(object data)
    {
        HideAdorner();

        if (_element == null)
            return;

        FrameworkElement? content = CreateAdornerContent(data);
        AdornerLayer? layer = AdornerLayer.GetAdornerLayer(_element);
        // The element may not be under an AdornerDecorator, a Window template provides one but a standalone element
        // may not
        if (content == null || layer == null)
            return;

        _adorner = new DragAdorner(_element, content, _position);
        layer.Add(_adorner);
    }

    private void HideAdorner()
    {
        if (_adorner == null || _element == null)
            return;

        AdornerLayer.GetAdornerLayer(_element)?.Remove(_adorner);
        _adorner = null;
    }

    #endregion

    #region Overridables

    /// <summary>
    /// Gets the object to drag from the element under the cursor, its <c>DataContext</c> by default. Return null to
    /// refuse the drag.
    /// </summary>
    protected virtual object? GetData(FrameworkElement? source) => source?.DataContext;

    /// <summary>
    /// Refuses the drag of <paramref name="data"/>, for cases the element is in a state that doesn't allow it.
    /// </summary>
    protected virtual bool CanDrag(object data) => true;

    /// <summary>
    /// Builds the visual shown under the cursor, from <see cref="AdornerTemplate"/> by default. Return null for no
    /// adorner.
    /// </summary>
    protected virtual FrameworkElement? CreateAdornerContent(object data)
    {
        if (AdornerTemplate == null)
            return null;

        return new ContentPresenter { Content = data, ContentTemplate = AdornerTemplate };
    }

    /// <summary>
    /// Cursor shown for the effect the target under the cursor chose : the standard cursor when the drop is allowed,
    /// and the refused one otherwise.
    /// </summary>
    protected virtual Cursor GetCursor(DragDropEffects effects)
    {
        if (effects.HasFlag(DragDropEffects.Copy))
            return Cursors.Cross;
        if (effects.HasFlag(DragDropEffects.Move))
            return Cursors.Arrow;
        return Cursors.No;
    }

    #endregion
}
