
using System.ComponentModel;
using System.Windows.Input;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources;

public interface IExplorerSource : INotifyPropertyChanged
{
    public IExplorerDirectory? Root { get; }
    public IExplorerDirectory? Current { get; }

    public ICommand RenameCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand CreateDirectoryCommand { get; }

    public ICommand OpenCommand { get; }
    public ICommand OpenInExplorerCommand { get; }
    public ICommand OpenWithDefaultCommand { get; }

    public ICommand CopyCommand { get; }
    public ICommand CutCommand { get; }
    public ICommand PasteCommand { get; }

    /// <summary>
    /// Open the root element and makes it the <see cref="Current"/> one.
    /// </summary>
    public Task Open();

    /// <summary>
    /// Open a node, if it's a directory loads the children of a directory and makes it the <see cref="Current"/> one.
    /// </summary>
    public Task Open(IExplorerNode node);
}
