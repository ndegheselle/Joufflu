using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Joufflu.FileExplorer.DragAndDrop;

/// <summary>
/// Follows the cursor during a drag and drop to show what is being dragged, replacing the drag and drop icon of the
/// system. Drawn in the <see cref="AdornerLayer"/> of the dragged element, so it stays inside the window.
/// </summary>
public class DragAdorner : Adorner
{
    /// <summary>
    /// Offset from the cursor, so the adorner sits next to it and not over it.
    /// </summary>
    private static readonly Vector CursorOffset = new Vector(20, 0);

    private readonly FrameworkElement _child;
    private Point _position;

    /// <param name="adornedElement">Element owning the <see cref="AdornerLayer"/> the adorner is drawn in.</param>
    /// <param name="child">Visual shown under the cursor.</param>
    /// <param name="position">Cursor position, relative to <paramref name="adornedElement"/>.</param>
    public DragAdorner(UIElement adornedElement, FrameworkElement child, Point position) : base(adornedElement)
    {
        _child = child ?? throw new ArgumentNullException(nameof(child));
        _position = position;

        AddVisualChild(_child);
        // Let the dragged element behind stay visible
        _child.Opacity = 0.7;
        // Don't interfere with the drop hit testing
        IsHitTestVisible = false;
    }

    /// <summary>
    /// Moves the adorner under the cursor, ignoring the moves too small to be visible.
    /// </summary>
    /// <param name="position">Cursor position, relative to the adorned element.</param>
    public void UpdatePosition(Point position)
    {
        if (Math.Abs(_position.X - position.X) < 1 && Math.Abs(_position.Y - position.Y) < 1)
            return;

        _position = position;
        InvalidateArrange();
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        if (index != 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _child;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        // Never constrain the content, the adorner has the size of the adorned element and the content is usually
        // smaller but may be arranged partly outside of it
        _child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return base.MeasureOverride(constraint);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // On the right of the cursor, so it doesn't hide what is under it
        _child.Arrange(new Rect(_position + CursorOffset, _child.DesiredSize));
        return finalSize;
    }
}
