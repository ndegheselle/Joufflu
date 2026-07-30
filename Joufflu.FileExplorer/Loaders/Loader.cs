
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls.Primitives;

namespace Joufflu.FileExplorer.Loaders;

public interface IExplorerNode
{
    public string Name { get; }
}
public interface IExplorerFolder : IExplorerNode
{
    public ObservableCollection<IExplorerNode> Children { get; }
}

public interface IExplorerLoader
{
    public IExplorerFolder Root { get; }
    public void Load();
}

public class ExplorerFile : IExplorerNode
{
    public string Path { get; set; }
    public string Name { get; set; }

    public ExplorerFile(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(Path);
    }
}

public class ExplorerFolder : IExplorerFolder
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

    public IExplorerFolder Root { get; private set; }

    public DirectoryLoader(string rootDirectoryPath, int depth = 2)
    {
        _rootDirectoryPath = rootDirectoryPath;
        _depth = depth;
    }

    public void Load()
    {
        var dirInfo = new DirectoryInfo(_rootDirectoryPath);
        foreach (var entry in dirInfo.EnumerateFileSystemInfos())
        {
            if (entry is FileInfo fi)

                Console.WriteLine($"File: {fi.FullName}, {fi.Length} bytes");
            else if (entry is DirectoryInfo di)
                Console.WriteLine($"Dir: {di.FullName}");
        }
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
                    LoadChildren();
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
