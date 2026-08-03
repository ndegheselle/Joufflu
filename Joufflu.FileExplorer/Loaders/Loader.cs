
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Controls;

namespace Joufflu.FileExplorer.Loaders;

public interface IExplorerLoader
{
    public IExplorerFolder? Root { get; }
    public IExplorerFolder? Current { get; }

    public void Load();
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

    public IExplorerFolder? Root { get; private set; }
    public IExplorerFolder? Current { get; private set; }

    public DirectoryLoader(string rootDirectoryPath, int depth = 1)
    {
        _rootDirectoryPath = rootDirectoryPath;
        _depth = depth;
    }

    public void Load()
    {
        ExplorerFolder root = new ExplorerFolder(new DirectoryInfo(_rootDirectoryPath), this);
        LoadChildren(root, _depth);
        Root = root;
        Current = root;
    }

    public void Open(ExplorerFolder folder)
    {
        LoadChildren(folder, _depth);
        Current = folder;
    }

    private void LoadChildren(ExplorerFolder folder, int depth)
    {
        var dirInfo = new DirectoryInfo(folder.Path);
        foreach (var entry in dirInfo.EnumerateFileSystemInfos())
        {
            if (entry is FileInfo fi)
            {
                folder.Children.Add(new ExplorerFile(fi));
            }
            else if (entry is DirectoryInfo di)
            {
                ExplorerFolder subFolder = new ExplorerFolder(di, this);
                folder.Children.Add(subFolder);
                if (depth > 0)
                    LoadChildren(subFolder, depth - 1);
            }
        }
    }
}

/// <summary>
/// Load all the childrens of a root directory and keep it up to date
/// </summary>
public class DirectoryObservableLoader
{

}
