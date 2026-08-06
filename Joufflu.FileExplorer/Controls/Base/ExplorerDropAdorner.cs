using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Joufflu;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// Outlines the directory a drag is hovering, so the user sees where the content would land.
/// </summary>
/// <remarks>
/// An adorner rather than a trigger on the item container : the container style of a tree is a named resource applied
/// through the style of the <c>TreeView</c>, so triggering on it would mean deriving from that style, and would break
/// for any consumer setting an <c>ItemContainerStyle</c> of its own.
/// </remarks>
internal sealed class ExplorerDropAdorner : Adorner
{
    private const double BorderThickness = 2;

    private ExplorerDropAdorner(UIElement adornedElement) : base(adornedElement)
    {
        // The adorner only paints : it must never take the mouse away from the drag.
        IsHitTestVisible = false;
    }

    /// <summary>
    /// Puts an adorner on an element, or null when the element has no adorner layer : outside of an
    /// <c>AdornerDecorator</c> the highlight is simply not shown, being decoration.
    /// </summary>
    public static ExplorerDropAdorner? Attach(UIElement element)
    {
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer == null)
            return null;

        var adorner = new ExplorerDropAdorner(element);
        layer.Add(adorner);
        return adorner;
    }

    public void Detach()
    {
        AdornerLayer.GetAdornerLayer(AdornedElement)?.Remove(this);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var brush = TryFindResource(Brushes.PrimaryBrush) as Brush ?? System.Windows.Media.Brushes.DodgerBlue;

        var pen = new Pen(brush, BorderThickness);
        var fill = brush.Clone();
        fill.Opacity = 0.15;

        // Inset by half the thickness so the whole stroke stays inside the container.
        var bounds = new Rect(AdornedElement.RenderSize);
        bounds.Inflate(-BorderThickness / 2, -BorderThickness / 2);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        double radius = TryFindResource(Dimensions.CornerRadius) is CornerRadius corner ? corner.TopLeft : 0;
        drawingContext.DrawRoundedRectangle(fill, pen, bounds, radius, radius);
    }
}
