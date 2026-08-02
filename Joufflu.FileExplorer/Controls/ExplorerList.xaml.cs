using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Lists the nodes of a folder in a <see cref="ListView"/>, opening a folder on double click.
    /// </summary>
    [TemplatePart(Name = PART_ListView, Type = typeof(ListView))]
    public class ExplorerList : Control
    {
        private const string PART_ListView = "PART_ListView";

        private ListView? _listView;

        static ExplorerList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExplorerList),
                new FrameworkPropertyMetadata(typeof(ExplorerList)));
        }

        #region ItemsSource
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ExplorerList),
            new PropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>Nodes displayed by the list, usually the children of a folder.</summary>
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExplorerList)d).ItemsView = CreateView(e.NewValue as IEnumerable);
        }

        /// <summary>
        /// Wrap the source in a dedicated sorted view, so that two lists bound to the same
        /// collection don't share the singleton DefaultView (and its sorting).
        /// </summary>
        private static ICollectionView? CreateView(IEnumerable? source)
        {
            if (source == null)
                return null;

            ICollectionView view = source as ICollectionView
                ?? (source is IList list
                    ? new ListCollectionView(list)
                    : new CollectionViewSource { Source = source }.View);

            // Only a ListCollectionView can sort through a comparer, any other view is left as is.
            if (view is ListCollectionView sortable)
                sortable.CustomSort = ExplorerNodeComparer.Default;

            return view;
        }
        #endregion

        #region ItemsView
        private static readonly DependencyPropertyKey ItemsViewPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(ItemsView),
            typeof(ICollectionView),
            typeof(ExplorerList),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ItemsViewProperty = ItemsViewPropertyKey.DependencyProperty;

        /// <summary>
        /// View of <see cref="ItemsSource"/> displayed by the templated list, sorted by
        /// <see cref="ExplorerNodeComparer"/> : folders first, then names in natural order.
        /// </summary>
        public ICollectionView? ItemsView
        {
            get => (ICollectionView?)GetValue(ItemsViewProperty);
            private set => SetValue(ItemsViewPropertyKey, value);
        }
        #endregion

        #region SelectedItem
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ExplorerList),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_listView != null)
                _listView.MouseDoubleClick -= OnListMouseDoubleClick;

            _listView = GetTemplateChild(PART_ListView) as ListView;

            if (_listView != null)
                _listView.MouseDoubleClick += OnListMouseDoubleClick;
        }

        private void OnListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || _listView == null) return;

            if (ItemsControl.ContainerFromElement(_listView, (DependencyObject)e.OriginalSource)
                is not ListViewItem lvi) return;

            if (lvi.DataContext is not ExplorerFolder folder)
                return;

            folder.Open();
        }
    }
}
