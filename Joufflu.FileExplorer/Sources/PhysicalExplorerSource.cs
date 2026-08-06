using System.IO;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Helpers;

namespace Joufflu.FileExplorer.Sources;

/// <summary>
/// The files and folders of a directory of this machine. Supports every operation, through the shell, so it also
/// serves as the reference of what a complete source looks like.
/// </summary>
/// <remarks>
/// Deriving from it and overriding <see cref="CreateFile"/> or <see cref="CreateDirectory"/> is how a consumer
/// attaches data of its own to the nodes : the source builds them, so it decides their type.
/// </remarks>
public class PhysicalExplorerSource : IExplorerSource
{
    public PhysicalExplorerSource(string rootPath)
    {
        RootPath = rootPath;
    }

    public string RootPath { get; }

    /// <summary>
    /// Levels of sub directories filled in by each read, so that a tree shows an expander without a second round
    /// trip. One by default ; zero for a big hierarchy, the tree then reading a level when it is expanded.
    /// </summary>
    public int PrefetchDepth { get; set; } = 1;

    public virtual IExplorerDirectory? CreateRoot()
        => CreateDirectory(new DirectoryInfo(RootPath), null);

    /// <summary>Builds the node of a file. Override it to attach data of your own.</summary>
    protected virtual IExplorerNode CreateFile(FileInfo info, IExplorerDirectory parent)
        => new PhysicalFile(info, parent);

    /// <summary>Builds the node of a folder. Override it to attach data of your own.</summary>
    protected virtual IExplorerDirectory CreateDirectory(DirectoryInfo info, IExplorerDirectory? parent)
        => new PhysicalDirectory(info, parent);

    public virtual Task<IEnumerable<IExplorerNode>> GetChildrenAsync(
        IExplorerDirectory directory,
        CancellationToken cancellationToken)
    {
        // A source only knows the nodes it built itself, and leaves a foreign one empty rather than guessing.
        if (directory is not IPhysicalExplorerNode physical)
            return Task.FromResult<IEnumerable<IExplorerNode>>([]);

        string path = physical.FileSystemPath;
        return Task.Run<IEnumerable<IExplorerNode>>(
            () => Read(directory, path, PrefetchDepth, isRequested: true, cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Reads a directory, and fills the children of the sub directories it built while <paramref name="depth"/>
    /// allows it.
    /// </summary>
    /// <param name="isRequested">
    /// True for the directory the explorer asked for, false for one only prefetched below it : a prefetched
    /// directory we can't read is left empty, where the requested one reports its failure.
    /// </param>
    private List<IExplorerNode> Read(
        IExplorerDirectory directory,
        string path,
        int depth,
        bool isRequested,
        CancellationToken cancellationToken)
    {
        List<IExplorerNode> nodes = [];

        // IgnoreInaccessible so that one unreadable entry doesn't lose the whole directory ; the try/catch is for
        // the directory itself being unreadable, or being deleted while it is read.
        var options = new EnumerationOptions { IgnoreInaccessible = true };

        try
        {
            foreach (var entry in new DirectoryInfo(path).EnumerateFileSystemInfos("*", options))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (entry is FileInfo file)
                {
                    nodes.Add(CreateFile(file, directory));
                }
                else if (entry is DirectoryInfo subDirectoryInfo)
                {
                    var subDirectory = CreateDirectory(subDirectoryInfo, directory);
                    nodes.Add(subDirectory);

                    if (depth > 0)
                    {
                        // Filled here and not by the session : these are the children of a node the session isn't
                        // navigating to, and nothing is bound to them yet.
                        foreach (var child in Read(
                            subDirectory,
                            subDirectoryInfo.FullName,
                            depth - 1,
                            isRequested: false,
                            cancellationToken))
                            subDirectory.Children.Add(child);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception) when (!isRequested)
        { }

        return nodes;
    }

    public virtual bool CanRename(IExplorerNode node) => node is IPhysicalExplorerNode;

    public virtual Task RenameAsync(IExplorerNode node, string newName, CancellationToken cancellationToken)
    {
        if (node is not IPhysicalExplorerNode physical)
            throw new NotSupportedException($"'{node.Name}' is not a node of the file system.");
        if (string.IsNullOrWhiteSpace(newName) || newName.Intersect(Path.GetInvalidFileNameChars()).Any())
            throw new ArgumentException($"'{newName}' is not a valid name.", nameof(newName));

        string path = physical.FileSystemPath;
        return Task.Run(() => ShellFileOperations.Rename(path, newName), cancellationToken);
    }

    public virtual bool CanDelete(IReadOnlyList<IExplorerNode> nodes)
        => nodes.Count > 0 && nodes.All(node => node is IPhysicalExplorerNode);

    public virtual Task DeleteAsync(
        IReadOnlyList<IExplorerNode> nodes,
        bool permanent,
        CancellationToken cancellationToken)
    {
        string[] paths = nodes.OfType<IPhysicalExplorerNode>().Select(node => node.FileSystemPath).ToArray();
        return Task.Run(() => ShellFileOperations.Delete(paths, permanent), cancellationToken);
    }

    public virtual bool CanCreateDirectory(IExplorerDirectory parent) => parent is IPhysicalExplorerNode;

    public virtual Task CreateDirectoryAsync(
        IExplorerDirectory parent,
        string name,
        CancellationToken cancellationToken)
    {
        if (parent is not IPhysicalExplorerNode physical)
            throw new NotSupportedException($"'{parent.Name}' is not a folder of the file system.");

        string path = Path.Combine(physical.FileSystemPath, name);
        return Task.Run(() => Directory.CreateDirectory(path), cancellationToken);
    }

    public virtual string GetNewDirectoryName(IExplorerDirectory parent)
        => parent is IPhysicalExplorerNode physical ? GetNewDirectoryName(physical.FileSystemPath) : "New folder";

    public virtual bool CanAccept(ExplorerTransfer transfer, IExplorerDirectory target)
    {
        if (transfer.Paths.Count == 0 || target is not IPhysicalExplorerNode physical)
            return false;

        string targetPath = physical.FileSystemPath;

        // Dropping a folder into itself or into one of its own descendants would move a directory under itself.
        return !transfer.Paths.Any(path => IsSameOrAncestor(path, targetPath));
    }

    public virtual Task AcceptAsync(
        ExplorerTransfer transfer,
        IExplorerDirectory target,
        CancellationToken cancellationToken)
    {
        if (target is not IPhysicalExplorerNode physical)
            throw new NotSupportedException($"'{target.Name}' is not a folder of the file system.");

        string targetPath = physical.FileSystemPath;
        string[] paths = [.. transfer.Paths];
        bool isMove = transfer.IsMove;

        return Task.Run(
            () =>
            {
                if (isMove)
                    ShellFileOperations.Move(paths, targetPath);
                else
                    ShellFileOperations.Copy(paths, targetPath);
            },
            cancellationToken);
    }

    /// <summary>
    /// "New folder", then "New folder (2)" and so on while the name is taken, the way Windows does.
    /// </summary>
    public static string GetNewDirectoryName(string parentPath)
    {
        const string baseName = "New folder";

        if (!Directory.Exists(Path.Combine(parentPath, baseName)))
            return baseName;

        for (int index = 2; index < int.MaxValue; index++)
        {
            string name = $"{baseName} ({index})";
            if (!Directory.Exists(Path.Combine(parentPath, name)))
                return name;
        }

        return baseName;
    }

    /// <summary>
    /// Whether <paramref name="candidate"/> is <paramref name="path"/> itself or one of its parents.
    /// </summary>
    /// <remarks>
    /// Compared on whole segments, by ending both with a separator : a plain StartsWith would report "C:\foo2" as
    /// being inside "C:\foo".
    /// </remarks>
    private static bool IsSameOrAncestor(string candidate, string path)
    {
        string normalizedCandidate = Normalize(candidate);
        string normalizedPath = Normalize(path);

        return normalizedPath.StartsWith(normalizedCandidate, StringComparison.OrdinalIgnoreCase);

        static string Normalize(string value)
            => Path.TrimEndingDirectorySeparator(Path.GetFullPath(value)) + Path.DirectorySeparatorChar;
    }
}
