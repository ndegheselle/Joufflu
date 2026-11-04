using System.Windows;
using System.Windows.Media;

namespace Joufflu.Data.Controls;

/// <summary>
/// The indent of a row of <see cref="DataDisplay"/>, with a guide line per level above it. Each line
/// sits under the chevron of its level, as a tree view draws it under an expanded node: the rows
/// being laid flat, each one draws its part of the lines.
/// </summary>
public class DataGuides : FrameworkElement
{
    public static readonly DependencyProperty DepthProperty = DependencyProperty.Register(
        nameof(Depth), typeof(int), typeof(DataGuides),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>How many levels the row sits under the first one, as many lines.</summary>
    public int Depth
    {
        get => (int)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    public static readonly DependencyProperty ChevronSizeProperty = DependencyProperty.Register(
        nameof(ChevronSize), typeof(double), typeof(DataGuides),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>The width of a chevron: a line is drawn in its middle, a level indents by half of it.</summary>
    public double ChevronSize
    {
        get => (double)GetValue(ChevronSizeProperty);
        set => SetValue(ChevronSizeProperty, value);
    }

    public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(
        nameof(LineThickness), typeof(Thickness), typeof(DataGuides),
        new FrameworkPropertyMetadata(new Thickness(1), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>A border thickness, its left side being the width of a line.</summary>
    public Thickness LineThickness
    {
        get => (Thickness)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
        nameof(LineBrush), typeof(Brush), typeof(DataGuides),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush? LineBrush
    {
        get => (Brush?)GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    /// <summary>The indent of a level: half a chevron then the line, as the tree view's own.</summary>
    private double Step => ChevronSize / 2 + LineThickness.Left;

    protected override Size MeasureOverride(Size availableSize) => new(Depth * Step, 0);

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (LineBrush is null)
            return;

        for (int level = 0; level < Depth; level++)
        {
            var line = new Rect(level * Step + ChevronSize / 2, 0, LineThickness.Left, ActualHeight);
            drawingContext.DrawRectangle(LineBrush, null, line);
        }
    }
}
