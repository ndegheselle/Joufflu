using System.Collections.ObjectModel;

namespace Joufflu.FileExplorer.Nodes
{
    public interface IExplorerNode
    {
        public string Path { get; set; }
        public string Name { get; }
        public DateTime ModifiedAt { get; }

        /// <summary>
        /// Directory containing the node, null for the root of a source. Walked up by the navigation to the parent
        /// folder.
        /// </summary>
        public IExplorerDirectory? Parent { get; }
    }

    public interface IExplorerDirectory : IExplorerNode
    {
        public ObservableCollection<IExplorerNode> Children { get; }
        /// <summary>
        /// The directory and its ancestors, from the root down to it : the path the breadcrumb shows.
        /// </summary>
        public IReadOnlyList<IExplorerDirectory> DirectoryTree { get; }
    }

    public interface IExplorerFile : IExplorerNode
    {
        public long Size { get; }
    }
}
