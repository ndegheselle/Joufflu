
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
    /// Open the root element and makes it the <see cref="Current"/> one.
    /// </summary>
    Task Open();

    /// <summary>
    /// Open a node, if it's a directory loads the children of a directory and makes it the <see cref="Current"/> one.
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
