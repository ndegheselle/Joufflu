
using System.ComponentModel;
using System.Windows.Input;
using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources;

public interface IExplorerSource : INotifyPropertyChanged
{
    IExplorerDirectory? Root { get; }
    IExplorerDirectory? Current { get; }

    /// <summary>
    /// Renames a node, the parameter being the <see cref="ExplorerNodeRename"/> to apply. An empty or an unchanged
    /// name does nothing.
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

    Task Transfer(IReadOnlyList<string> paths, IExplorerDirectory target, bool isMove);
}

/// <summary>
/// Rename of a node, handed over to <see cref="IExplorerSource.RenameCommand"/> : the node and the name it is given.
/// </summary>
/// <remarks>
/// The node is carried along with the name, the source keeping no state about an edition in progress : which node is
/// being renamed is the business of the control the name is typed in, see
/// <see cref="Controls.Base.IExplorerUi.RenamedNode"/>.
/// </remarks>
public record ExplorerNodeRename(IExplorerNode Node, string Name);
