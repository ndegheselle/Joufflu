using System.Collections.ObjectModel;
using System.IO;
using Joufflu.FileExplorer.Nodes;

namespace Joufflu.FileExplorer.FileSystem
{
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
