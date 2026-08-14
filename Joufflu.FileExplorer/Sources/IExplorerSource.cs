
using System.ComponentModel;
using System.Windows.Input;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources;

public interface IExplorerSource : INotifyPropertyChanged
{
    IExplorerDirectory? Root { get; }
    IExplorerDirectory? Current { get; }

    ICommand RenameCommand { get; }
    ICommand RemoveCommand { get; }
    ICommand CreateDirectoryCommand { get; }

    ICommand OpenCommand { get; }
    ICommand OpenInExplorerCommand { get; }
    ICommand CopyPathCommand { get; }
    ICommand OpenWithDefaultCommand { get; }

    ICommand CopyCommand { get; }
    ICommand CutCommand { get; }
    ICommand PasteCommand { get; }

    /// <summary>
    /// Open the root element and makes it the <see cref="Current"/> one.
    /// </summary>
    Task Open();

    /// <summary>
    /// Open a node, if it's a directory loads the children of a directory and makes it the <see cref="Current"/> one.
    /// </summary>
    Task Open(IExplorerNode node);
}


public record ExplorerNodeRename(IExplorerNode Node, string Name);