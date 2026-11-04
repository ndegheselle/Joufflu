using System.Windows;

namespace Joufflu.Assets.Fonts;

/// <summary>Where a <see cref="Chevron"/> sits next to the header it opens.</summary>
public enum ChevronPlacement
{
    /// <summary>Before the header, as in a tree: points right, then down once open.</summary>
    Leading,

    /// <summary>After the header, as in an expander: points down, then up once open.</summary>
    Trailing
}

/// <summary>
/// The chevron of a header that opens content below it: a tree item, an expander, a menu group.
/// Its glyph follows <see cref="IsExpanded"/> and <see cref="Placement"/>, with no animation, so a
/// row reused while scrolling never turns as it comes into view.
/// </summary>
public class Chevron : FontIcon
{
    static Chevron()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Chevron), new FrameworkPropertyMetadata(typeof(FontIcon)));
    }

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(Chevron),
        new PropertyMetadata(false, (d, _) => ((Chevron)d).Refresh()));

    /// <summary>Whether the content the chevron opens is shown.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
        nameof(Placement), typeof(ChevronPlacement), typeof(Chevron),
        new PropertyMetadata(ChevronPlacement.Leading, (d, _) => ((Chevron)d).Refresh()));

    public ChevronPlacement Placement
    {
        get => (ChevronPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public Chevron() { Refresh(); }

    private void Refresh() => Text = GlyphOf(Placement, IsExpanded);

    /// <summary>The glyph of a chevron at [placement], open or not.</summary>
    public static string GlyphOf(ChevronPlacement placement, bool isExpanded) => (placement, isExpanded) switch
    {
        (ChevronPlacement.Leading, false) => LucideFontIcons.ChevronRight,
        (ChevronPlacement.Leading, true) => LucideFontIcons.ChevronDown,
        (ChevronPlacement.Trailing, false) => LucideFontIcons.ChevronDown,
        _ => LucideFontIcons.ChevronUp
    };
}
