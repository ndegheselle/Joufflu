using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Shows the loaded folders and files as a hierarchy in a <see cref="TreeView"/>, opening a folder on double
    /// click.
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
            // Expanded bubbles, so a single handler on the host covers every level.
            ItemsHost?.RemoveHandler(TreeViewItem.ExpandedEvent, (RoutedEventHandler)OnItemExpanded);

            base.OnApplyTemplate();

            ItemsHost?.AddHandler(TreeViewItem.ExpandedEvent, (RoutedEventHandler)OnItemExpanded);
        }

        /// <summary>
        /// Loads the children of an expanded folder. The loader goes one level deeper than what it is asked for, so
        /// expanding a folder is what gives its own sub folders their expander.
        /// </summary>
        private void OnItemExpanded(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is IExplorerDirectory directory)
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
