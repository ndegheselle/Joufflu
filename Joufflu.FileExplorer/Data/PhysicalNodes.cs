using System.IO;

namespace Joufflu.FileExplorer.Data;

/// <summary>
/// A file of the file system of this machine.
/// </summary>
/// <remarks>
/// Left open with a public constructor on purpose : overriding
/// <see cref="Sources.PhysicalExplorerSource.CreateFile"/> to build a derived type is how a consumer attaches
/// metadata of its own to the files it displays.
/// </remarks>
public class PhysicalFile : ExplorerNode, IPhysicalExplorerNode
{
    public PhysicalFile(FileInfo info, IExplorerDirectory? parent)
        : base(info.Name, info.LastWriteTime, parent)
    {
        Path = info.FullName;
        // Read once, at load : a binding evaluated on every layout pass must not touch the disk.
        Size = info.Exists ? info.Length : null;
    }

    public override string Path { get; }

    public override long? Size { get; }

    public string FileSystemPath => Path;
}

/// <summary>
/// A folder of the file system of this machine.
/// </summary>
/// <remarks>
/// Left open with a public constructor, see <see cref="PhysicalFile"/>.
/// </remarks>
public class PhysicalDirectory : ExplorerDirectory, IPhysicalExplorerNode
{
    public PhysicalDirectory(DirectoryInfo info, IExplorerDirectory? parent)
        : base(info.Name, info.LastWriteTime, parent)
    {
        Path = info.FullName;
    }

    public override string Path { get; }

    public string FileSystemPath => Path;
}
