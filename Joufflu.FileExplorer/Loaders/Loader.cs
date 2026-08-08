
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Controls;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Loaders;

public interface IExplorerLoader : INotifyPropertyChanged
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

    /// <summary>
    /// Navigation commands of the <see cref="ExplorerControlBar"/>, taking no parameter. Each one is disabled when
    /// the navigation it does isn't possible : at the root for <see cref="OpenParentCommand"/>, at an end of the
    /// history for the two others.
    /// </summary>
    public ICommand OpenParentCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand GoForwardCommand { get; }

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
public abstract class ExplorerLoader : ObservableObject, IExplorerLoader
{
    /// <summary>
    /// Directories opened so far in navigation order, <see cref="_historyIndex"/> pointing at the current one.
    /// </summary>
    private readonly List<IExplorerDirectory> _history = [];
    private int _historyIndex = -1;
    /// <summary>
    /// True while the history is being replayed, so that the navigation it does isn't recorded in it.
    /// </summary>
    private bool _isReplayingHistory;

    private readonly RelayCommand _openParentCommand;
    private readonly RelayCommand _goBackCommand;
    private readonly RelayCommand _goForwardCommand;

    private IExplorerDirectory? _root;
    public IExplorerDirectory? Root { get => _root; private set => SetProperty(ref _root, value); }

    private IExplorerDirectory? _current;
    public IExplorerDirectory? Current { get => _current; private set => SetProperty(ref _current, value); }

    private IReadOnlyList<IExplorerDirectory> _currentPath = [];
    public IReadOnlyList<IExplorerDirectory> CurrentPath
    {
        get => _currentPath;
        private set => SetProperty(ref _currentPath, value);
    }

    public ICommand? OpenCommand { get; }
    // TODO : not supported yet, the menu items using them stay disabled
    public virtual ICommand? RenameCommand => null;
    public virtual ICommand? DeleteCommand => null;

    public ICommand OpenParentCommand => _openParentCommand;
    public ICommand GoBackCommand => _goBackCommand;
    public ICommand GoForwardCommand => _goForwardCommand;

    protected ExplorerLoader()
    {
        OpenCommand = new RelayCommand<IExplorerNode>(
            node => Open((IExplorerDirectory)node!),
            node => node is IExplorerDirectory);
        _openParentCommand = new RelayCommand(() => Open(Current!.Parent!), () => Current?.Parent != null);
        _goBackCommand = new RelayCommand(() => GoTo(_historyIndex - 1), () => _historyIndex > 0);
        _goForwardCommand = new RelayCommand(
            () => GoTo(_historyIndex + 1),
            () => _historyIndex >= 0 && _historyIndex < _history.Count - 1);
    }

    public void OpenRoot()
    {
        _history.Clear();
        _historyIndex = -1;

        Root = CreateRoot();
        if (Root != null)
            Open(Root);
        else
            UpdateNavigation();
    }

    public void Open(IExplorerDirectory directory)
    {
        if (!LoadChildren(directory))
            return;

        Current = directory;
        CurrentPath = BuildPath(directory);
        if (!_isReplayingHistory)
            Record(directory);
        UpdateNavigation();
    }

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
    /// Records a navigation. Navigating after a few <see cref="GoBackCommand"/> drops the directories that were
    /// ahead, the way a browser does.
    /// </summary>
    private void Record(IExplorerDirectory directory)
    {
        // Reopening the current directory is a refresh, not a navigation.
        if (_historyIndex >= 0 && _history[_historyIndex] == directory)
            return;

        _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
        _history.Add(directory);
        _historyIndex = _history.Count - 1;
    }

    /// <summary>
    /// Opens the directory at an index of the history, leaving the history itself untouched.
    /// </summary>
    private void GoTo(int index)
    {
        if (index < 0 || index >= _history.Count)
            return;

        _historyIndex = index;
        _isReplayingHistory = true;
        try
        {
            Open(_history[index]);
        }
        finally
        {
            _isReplayingHistory = false;
        }
    }

    private void UpdateNavigation()
    {
        _openParentCommand.NotifyCanExecuteChanged();
        _goBackCommand.NotifyCanExecuteChanged();
        _goForwardCommand.NotifyCanExecuteChanged();
    }

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
        => new PhysicalDirectory(new DirectoryInfo(_rootDirectoryPath), null);

    protected override bool LoadChildren(IExplorerDirectory directory)
    {
        if (directory is not PhysicalDirectory physicalDirectory)
            return false;

        LoadPhysicalDirectory(physicalDirectory, _depth);
        return true;
    }

    private static void LoadPhysicalDirectory(PhysicalDirectory directory, int depth)
    {
        directory.Children.Clear();
        var dirInfo = new DirectoryInfo(directory.Path);
        foreach (var entry in dirInfo.EnumerateFileSystemInfos())
        {
            if (entry is FileInfo fi)
            {
                directory.Children.Add(new PhysicalFile(fi, directory));
            }
            else if (entry is DirectoryInfo di)
            {
                PhysicalDirectory subDirectory = new PhysicalDirectory(di, directory);
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
