using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Behaviour shared by the controls displaying explorer nodes (<see cref="ExplorerList"/>,
    /// <see cref="ExplorerTree"/>) : the loader, opening a directory on double click and the context menu of a node.
    /// A derived control only provides the <see cref="ItemsControl"/> template part displaying the nodes and its
    /// selection.
    /// </summary>
    [TemplatePart(Name = PartItemsHost, Type = typeof(ItemsControl))]
    public abstract class ExplorerNodesControl : Control
    {
        protected const string PartItemsHost = "PART_ItemsHost";

        /// <summary>
        /// Single menu instance filled on opening : WPF captures the <see cref="FrameworkElement.ContextMenu"/> value
        /// before raising <see cref="FrameworkElement.ContextMenuOpening"/>, so the instance can't be replaced there.
        /// </summary>
        private readonly ContextMenu _contextMenu = new();

        #region Dependency Property
        public static readonly DependencyProperty LoaderProperty = DependencyProperty.Register(
            nameof(Loader),
            typeof(IExplorerLoader),
            typeof(ExplorerNodesControl),
            new PropertyMetadata(null));
        #endregion

        public IExplorerLoader? Loader
        {
            get => (IExplorerLoader?)GetValue(LoaderProperty);
            set => SetValue(LoaderProperty, value);
        }

        /// <summary>
        /// Control displaying the nodes, taken from the <see cref="PartItemsHost"/> template part.
        /// </summary>
        protected ItemsControl? ItemsHost { get; private set; }

        /// <summary>
        /// Nodes selected in <see cref="ItemsHost"/>, empty when nothing is selected.
        /// </summary>
        protected abstract IEnumerable<IExplorerNode> SelectedNodes { get; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ItemsHost != null)
            {
                ItemsHost.MouseDoubleClick -= OnItemsHostMouseDoubleClick;
                ItemsHost.ContextMenuOpening -= OnItemsHostContextMenuOpening;
                ItemsHost.ContextMenu = null;
            }

            ItemsHost = GetTemplateChild(PartItemsHost) as ItemsControl;

            if (ItemsHost != null)
            {
                ItemsHost.MouseDoubleClick += OnItemsHostMouseDoubleClick;
                ItemsHost.ContextMenuOpening += OnItemsHostContextMenuOpening;
                ItemsHost.ContextMenu = _contextMenu;
                _contextMenu.PlacementTarget = ItemsHost;
            }
        }

        #region UI events
        private void OnItemsHostMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GetNodeAt(e.OriginalSource as DependencyObject) is not IExplorerDirectory directory)
                return;

            Loader?.Open(directory);
            e.Handled = true;
        }

        private void OnItemsHostContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _contextMenu.Items.Clear();
            _contextMenu.DataContext = null;

            var node = GetNodeAt(e.OriginalSource as DependencyObject);
            if (node == null)
            {
                e.Handled = true;
                return;
            }

            var nodes = GetMenuNodes(node);
            var template = FindContextMenuTemplate(
                node.GetType(),
                nodes.Count > 1 ? MenuScope.Multiple : MenuScope.Single);

            if (template?.LoadContent() is not ContextMenu menu)
            {
                e.Handled = true;
                return;
            }

            MoveItems(menu, _contextMenu);
            _contextMenu.DataContext = new ExplorerMenuContext(Loader, nodes);
        }
        #endregion

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
        private List<IExplorerNode> GetMenuNodes(IExplorerNode node)
        {
            var nodes = SelectedNodes.ToList();
            if (!nodes.Remove(node))
                return [node];

            nodes.Insert(0, node);
            return nodes;
        }

        /// <summary>
        /// Searches the context menu template of a node type, from the most specific type to <see cref="object"/>.
        /// </summary>
        private DataTemplate? FindContextMenuTemplate(Type nodeType, MenuScope scope)
        {
            foreach (var type in GetTypeCandidates(nodeType))
            {
                if (TryFindResource(new ContextMenuTemplateKey(type) { Scope = scope }) is DataTemplate template)
                    return template;
            }

            return null;
        }

        private static IEnumerable<Type> GetTypeCandidates(Type nodeType)
        {
            for (Type? type = nodeType; type != null && type != typeof(object); type = type.BaseType)
                yield return type;

            foreach (var interfaceType in nodeType.GetInterfaces())
                yield return interfaceType;

            yield return typeof(object);
        }

        /// <summary>
        /// Node displayed by the item container of <paramref name="source"/>, whatever its nesting level.
        /// </summary>
        private IExplorerNode? GetNodeAt(DependencyObject? source)
        {
            if (ItemsHost == null || source == null)
                return null;

            return (ItemsControl.ContainerFromElement(ItemsHost, source) as FrameworkElement)?.DataContext
                as IExplorerNode;
        }
    }
}
