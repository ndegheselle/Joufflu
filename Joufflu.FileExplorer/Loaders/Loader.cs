
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Controls;

namespace Joufflu.FileExplorer.Loaders;

public interface IExplorerLoader
{
    public IExplorerDirectory? Root { get; }
    public IExplorerDirectory? Current { get; }

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
/// Load all the childrens of a root directory and keep it up to date
/// </summary>
public class DirectoryLoader : ObservableObject, IExplorerLoader
{
    private readonly string _rootDirectoryPath;
    /// <summary>
    /// How much childrens from the root the loader should load.
    /// </summary>
    private readonly int _depth;

    private IExplorerDirectory? _root;
    public IExplorerDirectory? Root { get => _root; private set => SetProperty(ref _root, value); }
    private IExplorerDirectory? _current;
    public IExplorerDirectory? Current { get => _current; private set => SetProperty(ref _current, value); }

    public ICommand? OpenCommand { get; }
    // TODO : not supported yet, the menu items using them stay disabled
    public ICommand? RenameCommand => null;
    public ICommand? DeleteCommand => null;

    public DirectoryLoader(string rootDirectoryPath, int depth = 1)
    {
        _rootDirectoryPath = rootDirectoryPath;
        _depth = depth;
        OpenCommand = new RelayCommand<IExplorerNode>(
            node => Open((IExplorerDirectory)node!),
            node => node is IExplorerDirectory);
    }

    public void OpenRoot()
    {
        PhysicalDirectory root = new PhysicalDirectory(new DirectoryInfo(_rootDirectoryPath), this);
        LoadPhysicalDirectory(root, _depth);
        Root = root;
        Current = root;
    }

    public void Open(IExplorerDirectory directory)
    {
        if (directory is PhysicalDirectory physicalDirectory)
        {
            LoadPhysicalDirectory(physicalDirectory, _depth);
            Current = directory;
        }
    }

    public void Load(IExplorerDirectory directory)
    {
        if (directory is PhysicalDirectory physicalDirectory)
            LoadPhysicalDirectory(physicalDirectory, _depth);
    }

    private void LoadPhysicalDirectory(PhysicalDirectory directory, int depth)
    {
        directory.Children.Clear();
        var dirInfo = new DirectoryInfo(directory.Path);
        foreach (var entry in dirInfo.EnumerateFileSystemInfos())
        {
            if (entry is FileInfo fi)
            {
                directory.Children.Add(new PhysicalFile(fi));
            }
            else if (entry is DirectoryInfo di)
            {
                PhysicalDirectory subDirectory = new PhysicalDirectory(di, this);
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
