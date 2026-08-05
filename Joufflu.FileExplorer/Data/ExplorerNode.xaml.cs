using System.Collections.ObjectModel;
using System.IO;
using Joufflu.FileExplorer.Controls;

namespace Joufflu.FileExplorer.Data
{
    public interface IExplorerNode
    {
        public string Name { get; }
        public DateTime ModifiedAt { get; }

        /// <summary>
        /// Directory containing the node, null for the root of a loader. Walked up by the navigation to the parent
        /// folder and by the breadcrumb of the <see cref="ExplorerControlBar"/>.
        /// </summary>
        public IExplorerDirectory? Parent { get; }
    }

    public interface IExplorerDirectory : IExplorerNode
    {
        public ObservableCollection<IExplorerNode> Children { get; }
    }

    public interface IExplorerFile : IExplorerNode
    { }

    public class PhysicalFile : IExplorerFile
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedAt { get; set; }
        public IExplorerDirectory? Parent { get; }

        public PhysicalFile(FileInfo fi, IExplorerDirectory? parent)
        {
            Path = fi.FullName;
            Name = fi.Name;
            ModifiedAt = fi.LastWriteTime;
            Parent = parent;
        }
    }

    public class PhysicalDirectory : IExplorerDirectory
    {
        public string Path { get; set; }
        public string Name { get; set; } = "";
        public DateTime ModifiedAt { get; set; }
        public IExplorerDirectory? Parent { get; }

        public ObservableCollection<IExplorerNode> Children { get; private set; } = [];

        public PhysicalDirectory(DirectoryInfo di, IExplorerDirectory? parent)
        {
            Path = di.FullName;
            Name = di.Name;
            ModifiedAt = di.LastWriteTime;
            Parent = parent;
        }
    }
}
