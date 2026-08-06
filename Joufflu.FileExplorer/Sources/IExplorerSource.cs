using Joufflu.FileExplorer.Data;

namespace Joufflu.FileExplorer.Sources;

/// <summary>
/// Where the nodes of an explorer come from, and which mutations they accept. This is the only thing a consumer has to
/// write to display something else than the file system : an archive, a server, a database, nodes built in memory.
/// </summary>
/// <remarks>
/// Only <see cref="CreateRoot"/> and <see cref="GetChildrenAsync"/> have to be written. Everything below them is
/// opt-in : a member left alone disables the menu items and the drops that would have used it, so a read only source
/// needs no code at all to refuse a rename.
/// <para>
/// A source must be able to read the children of any node it has ever produced, including one that a reload has since
/// replaced : the history of an <see cref="ExplorerSession"/> holds node instances. Keying the nodes on their path or
/// their URL is enough.
/// </para>
/// </remarks>
public interface IExplorerSource
{
    /// <summary>
    /// Root node of the source, a null one leaving the explorer empty. Not asynchronous : building the node is not
    /// I/O, even for a remote source whose root is a bare "/" ; its content is read by
    /// <see cref="GetChildrenAsync"/> like any other directory's.
    /// </summary>
    IExplorerDirectory? CreateRoot();

    /// <summary>
    /// Reads the children of a directory.
    /// </summary>
    /// <remarks>
    /// Returns them instead of filling <see cref="IExplorerDirectory.Children"/> : that collection is bound to the
    /// controls, so only the session may touch it and only on the dispatcher thread. This method is therefore free to
    /// run anywhere.
    /// </remarks>
    Task<IEnumerable<IExplorerNode>> GetChildrenAsync(
        IExplorerDirectory directory,
        CancellationToken cancellationToken);

    /// <summary>Whether a node of this source can be renamed.</summary>
    bool CanRename(IExplorerNode node) => false;

    Task RenameAsync(IExplorerNode node, string newName, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <summary>Whether a set of nodes of this source can be deleted, all of them or none.</summary>
    bool CanDelete(IReadOnlyList<IExplorerNode> nodes) => false;

    /// <param name="permanent">
    /// False when the user only asked for a delete : a file system source then sends the nodes to the recycle bin.
    /// </param>
    Task DeleteAsync(IReadOnlyList<IExplorerNode> nodes, bool permanent, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    bool CanCreateDirectory(IExplorerDirectory parent) => false;

    Task CreateDirectoryAsync(IExplorerDirectory parent, string name, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <summary>
    /// Name a new directory is created under before the user renames it. Override it to avoid a name already taken,
    /// the way the file explorer numbers "New folder (2)".
    /// </summary>
    string GetNewDirectoryName(IExplorerDirectory parent) => "New folder";

    /// <summary>
    /// Whether a drop or a paste into <paramref name="target"/> is accepted.
    /// </summary>
    /// <remarks>
    /// Called while a drag hovers the explorer, to decide the cursor it shows : it must answer immediately and must
    /// not do any I/O.
    /// </remarks>
    bool CanAccept(ExplorerTransfer transfer, IExplorerDirectory target) => false;

    /// <summary>
    /// Carries the content of a transfer into a directory of this source. The target source decides how, reading
    /// whichever half of the <see cref="ExplorerTransfer"/> it understands, so no negotiation between two sources is
    /// needed.
    /// </summary>
    Task AcceptAsync(ExplorerTransfer transfer, IExplorerDirectory target, CancellationToken cancellationToken)
        => throw new NotSupportedException();
}
