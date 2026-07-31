using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.FileExplorer;

/// <summary>
/// Default handling of the <see cref="ExplorerCommands"/> : apply them on the file system for the nodes
/// backed by a path (<see cref="IExplorerPathNode"/>). Commands are disabled for any other node.
/// </summary>
/// <remarks>
/// Every operation is virtual, inherit to change a behavior (send the deleted files to the recycle bin,
/// handle <see cref="ExplorerCommands.Rename"/> with your own UI, ...). Nodes are not refreshed by the
/// handler, it is up to the loader to keep the displayed nodes up to date.
/// </remarks>
public class FileSystemCommandHandler : IExplorerCommandHandler
{
    /// <summary>Clipboard format used by the shell to know if the files were cut or copied.</summary>
    private const string DropEffectFormat = "Preferred DropEffect";

    /// <summary>Instance used by default by the explorer controls.</summary>
    public static FileSystemCommandHandler Default { get; } = new FileSystemCommandHandler();

    public virtual bool CanExecute(ExplorerCommandContext context)
    {
        RoutedUICommand command = context.Command;

        if (command == ExplorerCommands.Cut || command == ExplorerCommands.Copy || command == ExplorerCommands.Delete)
            return context.Nodes.Count > 0 && GetPaths(context.Nodes).Count == context.Nodes.Count;

        if (command == ExplorerCommands.Paste)
            return GetFolderPath(context) != null && HasClipboardFiles();

        if (command == ExplorerCommands.NewFolder)
            return GetFolderPath(context) != null;

        // Renaming needs an UI, it is up to the app to handle it
        return false;
    }

    public virtual void Execute(ExplorerCommandContext context)
    {
        RoutedUICommand command = context.Command;
        string? folderPath = GetFolderPath(context);

        try
        {
            if (command == ExplorerCommands.Cut)
                CopyToClipboard(GetPaths(context.Nodes), true);
            else if (command == ExplorerCommands.Copy)
                CopyToClipboard(GetPaths(context.Nodes), false);
            else if (command == ExplorerCommands.Delete)
                Delete(GetPaths(context.Nodes));
            else if (command == ExplorerCommands.Paste && folderPath != null)
                Paste(folderPath);
            else if (command == ExplorerCommands.NewFolder && folderPath != null)
                CreateFolder(folderPath);
        }
        catch (Exception exception)
        {
            OnError(exception);
        }
    }

    #region Operations
    /// <summary>
    /// Put the paths in the clipboard, in a format the windows explorer understands.
    /// </summary>
    protected virtual void CopyToClipboard(IReadOnlyList<string> paths, bool isMove)
    {
        if (paths.Count == 0)
            return;

        StringCollection files = new StringCollection();
        foreach (string path in paths)
            files.Add(path);

        DataObject data = new DataObject();
        data.SetFileDropList(files);
        // The shell expects the drop effect as a 4 bytes integer
        DragDropEffects effect = isMove ? DragDropEffects.Move : DragDropEffects.Copy;
        data.SetData(DropEffectFormat, new MemoryStream(BitConverter.GetBytes((int)effect)));
        Clipboard.SetDataObject(data, true);
    }

    /// <summary>
    /// Copy (or move if the files were cut) the content of the clipboard into a folder.
    /// </summary>
    protected virtual void Paste(string destinationFolder)
    {
        StringCollection files = Clipboard.GetFileDropList();
        bool isMove = IsClipboardMove();

        foreach (string? path in files)
        {
            if (string.IsNullOrEmpty(path))
                continue;
            Transfer(path, destinationFolder, isMove);
        }

        // Cut files can only be pasted once
        if (isMove)
            Clipboard.Clear();
    }

    /// <summary>
    /// Copy or move a file or a folder into a destination folder, without overwriting anything.
    /// </summary>
    protected virtual void Transfer(string source, string destinationFolder, bool isMove)
    {
        bool isDirectory = Directory.Exists(source);
        if (!isDirectory && !File.Exists(source))
            return;

        // Pasting a folder into itself (or into one of its children) would loop
        if (isDirectory && IsSameOrSubFolder(destinationFolder, source))
            return;

        string destination = GetAvailablePath(
            destinationFolder,
            Path.GetFileName(Path.TrimEndingDirectorySeparator(source)));

        if (isDirectory && isMove)
            Directory.Move(source, destination);
        else if (isDirectory)
            CopyDirectory(source, destination);
        else if (isMove)
            File.Move(source, destination);
        else
            File.Copy(source, destination);
    }

    /// <summary>
    /// Delete files and folders, asking for a confirmation first.
    /// </summary>
    protected virtual void Delete(IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
            return;

        string message = paths.Count == 1
            ? $"Delete '{Path.GetFileName(Path.TrimEndingDirectorySeparator(paths[0]))}' ?"
            : $"Delete {paths.Count} items ?";
        if (MessageBox.Show(message, "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        foreach (string path in paths)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            else if (File.Exists(path))
                File.Delete(path);
        }
    }

    /// <summary>
    /// Create a new folder with an available name ("New folder", "New folder (2)", ...).
    /// </summary>
    protected virtual void CreateFolder(string destinationFolder)
    {
        Directory.CreateDirectory(GetAvailablePath(destinationFolder, "New folder"));
    }

    /// <summary>
    /// Called when an operation failed, shows the error to the user by default.
    /// </summary>
    protected virtual void OnError(Exception exception)
    {
        MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    #endregion

    #region Helpers
    /// <summary>Paths of the nodes that are backed by one.</summary>
    protected static IReadOnlyList<string> GetPaths(IReadOnlyList<IExplorerNode> nodes)
        => nodes.OfType<IExplorerPathNode>().Select(node => node.Path).ToList();

    /// <summary>Path of the folder the command applies to, if it is an existing directory.</summary>
    protected static string? GetFolderPath(ExplorerCommandContext context)
        => context.Folder is IExplorerPathNode node && Directory.Exists(node.Path) ? node.Path : null;

    /// <summary>Get a path that is not used yet in a folder ("file.txt" -> "file (2).txt").</summary>
    protected static string GetAvailablePath(string destinationFolder, string name)
    {
        string path = Path.Combine(destinationFolder, name);
        if (!Exists(path))
            return path;

        string nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
        string extension = Path.GetExtension(name);
        for (int number = 2; number < 1000; number++)
        {
            path = Path.Combine(destinationFolder, $"{nameWithoutExtension} ({number}){extension}");
            if (!Exists(path))
                return path;
        }

        throw new IOException($"No name available for '{name}' in '{destinationFolder}'.");
    }

    private static bool Exists(string path) => File.Exists(path) || Directory.Exists(path);

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.EnumerateFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        foreach (string directory in Directory.EnumerateDirectories(source))
            CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }

    private static bool IsSameOrSubFolder(string childPath, string rootPath)
    {
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootPath));
        string child = Path.TrimEndingDirectorySeparator(Path.GetFullPath(childPath));
        return child.Equals(root, StringComparison.OrdinalIgnoreCase)
            || child.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasClipboardFiles()
    {
        // The clipboard may be locked by another app
        try
        {
            return Clipboard.ContainsFileDropList();
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsClipboardMove()
    {
        if (Clipboard.GetData(DropEffectFormat) is not MemoryStream stream)
            return false;

        byte[] effect = new byte[sizeof(int)];
        if (stream.Length < effect.Length)
            return false;

        stream.Position = 0;
        stream.ReadExactly(effect);
        return (BitConverter.ToInt32(effect, 0) & (int)DragDropEffects.Move) != 0;
    }
    #endregion
}
