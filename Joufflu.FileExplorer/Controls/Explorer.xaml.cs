using System.Windows;
using Joufflu.FileExplorer.Controls.Base;

namespace Joufflu.FileExplorer.Controls
{
    /// <summary>
    /// Complete explorer of a loader : an <see cref="ExplorerControlBar"/> on top, an <see cref="ExplorerTree"/> of
    /// the loaded hierarchy next to an <see cref="ExplorerList"/> of the opened directory, and a status bar counting
    /// its nodes and the selected ones.
    /// </summary>
    /// <remarks>
    /// Every part shares the <see cref="ExplorerControl.Loader"/> of the explorer, which is what keeps them in sync :
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

        static Explorer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Explorer), new FrameworkPropertyMetadata(typeof(Explorer)));
        }
    }
}
