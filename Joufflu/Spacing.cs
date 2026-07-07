using System.Windows;
using System.Windows.Controls;

namespace Joufflu;

/// <summary>
/// Attached properties that add a gap between the children of a <see cref="Panel"/>
/// (<see cref="StackPanel"/>, <see cref="WrapPanel"/> and <see cref="Grid"/>) by
/// applying margins to those children.
/// <para>
/// Use <see cref="GapProperty"/> for a uniform gap on both axes, and
/// <see cref="HorizontalGapProperty"/> / <see cref="VerticalGapProperty"/>
/// to override a single axis.
/// Otherwise spacing only applies when a value is explicitly set.
/// </para>
/// <para>
/// Because spacing is implemented through child margins, any margin set directly on a
/// child is overwritten while spacing is active.
/// </para>
/// </summary>
public static class Spacing
{
    /// <summary>Uniform gap (in DIPs) applied on both axes between children.</summary>
    public static readonly DependencyProperty GapProperty =
        DependencyProperty.RegisterAttached(
            "Gap",
            typeof(double),
            typeof(Spacing),
            new FrameworkPropertyMetadata(double.NaN, OnGapChanged));

    public static double GetGap(DependencyObject obj) => (double)obj.GetValue(GapProperty);
    public static void SetGap(DependencyObject obj, double value) => obj.SetValue(GapProperty, value);

    /// <summary>Horizontal gap (in DIPs); overrides <see cref="GapProperty"/> on the horizontal axis.</summary>
    public static readonly DependencyProperty HorizontalGapProperty =
        DependencyProperty.RegisterAttached(
            "HorizontalGap",
            typeof(double),
            typeof(Spacing),
            new FrameworkPropertyMetadata(double.NaN, OnGapChanged));

    public static double GetHorizontalGap(DependencyObject obj) => (double)obj.GetValue(HorizontalGapProperty);
    public static void SetHorizontalGap(DependencyObject obj, double value) => obj.SetValue(HorizontalGapProperty, value);

    /// <summary>Vertical gap (in DIPs); overrides <see cref="GapProperty"/> on the vertical axis.</summary>
    public static readonly DependencyProperty VerticalGapProperty =
        DependencyProperty.RegisterAttached(
            "VerticalGap",
            typeof(double),
            typeof(Spacing),
            new FrameworkPropertyMetadata(double.NaN, OnGapChanged));

    public static double GetVerticalGap(DependencyObject obj) => (double)obj.GetValue(VerticalGapProperty);
    public static void SetVerticalGap(DependencyObject obj, double value) => obj.SetValue(VerticalGapProperty, value);

    // Marks a panel whose children collection we already observe, so we only hook once.
    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IsHooked",
            typeof(bool),
            typeof(Spacing),
            new PropertyMetadata(false));

    private static void OnGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Panel panel)
            return;

        if (!(bool)panel.GetValue(IsHookedProperty))
        {
            panel.SetValue(IsHookedProperty, true);
            // Re-apply after every layout pass so dynamically added/removed children
            // (e.g. an ItemsControl's generated items) keep their spacing. Applying an
            // unchanged margin is a no-op and does not re-trigger layout, so this settles.
            panel.LayoutUpdated += (_, _) => ApplySpacing(panel);
        }

        ApplySpacing(panel);
    }

    private static void ApplySpacing(Panel panel)
    {
        double fallbackH = 0, fallbackV = 0;
        double horizontal = Resolve(GetHorizontalGap(panel), GetGap(panel), fallbackH);
        double vertical = Resolve(GetVerticalGap(panel), GetGap(panel), fallbackV);

        switch (panel)
        {
            case Grid grid:
                ApplyToGrid(grid, horizontal, vertical);
                break;
            case StackPanel stack:
                ApplyToStack(stack, horizontal, vertical);
                break;
            default: // WrapPanel and any other Panel
                ApplyUniform(panel, horizontal, vertical);
                break;
        }
    }

    // A per-axis override wins; otherwise the uniform value; otherwise the fallback
    private static double Resolve(double axisValue, double uniform, double fallback)
    {
        if (!double.IsNaN(axisValue))
            return axisValue;
        return double.IsNaN(uniform) ? fallback : uniform;
    }

    // StackPanel is one-dimensional: gap every child except the first, on the leading
    // edge of the stacking axis. This leaves no outer margin on the panel.
    private static void ApplyToStack(StackPanel stack, double horizontal, double vertical)
    {
        bool isVertical = stack.Orientation == Orientation.Vertical;
        for (int i = 0; i < stack.Children.Count; i++)
        {
            if (stack.Children[i] is not FrameworkElement child)
                continue;

            double gap = i == 0 ? 0 : (isVertical ? vertical : horizontal);
            SetMargin(child, isVertical ? new Thickness(0, gap, 0, 0) : new Thickness(gap, 0, 0, 0));
        }
    }

    // Grid gaps are driven by each child's Grid.Row / Grid.Column: anything past the
    // first row/column gets a leading margin, so gaps sit only between cells.
    private static void ApplyToGrid(Grid grid, double horizontal, double vertical)
    {
        foreach (UIElement element in grid.Children)
        {
            if (element is not FrameworkElement child)
                continue;

            double left = Grid.GetColumn(child) > 0 ? horizontal : 0;
            double top = Grid.GetRow(child) > 0 ? vertical : 0;
            SetMargin(child, new Thickness(left, top, 0, 0));
        }
    }

    // WrapPanel (and other panels) wrap in both directions, so line breaks are not known
    // up front. Apply a trailing margin on both axes to every child; gaps between items
    // are exact, at the cost of a trailing margin on the panel's edges.
    private static void ApplyUniform(Panel panel, double horizontal, double vertical)
    {
        foreach (UIElement element in panel.Children)
        {
            if (element is FrameworkElement child)
                SetMargin(child, new Thickness(0, 0, horizontal, vertical));
        }
    }

    private static void SetMargin(FrameworkElement child, Thickness margin)
    {
        if (child.Margin != margin)
            child.Margin = margin;
    }
}
