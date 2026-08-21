using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Extensions;

/// <summary>
/// Sides of a <see cref="Thickness"/> kept by <see cref="Derive.BorderSidesProperty"/>.
/// </summary>
[Flags]
public enum ThicknessSides
{
    None = 0,
    Left = 1,
    Top = 2,
    Right = 4,
    Bottom = 8,
    Horizontal = Left | Right,
    Vertical = Top | Bottom,
    All = Left | Top | Right | Bottom
}

/// <summary>
/// Corners of a <see cref="CornerRadius"/> kept by <see cref="Derive.CornersProperty"/>.
/// </summary>
[Flags]
public enum Corners
{
    None = 0,
    TopLeft = 1,
    TopRight = 2,
    BottomRight = 4,
    BottomLeft = 8,
    Top = TopLeft | TopRight,
    Bottom = BottomLeft | BottomRight,
    Left = TopLeft | BottomLeft,
    Right = TopRight | BottomRight,
    All = TopLeft | TopRight | BottomRight | BottomLeft
}

/// <summary>
/// Builds a border thickness, a margin or a <see cref="CornerRadius"/> from a single scalar resource,
/// keeping only the requested sides or corners.
/// <para>
/// A <c>&lt;Thickness&gt;</c> declared in a <see cref="ResourceDictionary"/> is baked at parse time:
/// its <c>Left</c>/<c>Top</c>/<c>Right</c>/<c>Bottom</c> are plain CLR properties, so they can only be
/// fed with <c>StaticResource</c> and never follow a later theme change. These attached properties put
/// a real <c>DynamicResource</c> on the element instead, so a scalar edited at runtime (by a theme
/// customizer, for instance) flows through without any derived key having to be re-pushed by hand.
/// </para>
/// <example>
/// A border rounded on its top corners only, drawn on every side but the bottom:
/// <code>
/// &lt;Border
///     extensions:Derive.BorderThickness="{x:Static joufflu:Dimensions.Thickness}"
///     extensions:Derive.BorderSides="Left,Top,Right"
///     extensions:Derive.CornerRadius="{x:Static joufflu:Dimensions.Radius}"
///     extensions:Derive.Corners="Top" /&gt;
/// </code>
/// </example>
/// </summary>
public static class Derive
{
    #region BorderThickness

    /// <summary>
    /// Resource key of the scalar (a <see cref="double"/>) or <see cref="Thickness"/> the border
    /// thickness is derived from.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.RegisterAttached(
        "BorderThickness",
        typeof(object),
        typeof(Derive),
        new PropertyMetadata(null, OnBorderKeyChanged));

    /// <summary>
    /// Sides kept from the derived thickness. Defaults to <see cref="ThicknessSides.All"/>.
    /// </summary>
    public static readonly DependencyProperty BorderSidesProperty = DependencyProperty.RegisterAttached(
        "BorderSides",
        typeof(ThicknessSides),
        typeof(Derive),
        new PropertyMetadata(ThicknessSides.All, OnBorderMaskChanged));

    /// <summary>
    /// Holds the live value of the resource pointed at by <see cref="BorderThicknessProperty"/>.
    /// </summary>
    private static readonly DependencyProperty BorderSourceProperty = DependencyProperty.RegisterAttached(
        "BorderSource",
        typeof(object),
        typeof(Derive),
        new PropertyMetadata(null, OnBorderMaskChanged));

    public static object? GetBorderThickness(DependencyObject element) => element.GetValue(BorderThicknessProperty);

    public static void SetBorderThickness(DependencyObject element, object? value)
        => element.SetValue(BorderThicknessProperty, value);

    public static ThicknessSides GetBorderSides(DependencyObject element)
        => (ThicknessSides)element.GetValue(BorderSidesProperty);

    public static void SetBorderSides(DependencyObject element, ThicknessSides value)
        => element.SetValue(BorderSidesProperty, value);

    private static void OnBorderKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => Track(d, BorderSourceProperty, e.NewValue);

    private static void OnBorderMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        object? source = element.GetValue(BorderSourceProperty);
        if (source == null)
            return;

        Thickness value = Mask(ToThickness(source, "BorderThickness"), GetBorderSides(element));
        element.SetCurrentValue(ResolveBorderThickness(element), value);
    }

    /// <summary>
    /// <see cref="Border"/> and <see cref="Control"/> each declare their own BorderThickness property.
    /// </summary>
    private static DependencyProperty ResolveBorderThickness(FrameworkElement element) => element switch
    {
        Border => Border.BorderThicknessProperty,
        Control => Control.BorderThicknessProperty,
        _ => throw new InvalidOperationException(
            $"Derive.BorderThickness is only supported on Border and Control, not on {element.GetType().Name}.")
    };

    #endregion

    #region CornerRadius

    /// <summary>
    /// Resource key of the scalar (a <see cref="double"/>) or <see cref="System.Windows.CornerRadius"/>
    /// the corner radius is derived from.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
        "CornerRadius",
        typeof(object),
        typeof(Derive),
        new PropertyMetadata(null, OnCornerKeyChanged));

    /// <summary>
    /// Corners kept from the derived radius. Defaults to <see cref="Corners.All"/>.
    /// </summary>
    public static readonly DependencyProperty CornersProperty = DependencyProperty.RegisterAttached(
        "Corners",
        typeof(Corners),
        typeof(Derive),
        new PropertyMetadata(Corners.All, OnCornerMaskChanged));

    /// <summary>
    /// Holds the live value of the resource pointed at by <see cref="CornerRadiusProperty"/>.
    /// </summary>
    private static readonly DependencyProperty CornerSourceProperty = DependencyProperty.RegisterAttached(
        "CornerSource",
        typeof(object),
        typeof(Derive),
        new PropertyMetadata(null, OnCornerMaskChanged));

    public static object? GetCornerRadius(DependencyObject element) => element.GetValue(CornerRadiusProperty);

    public static void SetCornerRadius(DependencyObject element, object? value)
        => element.SetValue(CornerRadiusProperty, value);

    public static Corners GetCorners(DependencyObject element) => (Corners)element.GetValue(CornersProperty);

    public static void SetCorners(DependencyObject element, Corners value) => element.SetValue(CornersProperty, value);

    private static void OnCornerKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => Track(d, CornerSourceProperty, e.NewValue);

    private static void OnCornerMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        object? source = element.GetValue(CornerSourceProperty);
        if (source == null)
            return;

        CornerRadius value = Mask(ToCornerRadius(source), GetCorners(element));
        element.SetCurrentValue(ResolveCornerRadius(element), value);
    }

    private static DependencyProperty ResolveCornerRadius(FrameworkElement element) => element switch
    {
        Border => Border.CornerRadiusProperty,
        _ => throw new InvalidOperationException(
            $"Derive.CornerRadius is only supported on Border, not on {element.GetType().Name}.")
    };

    #endregion

    #region Margin

    /// <summary>
    /// Resource key of the scalar (a <see cref="double"/>) or <see cref="Thickness"/> the margin is
    /// derived from.
    /// </summary>
    public static readonly DependencyProperty MarginProperty = DependencyProperty.RegisterAttached(
        "Margin",
        typeof(object),
        typeof(Derive),
        new PropertyMetadata(null, OnMarginKeyChanged));

    /// <summary>
    /// Sides kept from the derived margin. Defaults to <see cref="ThicknessSides.All"/>.
    /// </summary>
    public static readonly DependencyProperty MarginSidesProperty = DependencyProperty.RegisterAttached(
        "MarginSides",
        typeof(ThicknessSides),
        typeof(Derive),
        new PropertyMetadata(ThicknessSides.All, OnMarginMaskChanged));

    /// <summary>
    /// Holds the live value of the resource pointed at by <see cref="MarginProperty"/>.
    /// </summary>
    private static readonly DependencyProperty MarginSourceProperty = DependencyProperty.RegisterAttached(
        "MarginSource",
        typeof(object),
        typeof(Derive),
        new PropertyMetadata(null, OnMarginMaskChanged));

    public static object? GetMargin(DependencyObject element) => element.GetValue(MarginProperty);

    public static void SetMargin(DependencyObject element, object? value) => element.SetValue(MarginProperty, value);

    public static ThicknessSides GetMarginSides(DependencyObject element)
        => (ThicknessSides)element.GetValue(MarginSidesProperty);

    public static void SetMarginSides(DependencyObject element, ThicknessSides value)
        => element.SetValue(MarginSidesProperty, value);

    private static void OnMarginKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => Track(d, MarginSourceProperty, e.NewValue);

    private static void OnMarginMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        object? source = element.GetValue(MarginSourceProperty);
        if (source == null)
            return;

        Thickness value = Mask(ToThickness(source, "Margin"), GetMarginSides(element));
        element.SetCurrentValue(FrameworkElement.MarginProperty, value);
    }

    #endregion

    #region Resolution

    /// <summary>
    /// Points <paramref name="source"/> at the resource <paramref name="key"/>, so the derived value
    /// follows every later change of that resource.
    /// </summary>
    private static void Track(DependencyObject d, DependencyProperty source, object? key)
    {
        if (d is not FrameworkElement element)
            return;

        if (key == null)
            element.SetValue(source, null);
        else
            element.SetResourceReference(source, key);
    }

    private static Thickness ToThickness(object source, string property) => source switch
    {
        Thickness thickness => thickness,
        IConvertible convertible => new Thickness(convertible.ToDouble(null)),
        _ => throw new InvalidOperationException(
            $"Derive.{property} expects a Thickness or a numeric resource, got {source.GetType().Name}.")
    };

    private static CornerRadius ToCornerRadius(object source) => source switch
    {
        CornerRadius radius => radius,
        IConvertible convertible => new CornerRadius(convertible.ToDouble(null)),
        _ => throw new InvalidOperationException(
            $"Derive.CornerRadius expects a CornerRadius or a numeric resource, got {source.GetType().Name}.")
    };

    private static Thickness Mask(Thickness value, ThicknessSides sides) => new(
        sides.HasFlag(ThicknessSides.Left) ? value.Left : 0,
        sides.HasFlag(ThicknessSides.Top) ? value.Top : 0,
        sides.HasFlag(ThicknessSides.Right) ? value.Right : 0,
        sides.HasFlag(ThicknessSides.Bottom) ? value.Bottom : 0);

    private static CornerRadius Mask(CornerRadius value, Corners corners) => new(
        corners.HasFlag(Corners.TopLeft) ? value.TopLeft : 0,
        corners.HasFlag(Corners.TopRight) ? value.TopRight : 0,
        corners.HasFlag(Corners.BottomRight) ? value.BottomRight : 0,
        corners.HasFlag(Corners.BottomLeft) ? value.BottomLeft : 0);

    #endregion
}
