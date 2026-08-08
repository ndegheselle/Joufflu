
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Controls;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources;

public interface IExplorerSource : INotifyPropertyChanged
{
    public IExplorerDirectory? Root { get; }
    public IExplorerDirectory? Current { get; }

    /// <summary>
    /// Directories leading to <see cref="Current"/>, from its topmost parent to itself, displayed as a breadcrumb by
    /// the <see cref="ExplorerControlBar"/>.
    /// </summary>
    public IReadOnlyList<IExplorerDirectory> CurrentPath { get; }

    /// <summary>
    /// Commands used by the context menus of the explorer, a null command disables its menu items.
    /// The parameter is the targeted node, or the selected nodes for the ones acting on a selection.
    /// </summary>
    public ICommand? OpenCommand { get; }
    public ICommand? RenameCommand { get; }
    public ICommand? DeleteCommand { get; }

    public void OpenRoot();

    /// <summary>
    /// Loads the children of a directory and makes it the <see cref="Current"/> one.
    /// </summary>
    public void Open(IExplorerDirectory directory);

    /// <summary>
    /// Loads the children of a directory without navigating to it, for a control showing a hierarchy it only needs
    /// the content of.
    /// </summary>
    public void Load(IExplorerDirectory directory);
}

/// <summary>
/// Navigation shared by the loaders : the opened directory, the path leading to it and the history of the visited
/// ones. A derived loader only provides its root and how the children of a directory are read.
/// </summary>
public abstract partial class ExplorerLoader : ObservableObject, IExplorerSource
{
    private readonly RelayCommand _openParentCommand;

    [ObservableProperty]
    private IExplorerDirectory? root;
    [ObservableProperty]
    private IExplorerDirectory? current;

    [ObservableProperty]
    private IReadOnlyList<IExplorerDirectory> currentPath = [];

    public ICommand OpenParentCommand => _openParentCommand;

    protected ExplorerLoader()
    {
        _openParentCommand = new RelayCommand(() => Open(Current!.Parent!), () => Current?.Parent != null);
    }

    public void OpenRoot()
    {
        Root = CreateRoot();
        if (Root != null)
            Open(Root);
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    public void Open(IExplorerDirectory directory)
    {
        if (!LoadChildren(directory))
            return;

        Current = directory;
        CurrentPath = BuildPath(directory);
    }
    public bool CanOpen(IExplorerNode node) => node is IExplorerDirectory directory;

    public void Load(IExplorerDirectory directory) => LoadChildren(directory);

    /// <summary>
    /// Builds the root directory, whose children <see cref="OpenRoot"/> then loads. A null root leaves the explorer
    /// empty.
    /// </summary>
    protected abstract IExplorerDirectory? CreateRoot();

    /// <summary>
    /// Loads the children of a directory and returns whether it could : a loader only knows the nodes it created
    /// itself, and leaves a foreign one alone instead of opening it.
    /// </summary>
    protected abstract bool LoadChildren(IExplorerDirectory directory);

    /// <summary>
    /// Directories from the topmost parent of a directory down to itself, itself last.
    /// </summary>
    private static IReadOnlyList<IExplorerDirectory> BuildPath(IExplorerDirectory directory)
    {
        List<IExplorerDirectory> path = [];
        for (IExplorerDirectory? node = directory; node != null; node = node.Parent)
            path.Insert(0, node);
        return path;
    }
}

/// <summary>
/// Load all the childrens of a root directory and keep it up to date
/// </summary>
public class DirectoryLoader : ExplorerLoader
{
    private readonly string _rootDirectoryPath;
    /// <summary>
    /// How much childrens from the root the loader should load.
    /// </summary>
    private readonly int _depth;

    public DirectoryLoader(string rootDirectoryPath, int depth = 1)
    {
        _rootDirectoryPath = rootDirectoryPath;
        _depth = depth;
    }

    protected override IExplorerDirectory CreateRoot()
        => new FileSystemDirectory(new DirectoryInfo(_rootDirectoryPath), null);

    protected override bool LoadChildren(IExplorerDirectory directory)
    {
        if (directory is not FileSystemDirectory physicalDirectory)
            return false;

        LoadPhysicalDirectory(physicalDirectory, _depth);
        return true;
    }

    private static void LoadPhysicalDirectory(FileSystemDirectory directory, int depth)
    {
        directory.Children.Clear();
        var dirInfo = new DirectoryInfo(directory.Path);
        foreach (var entry in dirInfo.EnumerateFileSystemInfos())
        {
            if (entry is FileInfo fi)
            {
                directory.Children.Add(new FileSystemFile(fi, directory));
            }
            else if (entry is DirectoryInfo di)
            {
                FileSystemDirectory subDirectory = new FileSystemDirectory(di, directory);
                directory.Children.Add(subDirectory);
                if (depth > 0)
                    LoadPhysicalDirectory(subDirectory, depth - 1);
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
