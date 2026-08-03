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

        private static readonly DependencyPropertyDescriptor ItemsSourceDescriptor =
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListView));

        private ListView? _listView;

        /// <summary>
        /// The context menu opening event is only raised if the list view already has a menu, this placeholder is
        /// kept in place so the real menu can be resolved on opening.
        /// </summary>
        private readonly ContextMenu _placeholderContextMenu = new ContextMenu();

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

        public static readonly DependencyProperty FileContextMenuProperty = DependencyProperty.Register(
            nameof(FileContextMenu),
            typeof(ContextMenu),
            typeof(ExplorerList),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DirectoryContextMenuProperty = DependencyProperty.Register(
            nameof(DirectoryContextMenu),
            typeof(ContextMenu),
            typeof(ExplorerList),
            new PropertyMetadata(null));

        public static readonly DependencyProperty SelectionContextMenuProperty = DependencyProperty.Register(
            nameof(SelectionContextMenu),
            typeof(ContextMenu),
            typeof(ExplorerList),
            new PropertyMetadata(null));

        #endregion

        public IExplorerLoader? Loader
        {
            get => (IExplorerLoader?)GetValue(LoaderProperty);
            set => SetValue(LoaderProperty, value);
        }

        /// <summary>
        /// Menu shown for a single <see cref="IExplorerFile"/> with no more specific <see cref="NodeContextMenus"/>.
        /// </summary>
        public ContextMenu? FileContextMenu
        {
            get => (ContextMenu?)GetValue(FileContextMenuProperty);
            set => SetValue(FileContextMenuProperty, value);
        }

        /// <summary>
        /// Menu shown for a single <see cref="IExplorerDirectory"/> with no more specific <see cref="NodeContextMenus"/>.
        /// </summary>
        public ContextMenu? DirectoryContextMenu
        {
            get => (ContextMenu?)GetValue(DirectoryContextMenuProperty);
            set => SetValue(DirectoryContextMenuProperty, value);
        }

        /// <summary>
        /// Menu shown when more than one node is selected, its data context is the selected nodes.
        /// </summary>
        public ContextMenu? SelectionContextMenu
        {
            get => (ContextMenu?)GetValue(SelectionContextMenuProperty);
            set => SetValue(SelectionContextMenuProperty, value);
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_listView != null)
            {
                _listView.MouseDoubleClick -= OnListViewMouseDoubleClick;
                _listView.ContextMenuOpening -= OnListViewContextMenuOpening;
                ItemsSourceDescriptor.RemoveValueChanged(_listView, OnListViewItemsSourceChanged);
            }

            _listView = GetTemplateChild(PartListView) as ListView;

            if (_listView != null)
            {
                _listView.MouseDoubleClick += OnListViewMouseDoubleClick;
                _listView.ContextMenuOpening += OnListViewContextMenuOpening;
                // The items source is replaced every time an other directory is opened, the sort has to be reapplied
                ItemsSourceDescriptor.AddValueChanged(_listView, OnListViewItemsSourceChanged);
                _listView.ContextMenu = _placeholderContextMenu;
                ApplySort();
            }
        }

        private void OnListViewItemsSourceChanged(object? sender, EventArgs e) => ApplySort();

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

        private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GetNodeAt(e.OriginalSource as DependencyObject) is IExplorerDirectory directory)
            {
                Loader?.Open(directory);
                e.Handled = true;
            }
        }

        private void OnListViewContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (_listView == null)
                return;

            IExplorerNode? clicked = GetNodeAt(e.OriginalSource as DependencyObject);
            // Right clicking outside of the current selection moves it, like the windows explorer does
            if (clicked != null && !_listView.SelectedItems.Contains(clicked))
                _listView.SelectedItem = clicked;

            List<IExplorerNode> selection = _listView.SelectedItems.OfType<IExplorerNode>().ToList();
            // No node under the cursor, nothing to act on
            ContextMenu? menu = clicked == null || selection.Count == 0
                ? null
                : selection.Count > 1 ? SelectionContextMenu : ResolveNodeContextMenu(selection[0]);

            if (menu == null)
            {
                // Keep the placeholder so the event is raised again on the next right click
                _listView.ContextMenu = _placeholderContextMenu;
                e.Handled = true;
                return;
            }

            menu.DataContext = new ExplorerMenuContext(Loader, selection);
            _listView.ContextMenu = menu;
        }

        /// <summary>
        /// Finds the <see cref="ExplorerMenuKey"/> resource of the node type, the same way an implicit
        /// <see cref="DataTemplate"/> is resolved : the class hierarchy first then the interfaces,
        /// falling back on the directory / file defaults.
        /// </summary>
        private ContextMenu? ResolveNodeContextMenu(IExplorerNode node)
        {
            Type nodeType = node.GetType();
            for (Type? type = nodeType; type != null && type != typeof(object); type = type.BaseType)
            {
                if (TryFindResource(new ExplorerMenuKey(type)) is ContextMenu menu)
                    return menu;
            }

            foreach (Type interfaceType in GetInterfacesBySpecificity(nodeType))
            {
                if (TryFindResource(new ExplorerMenuKey(interfaceType)) is ContextMenu menu)
                    return menu;
            }

            return node is IExplorerDirectory ? DirectoryContextMenu : FileContextMenu;
        }

        /// <summary>
        /// Interfaces of a type, the most derived ones first so that they take priority over the ones they extend.
        /// </summary>
        private static IEnumerable<Type> GetInterfacesBySpecificity(Type type)
        {
            Type[] interfaces = type.GetInterfaces();
            // An interface extended by an other one is less specific, so it is tested last
            return interfaces.OrderBy(i => interfaces.Count(other => i.IsAssignableFrom(other)));
        }

        private IExplorerNode? GetNodeAt(DependencyObject? source)
        {
            if (source == null)
                return null;

            return (ItemsControl.ContainerFromElement(_listView, source) as ListViewItem)?.DataContext as IExplorerNode;
        }
    }
}
