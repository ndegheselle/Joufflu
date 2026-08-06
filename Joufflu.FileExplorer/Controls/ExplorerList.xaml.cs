using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;

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

        private static readonly DependencyPropertyKey ExtraColumnsPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ExtraColumns),
            typeof(ObservableCollection<GridViewColumn>),
            typeof(ExplorerList),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ExtraColumnsProperty = ExtraColumnsPropertyKey.DependencyProperty;

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

        protected override IEnumerable<IExplorerNode> GetSelectedNodes()
            => (ItemsHost as ListView)?.SelectedItems.OfType<IExplorerNode>() ?? [];

        /// <summary>
        /// Columns appended to the ones of the template, for the extra properties of a node type of its own :
        /// <c>&lt;GridViewColumn Header="Review" DisplayMemberBinding="{Binding Review}" /&gt;</c>. That saves
        /// retemplating the whole list to show one more value.
        /// </summary>
        public ObservableCollection<GridViewColumn> ExtraColumns
            => (ObservableCollection<GridViewColumn>)GetValue(ExtraColumnsProperty);

        /// <summary>
        /// Columns appended to the <see cref="GridView"/> so far, so they can be taken back out.
        /// </summary>
        private readonly List<GridViewColumn> _appendedColumns = [];

        public ExplorerList()
        {
            // Created per instance and not as the default of the property : a mutable collection given as metadata
            // would be the same one for every ExplorerList of the application.
            var extraColumns = new ObservableCollection<GridViewColumn>();
            extraColumns.CollectionChanged += OnExtraColumnsChanged;
            SetValue(ExtraColumnsPropertyKey, extraColumns);

            SetBinding(
                NodesProperty,
                new Binding($"{nameof(Session)}.{nameof(ExplorerSession.Current)}.{nameof(IExplorerDirectory.Children)}")
                {
                    Source = this
                });
        }

        public override void OnApplyTemplate()
        {
            if (ItemsHost is ListView oldList)
                oldList.SelectionChanged -= OnListSelectionChanged;

            base.OnApplyTemplate();

            if (ItemsHost is ListView newList)
                newList.SelectionChanged += OnListSelectionChanged;

            ApplyExtraColumns();
        }

        private void OnExtraColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e) => ApplyExtraColumns();

        /// <summary>
        /// Puts <see cref="ExtraColumns"/> after the columns of the template.
        /// </summary>
        /// <remarks>
        /// <see cref="GridView.Columns"/> is read only but not fixed, so appending to it works. The columns added
        /// before are taken out first, the collection having possibly changed.
        /// </remarks>
        private void ApplyExtraColumns()
        {
            if (ItemsHost is not ListView { View: GridView grid })
                return;

            foreach (var column in _appendedColumns)
                grid.Columns.Remove(column);
            _appendedColumns.Clear();

            foreach (var column in ExtraColumns)
            {
                grid.Columns.Add(column);
                _appendedColumns.Add(column);
            }
        }

        private void OnListSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateSelectedNodes();

        /// <summary>
        /// Brings a node into view, so that its name editor has a container to be placed over.
        /// </summary>
        protected override void ScrollToNode(IExplorerNode node) => (ItemsHost as ListView)?.ScrollIntoView(node);

        private static void OnNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ExplorerList)d).UpdateItemsView(e.NewValue as IList);

        private static void OnSortComparerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ExplorerList)d).ApplySort();

        /// <summary>Re-runs the <see cref="ExplorerNodesControl.VisibleNodes"/> filter on the current view.</summary>
        protected override void OnVisibleNodesChanged() => (ItemsView as ListCollectionView)?.Refresh();

        /// <summary>
        /// Builds the view of the newly opened folder, sorted by <see cref="SortComparer"/> and filtered to
        /// <see cref="ExplorerNodesControl.VisibleNodes"/>.
        /// </summary>
        private void UpdateItemsView(IList? nodes)
        {
            SetValue(
                ItemsViewPropertyKey,
                nodes == null ? null : new ListCollectionView(nodes) { CustomSort = SortComparer, Filter = FilterNode });
        }

        /// <summary>Keeps only the nodes whose kind is in <see cref="ExplorerNodesControl.VisibleNodes"/>.</summary>
        private bool FilterNode(object item) => item is IExplorerNode node && VisibleNodes.Includes(node);

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
