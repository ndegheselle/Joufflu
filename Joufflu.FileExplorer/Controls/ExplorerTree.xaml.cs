using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Shows the loaded folders and files as a hierarchy in a <see cref="TreeView"/>. Selecting a folder opens it, a
    /// double click only expands or collapses it.
    /// </summary>
    public class ExplorerTree : ExplorerNodesControl
    {
        static ExplorerTree()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExplorerTree),
                new FrameworkPropertyMetadata(typeof(ExplorerTree)));
        }

        protected override IEnumerable<IExplorerNode> SelectedNodes
        {
            get
            {
                // A TreeView only ever has one selected item.
                if ((ItemsHost as TreeView)?.SelectedItem is IExplorerNode node)
                    yield return node;
            }
        }

        public override void OnApplyTemplate()
        {
            // EventSetter cannot target a handler outside the code-behind of the XAML that declares the style, so
            // the Expanded event of every TreeViewItem is instead caught here as it bubbles up through the TreeView.
            if (ItemsHost is TreeView oldTree)
            {
                oldTree.RemoveHandler(TreeViewItem.ExpandedEvent, (RoutedEventHandler)OnTreeExpanded);
                oldTree.SelectedItemChanged -= OnTreeSelectedItemChanged;
            }

            base.OnApplyTemplate();

            if (ItemsHost is TreeView newTree)
            {
                newTree.AddHandler(TreeViewItem.ExpandedEvent, (RoutedEventHandler)OnTreeExpanded);
                newTree.SelectedItemChanged += OnTreeSelectedItemChanged;
            }
        }

        /// <summary>
        /// Expands or collapses a double clicked folder. Opening it is the job of the selection, which a click already
        /// did.
        /// </summary>
        protected override bool OnNodeDoubleClick(IExplorerNode node, FrameworkElement container)
        {
            if (node is not IExplorerDirectory || container is not TreeViewItem item)
                return false;

            item.IsExpanded = true;
            return true;
        }

        private void OnTreeExpanded(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem)e.OriginalSource;
            tvi.IsSelected = true;
            e.Handled = true;
        }

        private void OnTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is IExplorerDirectory directory)
                Loader?.Open(directory);
        }
    }

    /// <summary>
    /// Children of a directory node, in a sorted view of their own. Used for every level of an
    /// <see cref="ExplorerTree"/>, files converting to null so that they stay leaves.
    /// </summary>
    /// <remarks>
    /// Each binding builds its own view, so a folder displayed at several places (a tree level and an
    /// <see cref="ExplorerList"/>) does not share its sort, selection and current item.
    /// </remarks>
    public class ExplorerChildrenConverter : IValueConverter
    {
        public static readonly ExplorerChildrenConverter Default = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not IExplorerDirectory directory)
                return null;

            return new ListCollectionView(directory.Children) { CustomSort = ExplorerNodeComparer.Default };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}