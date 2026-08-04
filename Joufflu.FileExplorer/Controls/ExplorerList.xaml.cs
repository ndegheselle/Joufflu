using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Lists the nodes of the opened folder in a <see cref="ListView"/>, opening a folder on double click.
    /// </summary>
    public class ExplorerList : ExplorerNodesControl
    {
        static ExplorerList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExplorerList),
                new FrameworkPropertyMetadata(typeof(ExplorerList)));
        }

        #region Dependency Property
        public static readonly DependencyProperty SortComparerProperty = DependencyProperty.Register(
            nameof(SortComparer),
            typeof(IComparer),
            typeof(ExplorerList),
            new PropertyMetadata(ExplorerNodeComparer.Default, OnSortComparerChanged));

        private static readonly DependencyPropertyKey ItemsViewPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ItemsView),
            typeof(ICollectionView),
            typeof(ExplorerList),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ItemsViewProperty = ItemsViewPropertyKey.DependencyProperty;

        /// <summary>
        /// Children of the opened folder, bound in the constructor so <see cref="ItemsView"/> follows the navigation.
        /// </summary>
        private static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            "Nodes",
            typeof(IList),
            typeof(ExplorerList),
            new PropertyMetadata(null, OnNodesChanged));
        #endregion

        /// <summary>
        /// Comparer applied to <see cref="ItemsView"/>, <see cref="ExplorerNodeComparer.Default"/> by default.
        /// Replacing it re-sorts in place without rebuilding the view, which is the seam for a sort driven by a
        /// column header and a direction. A null comparer leaves the nodes in their loading order.
        /// </summary>
        public IComparer? SortComparer
        {
            get => (IComparer?)GetValue(SortComparerProperty);
            set => SetValue(SortComparerProperty, value);
        }

        /// <summary>
        /// Sorted view of the opened folder, owned by this control. The same
        /// <see cref="IExplorerDirectory.Children"/> can be displayed by several controls at once (the tree and the
        /// list together), so the shared default view is not used : its sort, selection and current item would be
        /// shared too.
        /// </summary>
        public ICollectionView? ItemsView => (ICollectionView?)GetValue(ItemsViewProperty);

        protected override IEnumerable<IExplorerNode> SelectedNodes
            => (ItemsHost as ListView)?.SelectedItems.OfType<IExplorerNode>() ?? [];

        public ExplorerList()
        {
            SetBinding(
                NodesProperty,
                new Binding($"{nameof(Loader)}.{nameof(IExplorerLoader.Current)}.{nameof(IExplorerDirectory.Children)}")
                {
                    Source = this
                });
        }

        private static void OnNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ExplorerList)d).UpdateItemsView(e.NewValue as IList);

        private static void OnSortComparerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ExplorerList)d).ApplySort();

        /// <summary>
        /// Builds the view of the newly opened folder, already sorted by <see cref="SortComparer"/>.
        /// </summary>
        private void UpdateItemsView(IList? nodes)
        {
            SetValue(
                ItemsViewPropertyKey,
                nodes == null ? null : new ListCollectionView(nodes) { CustomSort = SortComparer });
        }

        /// <summary>
        /// Re-sorts the displayed nodes, by default directories first then by natural name order.
        /// </summary>
        private void ApplySort()
        {
            if (ItemsView is ListCollectionView view)
                view.CustomSort = SortComparer;
        }
    }
}
