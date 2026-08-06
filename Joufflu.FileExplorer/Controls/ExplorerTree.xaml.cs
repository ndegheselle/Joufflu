using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Joufflu.FileExplorer.Controls.Base;
using Joufflu.FileExplorer.Data;

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

            // A folder tree only shows folders by default ; a consumer can still opt back into files.
            VisibleNodesProperty.OverrideMetadata(
                typeof(ExplorerTree),
                new FrameworkPropertyMetadata(ExplorerNodeKinds.Directories));
        }

        protected override IEnumerable<IExplorerNode> GetSelectedNodes()
        {
            // A TreeView only ever has one selected item.
            if ((ItemsHost as TreeView)?.SelectedItem is IExplorerNode node)
                yield return node;
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

            // A level still empty has not been read yet, which happens when the source prefetches nothing. A source
            // that does prefetch never gets here : an empty folder shows no expander, so it cannot be expanded.
            if (tvi.DataContext is IExplorerDirectory { Children.Count: 0 } directory)
                _ = Session?.LoadAsync(directory);

            tvi.IsSelected = true;
            e.Handled = true;
        }

        private void OnTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateSelectedNodes();

            if (e.NewValue is IExplorerDirectory directory)
                _ = Session?.OpenAsync(directory);
        }
    }

    /// <summary>
    /// Children of a directory node, in a sorted view of their own. Used for every level of an
    /// <see cref="ExplorerTree"/>, files converting to null so that they stay leaves. As an
    /// <see cref="IMultiValueConverter"/> it also takes an <see cref="ExplorerNodeKinds"/> filter so a level can
    /// show only some of its children (a folder tree keeping only its sub folders).
    /// </summary>
    /// <remarks>
    /// Each binding builds its own view, so a folder displayed at several places (a tree level and an
    /// <see cref="ExplorerList"/>) does not share its sort, selection and current item.
    /// </remarks>
    public class ExplorerChildrenConverter : IValueConverter, IMultiValueConverter
    {
        public static readonly ExplorerChildrenConverter Default = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => BuildView(value as IExplorerDirectory, ExplorerNodeKinds.All);

        /// <summary>Values : the node, then the <see cref="ExplorerNodeKinds"/> to keep.</summary>
        public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            var directory = values.Length > 0 ? values[0] as IExplorerDirectory : null;
            var filter = values.Length > 1 && values[1] is ExplorerNodeKinds kinds ? kinds : ExplorerNodeKinds.All;
            return BuildView(directory, filter);
        }

        private static ListCollectionView? BuildView(IExplorerDirectory? directory, ExplorerNodeKinds filter)
        {
            if (directory == null)
                return null;

            var view = new ListCollectionView(directory.Children) { CustomSort = ExplorerNodeComparer.Default };
            if (filter != ExplorerNodeKinds.All)
                view.Filter = item => item is IExplorerNode node && filter.Includes(node);
            return view;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}