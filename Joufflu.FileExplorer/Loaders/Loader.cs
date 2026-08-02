
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Controls;
using System.IO;

namespace Joufflu.FileExplorer.Loaders;

public interface IExplorerLoader
{
    public IExplorerFolder? Root { get; }
    /// <summary>
    /// Folder currently browsed, the one an explorer displays the childrens of.
    /// </summary>
    public IExplorerFolder? Current { get; }

    public IExplorerFolder Load();
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

    private IExplorerFolder? _root;
    private IExplorerFolder? _current;

    public IExplorerFolder? Root { get => _root; private set => SetProperty(ref _root, value); }
    /// <inheritdoc/>
    public IExplorerFolder? Current { get => _current; private set => SetProperty(ref _current, value); }

    public DirectoryLoader(string rootDirectoryPath, int depth = 1)
    {
        _rootDirectoryPath = rootDirectoryPath;
        _depth = depth;
    }

    public IExplorerFolder Load()
    {
        ExplorerFolder root = new ExplorerFolder(new DirectoryInfo(_rootDirectoryPath), this);
        LoadChildren(root, _depth);
        Root = root;
        Current = root;
        return root;
    }

    /// <summary>
    /// Reload the childrens of a folder and make it the new <see cref="Current"/>.
    /// </summary>
    public void Open(ExplorerFolder folder)
    {
        LoadChildren(folder, _depth);
        Current = folder;
    }

    private void LoadChildren(ExplorerFolder folder, int depth)
    {
        folder.Children.Clear();
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
