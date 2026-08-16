using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Joufflu.FileExplorer.Controls.Base;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Complete explorer of a source : an <see cref="ExplorerControlBar"/> on top, an <see cref="ExplorerTree"/> of
    /// the loaded hierarchy next to an <see cref="ExplorerList"/> of the opened directory, and a status bar counting
    /// its nodes and the selected ones.
    /// </summary>
    /// <remarks>
    /// Every part shares the <see cref="ExplorerControl.Source"/> of the explorer, which is what keeps them in sync :
    /// selecting a directory in the tree, double clicking one in the list or using the bar all open it for the others.
    /// </remarks>
    [TemplatePart(Name = PartControlBar, Type = typeof(ExplorerControlBar))]
    [TemplatePart(Name = PartTree, Type = typeof(ExplorerTree))]
    [TemplatePart(Name = PartList, Type = typeof(ExplorerList))]
    public class Explorer : ExplorerControl
    {
        private const string PartControlBar = "PART_ControlBar";
        private const string PartTree = "PART_Tree";
        private const string PartList = "PART_List";

        /// <summary>
        /// Columns handed over to the list of the explorer, see <see cref="ExplorerList.ExtraColumns"/> : they are
        /// displayed after the ones of the list, and their cells are bound to the node of their row.
        /// </summary>
        public ObservableCollection<GridViewColumn> ExtraColumns { get; } = [];

        private ExplorerList? list;

        static Explorer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Explorer), new FrameworkPropertyMetadata(typeof(Explorer)));
        }

        public Explorer()
        {
            ExtraColumns.CollectionChanged += (_, _) => ApplyExtraColumns();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            list = GetTemplateChild(PartList) as ExplorerList;
            ApplyExtraColumns();
        }

        /// <summary>
        /// Hands the <see cref="ExtraColumns"/> over to the list, which puts them back at the end of its own columns.
        /// </summary>
        private void ApplyExtraColumns()
        {
            if (list == null)
                return;

            list.ExtraColumns.Clear();
            foreach (GridViewColumn column in ExtraColumns)
                list.ExtraColumns.Add(column);
        }
    }
}
