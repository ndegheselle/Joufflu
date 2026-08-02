using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Shows the nodes of a folder as a detailed list. Double clicking a folder opens it.
    /// </summary>
    [TemplatePart(Name = PART_ListView, Type = typeof(ListView))]
    public class ListExplorer : Control
    {
        private const string PART_ListView = "PART_ListView";

        private ListView? _listView;

        static ListExplorer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ListExplorer),
                new FrameworkPropertyMetadata(typeof(ListExplorer)));
        }

        #region Dependency Properties
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ListExplorer),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ListExplorer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        /// <summary>Nodes displayed by the list, usually the children of the current folder.</summary>
        public IEnumerable? ItemsSource
        {
            get { return (IEnumerable?)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object? SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // OnApplyTemplate can run several times (e.g. restyling), unsubscribe from the previous part
            if (_listView != null)
                _listView.MouseDoubleClick -= OnListViewMouseDoubleClick;

            _listView = GetTemplateChild(PART_ListView) as ListView;
            if (_listView != null)
                _listView.MouseDoubleClick += OnListViewMouseDoubleClick;
        }

        private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (ItemsControl.ContainerFromElement((ItemsControl)sender, (DependencyObject)e.OriginalSource)
                is not ListViewItem lvi) return;

            if (lvi.DataContext is not ExplorerFolder folder)
                return;

            folder.Open();
        }
    }
}
