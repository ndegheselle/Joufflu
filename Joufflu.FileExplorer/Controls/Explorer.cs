using System.Collections.ObjectModel;
using System.IO;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer.Controls
{
    public interface IExplorerNode
    {
        public string Name { get; }
        public DateTime ModifiedAt { get; }
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

        public PhysicalFile(FileInfo fi)
        {
            Path = fi.FullName;
            Name = fi.Name;
            ModifiedAt = fi.LastWriteTime;
        }
    }

    public class PhysicalDirectory : IExplorerDirectory
    {
        private readonly DirectoryLoader loader;

        public string Path { get; set; }
        public string Name { get; set; } = "";
        public DateTime ModifiedAt { get; set; }

        public ObservableCollection<IExplorerNode> Children { get; private set; } = [];

        public PhysicalDirectory(DirectoryInfo di, DirectoryLoader loader)
        {
            Path = di.FullName;
            Name = di.Name;
            ModifiedAt = di.LastWriteTime;
            this.loader = loader;
        }
    }

    internal class Explorer
    {
    }
}
