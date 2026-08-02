using Joufflu.FileExplorer.Loaders;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Shapes;

namespace Joufflu.FileExplorer.Controls
{
    public interface IExplorerNode
    {
        public string Name { get; }
        public DateTime ModifiedAt { get; }
    }
    public interface IExplorerFolder : IExplorerNode
    {
        public ObservableCollection<IExplorerNode> Children { get; }
    }

    public class ExplorerFile : IExplorerNode
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedAt { get; set; }

        public ExplorerFile(FileInfo fi)
        {
            Path = fi.FullName;
            Name = fi.Name;
            ModifiedAt = fi.LastWriteTime;
        }
    }

    public class ExplorerFolder : IExplorerFolder
    {
        private readonly DirectoryLoader loader;

        public string Path { get; set; }
        public string Name { get; set; } = "";
        public DateTime ModifiedAt { get; set; }

        public ObservableCollection<IExplorerNode> Children { get; private set; } = [];

        public ExplorerFolder(DirectoryInfo di, DirectoryLoader loader)
        {
            Path = di.FullName;
            Name = di.Name;
            ModifiedAt = di.LastWriteTime;
            this.loader = loader;
        }

        /// <summary>
        /// Browse into this folder, it becomes the loader <see cref="IExplorerLoader.Current"/>.
        /// </summary>
        public void Open() => loader.Open(this);
    }

    internal class Explorer
    {
    }
}
