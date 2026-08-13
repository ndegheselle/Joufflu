
using System.ComponentModel;
using System.Windows.Input;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources;

public interface IExplorerSource : INotifyPropertyChanged
{
    IExplorerDirectory? Root { get; }
    IExplorerDirectory? Current { get; }

    /// <summary>
    /// Node being renamed, null while none is. The controls displaying the nodes replace the name of that one with an
    /// editable one, so that a rename is typed where the node is displayed.
    /// </summary>
    IExplorerNode? RenamedNode { get; }

    /// <summary>
    /// Starts the rename of the node given as a parameter, which becomes the <see cref="RenamedNode"/> one. Ended by
    /// <see cref="RenameCommand"/>.
    /// </summary>
    ICommand RenamingCommand { get; }

    /// <summary>
    /// Ends the rename in progress, the parameter being the new name of the <see cref="RenamedNode"/> one. A null, an
    /// empty or an unchanged name only gives it up.
    /// </summary>
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
