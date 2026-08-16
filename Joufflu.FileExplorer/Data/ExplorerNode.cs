using System.Collections.ObjectModel;
using System.IO;
using Joufflu.FileExplorer.Controls;

namespace Joufflu.FileExplorer.Data
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
        /// All the parent of the current directory (including this one)
        /// </summary>
        public IReadOnlyList<IExplorerDirectory> DirectoryTree { get; }
    }

    public interface IExplorerFile : IExplorerNode
    {
        public long Size { get; }
    }

    public class FileSystemFile : IExplorerFile
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedAt { get; set; }
        public IExplorerDirectory? Parent { get; }
        public long Size { get; set; }

        public FileSystemFile(FileInfo fi, IExplorerDirectory? parent)
        {
            Path = fi.FullName;
            Name = fi.Name;
            ModifiedAt = fi.LastWriteTime;
            Size = fi.Length;
            Parent = parent;
        }
    }

    public class FileSystemDirectory : IExplorerDirectory
    {
        public string Path { get; set; }
        public string Name { get; set; } = "";
        public DateTime ModifiedAt { get; set; }

        public IExplorerDirectory? Parent { get; }
        // The root ends the walk up : it is the first directory of the path, and the breadcrumb starts on it.
        public IReadOnlyList<IExplorerDirectory> DirectoryTree => Parent == null ? [this] : [..Parent.DirectoryTree, this];


        public ObservableCollection<IExplorerNode> Children { get; private set; } = [];

        public FileSystemDirectory(DirectoryInfo di, IExplorerDirectory? parent)
        {
            Path = di.FullName;
            Name = di.Name;
            ModifiedAt = di.LastWriteTime;
            Parent = parent;
        }
    }
}
