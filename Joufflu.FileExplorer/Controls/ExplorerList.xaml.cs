using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer.Controls
{
    /* TODO :
     * Could be simplified by inheriting a ListView directly
     * Move the sort view creation to the view model ?
     */

    /// <summary>
    /// Lists the nodes of a folder in a <see cref="ListView"/>, opening a folder on double click.
    /// </summary>
    [TemplatePart(Name = PartListView, Type = typeof(ListView))]
    public class ExplorerList : Control
    {
        private const string PartListView = "PART_ListView";

        private ListView? _listView;

        /// <summary>
        /// Single menu instance filled on opening : WPF captures the <see cref="FrameworkElement.ContextMenu"/> value
        /// before raising <see cref="FrameworkElement.ContextMenuOpening"/>, so the instance can't be replaced there.
        /// </summary>
        private readonly ContextMenu _contextMenu = new();

        static ExplorerList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExplorerList),
                new FrameworkPropertyMetadata(typeof(ExplorerList)));
        }


        #region Dependency Property
        public static readonly DependencyProperty LoaderProperty = DependencyProperty.Register(
            nameof(Loader),
            typeof(IExplorerLoader),
            typeof(ExplorerList),
            new PropertyMetadata(null));
        #endregion

        public IExplorerLoader? Loader
        {
            get => (IExplorerLoader?)GetValue(LoaderProperty);
            set => SetValue(LoaderProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_listView != null)
            {
                _listView.MouseDoubleClick -= OnListViewMouseDoubleClick;
                _listView.ContextMenuOpening -= OnListViewContextMenuOpening;
                _listView.ContextMenu = null;
            }

            _listView = GetTemplateChild(PartListView) as ListView;

            if (_listView != null)
            {
                _listView.MouseDoubleClick += OnListViewMouseDoubleClick;
                _listView.ContextMenuOpening += OnListViewContextMenuOpening;
                _listView.ContextMenu = _contextMenu;
                _contextMenu.PlacementTarget = _listView;
                ApplySort();
            }
        }

        /// <summary>
        /// Sorts the displayed nodes : directories first, then by natural name order.
        /// </summary>
        private void ApplySort()
        {
            if (_listView?.ItemsSource == null)
                return;

            if (CollectionViewSource.GetDefaultView(_listView.ItemsSource) is ListCollectionView view)
                view.CustomSort = ExplorerNodeComparer.Default;
        }

        #region UI events
        private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GetNodeAt(e.OriginalSource as DependencyObject) is not IExplorerDirectory directory)
                return;

            Loader?.Open(directory);
            e.Handled = true;
        }

        private void OnListViewContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (_listView == null)
                return;

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
            var nodes = _listView!.SelectedItems.OfType<IExplorerNode>().ToList();
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

        private IExplorerNode? GetNodeAt(DependencyObject? source)
        {
            if (source == null)
                return null;

            return (ItemsControl.ContainerFromElement(_listView, source) as ListViewItem)?.DataContext as IExplorerNode;
        }
    }
}
