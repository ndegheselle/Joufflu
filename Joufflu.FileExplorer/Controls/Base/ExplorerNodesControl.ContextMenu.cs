using System.Windows;
using System.Windows.Controls;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// The context menu of a node, of a selection, or of the empty space of the control.
/// </summary>
/// <remarks>
/// The menu is looked up as a <see cref="DataTemplate"/> keyed by node type and by
/// <see cref="MenuScope"/>, from the most specific type down to <see cref="object"/>, so that declaring one keyed on a
/// node type of its own is all a consumer needs to replace it.
/// </remarks>
public abstract partial class ExplorerNodesControl
{
    /// <summary>
    /// Single menu instance filled on opening : WPF captures the <see cref="FrameworkElement.ContextMenu"/> value
    /// before raising <see cref="FrameworkElement.ContextMenuOpening"/>, so the instance can't be replaced there.
    /// </summary>
    private readonly ContextMenu _contextMenu = new();

    private void AttachContextMenu(ItemsControl host)
    {
        host.ContextMenuOpening += OnItemsHostContextMenuOpening;
        host.ContextMenu = _contextMenu;
        _contextMenu.PlacementTarget = host;
    }

    private void DetachContextMenu(ItemsControl host)
    {
        host.ContextMenuOpening -= OnItemsHostContextMenuOpening;
        host.ContextMenu = null;
    }

    private void OnItemsHostContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        _contextMenu.Items.Clear();
        _contextMenu.DataContext = null;

        // Reading the clipboard is an interop call, so it happens once here rather than on every requery of Paste.
        Session?.RefreshClipboardState();

        var node = GetNodeAt(e.OriginalSource as DependencyObject);

        // Right clicking the empty space of the control targets the opened directory : that is where "New folder" and
        // "Paste" live, neither of them acting on a node.
        var nodes = node == null ? Array.Empty<IExplorerNode>() : GetMenuNodes(node);
        var directory = GetMenuDirectory(node);
        var scope = node == null
            ? MenuScope.Background
            : nodes.Count > 1 ? MenuScope.Multiple : MenuScope.Single;

        var template = FindContextMenuTemplate(node?.GetType() ?? directory?.GetType(), scope);

        if (template?.LoadContent() is not ContextMenu menu)
        {
            // Nothing declared for this type and scope : suppress the menu rather than showing an empty one.
            e.Handled = true;
            return;
        }

        MoveItems(menu, _contextMenu);
        _contextMenu.DataContext = new ExplorerMenuContext(this, nodes, directory);
    }

    /// <summary>
    /// Directory the menu acts in : the clicked node when it is one, its parent otherwise, and the opened directory
    /// when the click was on the empty space.
    /// </summary>
    private IExplorerDirectory? GetMenuDirectory(IExplorerNode? node) => node switch
    {
        IExplorerDirectory directory => directory,
        not null => node.Parent ?? Session?.Current,
        _ => Session?.Current
    };

    /// <summary>
    /// Moves the items of the menu loaded from a template to the persistent menu.
    /// </summary>
    private static void MoveItems(ContextMenu source, ContextMenu destination)
    {
        while (source.Items.Count > 0)
        {
            var item = source.Items[0];
            source.Items.RemoveAt(0);
            destination.Items.Add(item);
        }
    }

    /// <summary>
    /// Selected nodes with the one the menu was opened on first, or only that node when it isn't selected.
    /// </summary>
    private IReadOnlyList<IExplorerNode> GetMenuNodes(IExplorerNode node)
    {
        var nodes = GetSelectedNodes().ToList();
        if (!nodes.Remove(node))
            return [node];

        nodes.Insert(0, node);
        return nodes;
    }

    /// <summary>
    /// Searches the context menu template of a node type, from the most specific type to <see cref="object"/>.
    /// </summary>
    private DataTemplate? FindContextMenuTemplate(Type? nodeType, MenuScope scope)
    {
        if (nodeType == null)
            return null;

        foreach (var type in GetTypeCandidates(nodeType))
        {
            if (TryFindResource(new ContextMenuTemplateKey(type) { Scope = scope }) is DataTemplate template)
                return template;
        }

        return null;
    }

    /// <summary>
    /// Types a menu may be keyed on for a node, most specific first : the type itself, its base classes, then its
    /// interfaces, then <see cref="object"/>.
    /// </summary>
    /// <remarks>
    /// The interfaces are sorted, a derived one before the one it derives from, because
    /// <see cref="Type.GetInterfaces"/> returns them in no specified order : a node implementing both
    /// <see cref="IExplorerDirectory"/> and <see cref="IPhysicalExplorerNode"/> would otherwise get one menu or the
    /// other depending on the run.
    /// </remarks>
    private static IEnumerable<Type> GetTypeCandidates(Type nodeType)
    {
        for (Type? type = nodeType; type != null && type != typeof(object); type = type.BaseType)
            yield return type;

        var interfaces = nodeType.GetInterfaces().ToList();
        interfaces.Sort(
            (left, right) =>
            {
                if (left == right)
                    return 0;
                // The more derived interface comes first.
                if (right.IsAssignableFrom(left))
                    return -1;
                if (left.IsAssignableFrom(right))
                    return 1;
                // Unrelated : ordered by name, so that the choice is at least reproducible.
                return string.CompareOrdinal(left.FullName, right.FullName);
            });

        foreach (var interfaceType in interfaces)
            yield return interfaceType;

        yield return typeof(object);
    }
}
