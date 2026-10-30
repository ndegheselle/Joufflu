using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Joufflu.Toolkit;

/// <summary>
/// Gives its child the width of the narrowest <see cref="SharedWidth"/> of its group, so that inputs
/// sitting at different depths of a tree, each with a different room left, line up.
/// <para>
/// Members are grouped by their closest ancestor marked <see cref="IsScopeProperty"/>. A member
/// stretches over the room its parent gives it and lays its child against its right edge, as wide as
/// the smallest room among the visible members: with their right edges already aligned, their left
/// edges line up too.
/// </para>
/// <para>
/// The shared width only changes how the child is arranged, never the size the member asks for. The
/// room a member is given can't depend on it, so a parent sized to its content (a centered dialog, an
/// <c>Auto</c> column, a horizontal scroll) can't keep bouncing between two widths.
/// </para>
/// <example>
/// <code>
/// &lt;StackPanel toolkit:SharedWidth.IsScope="True"&gt;
///     &lt;toolkit:SharedWidth&gt;
///         &lt;TextBox /&gt;
///     &lt;/toolkit:SharedWidth&gt;
/// &lt;/StackPanel&gt;
/// </code>
/// </example>
/// </summary>
public class SharedWidth : Decorator
{
    public static readonly DependencyProperty IsScopeProperty = DependencyProperty.RegisterAttached(
        "IsScope",
        typeof(bool),
        typeof(SharedWidth),
        new PropertyMetadata(false));

    public static bool GetIsScope(DependencyObject element) => (bool)element.GetValue(IsScopeProperty);

    public static void SetIsScope(DependencyObject element, bool value) => element.SetValue(IsScopeProperty, value);

    private static readonly ConditionalWeakTable<FrameworkElement, Group> Groups = new();

    /// <summary>The group joined, kept so it can be left on <c>Unloaded</c>, when the scope is already out of reach.</summary>
    private Group? _group;

    /// <summary>The width this member was last arranged at, the most its child can take.</summary>
    private double _room;

    /// <summary>The width its group settled on, <c>NaN</c> while it has none.</summary>
    private double _sharedWidth = double.NaN;

    public SharedWidth()
    {
        Loaded += (_, _) => Join();
        Unloaded += (_, _) => Leave();
    }

    private void Join()
    {
        Leave();
        FrameworkElement? scope = FindScope(this);
        if (scope == null)
            return;

        _group = Groups.GetValue(scope, _ => new Group(scope));
        _group.Add(this);
    }

    private void Leave()
    {
        _group?.Remove(this);
        _group = null;
        ShareWidth(double.NaN);
    }

    private void ShareWidth(double width)
    {
        if (width.Equals(_sharedWidth))
            return;

        _sharedWidth = width;
        // Only the arrangement depends on it: the size asked of the parent stays the same.
        InvalidateArrange();
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        _room = arrangeSize.Width;
        if (Child == null)
            return arrangeSize;

        double width = double.IsNaN(_sharedWidth) ? _room : Math.Min(_sharedWidth, _room);
        Child.Arrange(new Rect(_room - width, 0, width, arrangeSize.Height));
        return arrangeSize;
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
        private readonly HashSet<SharedWidth> _members = [];

        public void Add(SharedWidth member)
        {
            // Only listened to while there is something to line up, LayoutUpdated firing on every pass.
            if (_members.Count == 0)
                scope.LayoutUpdated += OnLayoutUpdated;
            _members.Add(member);
            // Members join on Loaded, after the layout pass: without a new one, LayoutUpdated never comes.
            member.InvalidateArrange();
        }

        public void Remove(SharedWidth member)
        {
            if (!_members.Remove(member) || _members.Count > 0)
                return;
            scope.LayoutUpdated -= OnLayoutUpdated;
        }

        /// <summary>
        /// The rooms don't depend on the shared width, so the pass this one triggers finds the same
        /// width and stops there.
        /// </summary>
        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            var visible = _members.Where(member => member.IsVisible).ToList();
            if (visible.Count == 0)
                return;

            double width = visible.Min(member => member._room);
            if (width <= 0)
                return;

            foreach (var member in visible)
                member.ShareWidth(width);
        }
    }
}
