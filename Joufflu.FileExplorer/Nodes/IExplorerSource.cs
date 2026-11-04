
using System.ComponentModel;

namespace Joufflu.FileExplorer.Nodes;

/// <summary>
/// Where the nodes of the explorer come from and what can be done to them. Plain operations : the controls build the
/// commands their menus and shortcuts bind, so a source is only data and the operations on it.
/// </summary>
public interface IExplorerSource : INotifyPropertyChanged
{
    IExplorerDirectory? Root { get; }
    IExplorerDirectory? Current { get; }

    /// <summary>
    /// Loads the root directory and makes it the <see cref="Current"/> one. Nothing is displayed before it is called.
    /// </summary>
    Task Open();

    /// <summary>
    /// Opens a node : a directory is loaded and made the <see cref="Current"/> one, what a file does is up to the
    /// source.
    /// </summary>
    Task Open(IExplorerNode node);

    /// <summary>
    /// Gives a node a new name. An empty or an unchanged name does nothing, the edition having been given up.
    /// </summary>
    void Rename(IExplorerNode node, string name);

    Task Remove(IReadOnlyList<IExplorerNode> nodes);

    /// <summary>
    /// Creates a new directory in <paramref name="parent"/>, <see cref="Current"/> when none is given.
    /// </summary>
    void CreateDirectory(IExplorerDirectory? parent);

    void Copy(IReadOnlyList<IExplorerNode> nodes);

    void Cut(IReadOnlyList<IExplorerNode> nodes);

    /// <summary>
    /// Copies or moves what was copied or cut into <paramref name="target"/>, <see cref="Current"/> when none is given.
    /// </summary>
    Task Paste(IExplorerDirectory? target);

    /// <summary>Whether something was copied or cut, which <see cref="Paste"/> would paste.</summary>
    bool CanPaste();

    /// <summary>
    /// Copies, or moves when <paramref name="isMove"/>, the files and directories at <paramref name="paths"/> into
    /// <paramref name="target"/> : what is dropped on a control, from it or from the Windows explorer.
    /// </summary>
    Task Transfer(IReadOnlyList<string> paths, IExplorerDirectory target, bool isMove);
}

/// <summary>
/// Rename of a node, handed over to <see cref="IExplorerSource.Rename"/> : the node and the name it is given.
/// </summary>
/// <remarks>
/// The node is carried along with the name, the source keeping no state about an edition in progress : which node is
/// being renamed is the business of the control the name is typed in, see
/// <see cref="Controls.Base.ExplorerNodesControl.RenamedNode"/>.
/// </remarks>
public record ExplorerNodeRename(IExplorerNode Node, string Name);
