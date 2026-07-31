
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace Joufflu.FileExplorer.Loaders;

public interface IExplorerNode
{
    public string Name { get; }
}
public interface IExplorerFolder : IExplorerNode
{
    public ObservableCollection<IExplorerNode> Children { get; }
}

/// <summary>
/// Node backed by a file system path.
/// Nodes implementing it are handled by the default templates and commands of the explorer controls.
/// </summary>
public interface IExplorerPathNode : IExplorerNode
{
    public string Path { get; }
}

public interface IExplorerLoader
{
    public IExplorerFolder Root { get; }
    public void Load();
}

public class ExplorerFile : IExplorerPathNode
{
    public string Path { get; set; }
    public string Name { get; set; }

    public ExplorerFile(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(Path);
    }
}

public class ExplorerFolder : IExplorerFolder, IExplorerPathNode
{
    public string Path { get; set; }
    public string Name { get; set; } = "";
    public ObservableCollection<IExplorerNode> Children { get; private set; } = [];

    public ExplorerFolder(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(Path);
    }
}

/// <summary>
/// Load all the childrens of a root directory and keep it up to date
/// </summary>
public class DirectoryLoader : ObservableObject, IExplorerLoader
{
    private readonly string _rootDirectoryPath;
    /// <summary>
    /// How much childrens from the root the loader should load.
    /// </summary>
    private readonly int _depth;

    private IExplorerFolder _root;

    public IExplorerFolder Root { get => _root; private set => SetProperty(ref _root, value); }

    public DirectoryLoader(string rootDirectoryPath, int depth = 2)
    {
        _rootDirectoryPath = rootDirectoryPath;
        _depth = depth;
        _root = new ExplorerFolder(rootDirectoryPath);
    }

    public void Load()
    {
        ExplorerFolder root = new ExplorerFolder(_rootDirectoryPath);
        LoadChildren(root, _depth);
        Root = root;
    }

    private void LoadChildren(ExplorerFolder folder, int depth)
    {
        var dirInfo = new DirectoryInfo(folder.Path);
        foreach (var entry in dirInfo.EnumerateFileSystemInfos())
        {
            if (entry is FileInfo fi)
            {
                folder.Children.Add(new ExplorerFile(fi.FullName));
            }
            else if (entry is DirectoryInfo di)
            {
                ExplorerFolder subFolder = new ExplorerFolder(di.FullName);
                folder.Children.Add(subFolder);
                if (depth > 0)
                    LoadChildren(subFolder, depth - 1);
            }
        }
    }

    // Expose a Items list that is keep up to date
    // Expose a selected property
}

/// <summary>
/// Load all the childrens of a root directory and keep it up to date
/// </summary>
public class DirectoryObservableLoader
{

}
