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
            }

            _listView = GetTemplateChild(PartListView) as ListView;

            if (_listView != null)
            {
                _listView.MouseDoubleClick += OnListViewMouseDoubleClick;
                _listView.ContextMenuOpening += OnListViewContextMenuOpening;
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

        }
        #endregion

        private IExplorerNode? GetNodeAt(DependencyObject? source)
        {
            if (source == null)
                return null;

            return (ItemsControl.ContainerFromElement(_listView, source) as ListViewItem)?.DataContext as IExplorerNode;
        }
    }
}
