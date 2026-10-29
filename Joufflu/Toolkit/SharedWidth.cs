using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Joufflu.Toolkit;

/// <summary>
/// Gives every element marked <see cref="IsSharedProperty"/> the width of the narrowest one, so that
/// inputs sitting at different depths of a tree, each with a different room left, line up.
/// <para>
/// Elements are grouped by their closest ancestor marked <see cref="IsScopeProperty"/>. Each one is
/// right aligned and given the smallest width its layout slot offers among the visible members: with
/// their right edges already aligned, their left edges line up too. The slot is what the parent
/// hands the element, whatever its own <c>Width</c>, so it still says how much room there really is
/// once the width is set, and a member leaving or growing room lets the others widen again.
/// </para>
/// <para>
/// The scope must constrain the width of its content: a horizontal <c>ScrollViewer</c> would grow
/// with the shared width and never let it shrink.
/// </para>
/// <example>
/// <code>
/// &lt;StackPanel toolkit:SharedWidth.IsScope="True"&gt;
///     &lt;TextBox toolkit:SharedWidth.IsShared="True" /&gt;
/// &lt;/StackPanel&gt;
/// </code>
/// </example>
/// </summary>
public static class SharedWidth
{
    public static readonly DependencyProperty IsScopeProperty = DependencyProperty.RegisterAttached(
        "IsScope",
        typeof(bool),
        typeof(SharedWidth),
        new PropertyMetadata(false));

    public static bool GetIsScope(DependencyObject element) => (bool)element.GetValue(IsScopeProperty);

    public static void SetIsScope(DependencyObject element, bool value) => element.SetValue(IsScopeProperty, value);

    public static readonly DependencyProperty IsSharedProperty = DependencyProperty.RegisterAttached(
        "IsShared",
        typeof(bool),
        typeof(SharedWidth),
        new PropertyMetadata(false, OnIsSharedChanged));

    public static bool GetIsShared(DependencyObject element) => (bool)element.GetValue(IsSharedProperty);

    public static void SetIsShared(DependencyObject element, bool value) => element.SetValue(IsSharedProperty, value);

    /// <summary>
    /// The group a member joined, kept so it can leave on <c>Unloaded</c>, when its ancestors are
    /// already out of reach.
    /// </summary>
    private static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
        "Group",
        typeof(Group),
        typeof(SharedWidth),
        new PropertyMetadata(null));

    private static readonly ConditionalWeakTable<FrameworkElement, Group> Groups = new();

    private static void OnIsSharedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        element.Loaded -= OnLoaded;
        element.Unloaded -= OnUnloaded;
        Leave(element);

        if (e.NewValue is not true)
            return;

        element.HorizontalAlignment = HorizontalAlignment.Right;
        element.Loaded += OnLoaded;
        element.Unloaded += OnUnloaded;
        if (element.IsLoaded)
            Join(element);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e) => Join((FrameworkElement)sender);

    private static void OnUnloaded(object sender, RoutedEventArgs e) => Leave((FrameworkElement)sender);

    private static void Join(FrameworkElement element)
    {
        FrameworkElement? scope = FindScope(element);
        if (scope == null)
            return;

        Group group = Groups.GetValue(scope, _ => new Group(scope));
        group.Add(element);
        element.SetValue(GroupProperty, group);
    }

    private static void Leave(FrameworkElement element)
    {
        if (element.GetValue(GroupProperty) is not Group group)
            return;

        group.Remove(element);
        element.ClearValue(GroupProperty);
        element.ClearValue(FrameworkElement.WidthProperty);
    }

    private static FrameworkElement? FindScope(DependencyObject element)
    {
        DependencyObject? current = VisualTreeHelper.GetParent(element);
        while (current != null && !GetIsScope(current))
            current = VisualTreeHelper.GetParent(current);
        return current as FrameworkElement;
    }

    private sealed class Group(FrameworkElement scope)
    {
        private readonly HashSet<FrameworkElement> _members = [];

        public void Add(FrameworkElement member)
        {
            // Only listened to while there is something to line up, LayoutUpdated firing on every pass.
            if (_members.Count == 0)
            {
                scope.LayoutUpdated += OnLayoutUpdated;
                scope.SizeChanged += OnScopeSizeChanged;
            }
            _members.Add(member);
            // Members join on Loaded, after the layout pass: without a new one, LayoutUpdated never comes.
            scope.InvalidateArrange();
        }

        public void Remove(FrameworkElement member)
        {
            _members.Remove(member);
            if (_members.Count == 0)
            {
                scope.LayoutUpdated -= OnLayoutUpdated;
                scope.SizeChanged -= OnScopeSizeChanged;
            }
        }

        /// <summary>
        /// Under a horizontal scroll, the widths set by this group make the content as wide as they
        /// are, so the slots never shrink below them. Cleared, the next layout pass hands out the
        /// room really left, still before anything is rendered.
        /// </summary>
        private void OnScopeSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
                return;

            foreach (var member in _members)
                member.ClearValue(FrameworkElement.WidthProperty);
        }

        /// <summary>
        /// Only writes a width that changed, so the layout pass it triggers settles on the next one.
        /// </summary>
        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            var visible = _members.Where(member => member.IsVisible).ToList();
            if (visible.Count == 0)
                return;

            double width = visible.Min(member => LayoutInformation.GetLayoutSlot(member).Width - member.Margin.Left - member.Margin.Right);
            if (width <= 0)
                return;

            foreach (var member in visible)
            {
                if (Math.Abs(member.Width - width) > 0.5 || double.IsNaN(member.Width))
                    member.Width = width;
            }
        }
    }
}
