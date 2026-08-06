using System.IO;
using System.Runtime.InteropServices;

namespace Joufflu.FileExplorer.Helpers;

/// <summary>
/// Copying, moving, deleting and renaming through the shell.
/// </summary>
/// <remarks>
/// Deliberately one P/Invoke rather than a copy engine of our own : <c>SHFileOperation</c> brings the progress
/// window, the "replace or skip" prompt, the automatic "- Copy (2)" naming, the recycle bin and the undo of the
/// file explorer, all of which we would otherwise have to write and to host in a dialog this library has no
/// access to.
/// <para>
/// Every method blocks while the shell works, and shows its own windows : call them from a background thread.
/// </para>
/// </remarks>
internal static class ShellFileOperations
{
    #region Interop
    private const uint FoMove = 0x0001;
    private const uint FoCopy = 0x0002;
    private const uint FoDelete = 0x0003;
    private const uint FoRename = 0x0004;

    private const ushort FofNoConfirmation = 0x0010;
    private const ushort FofAllowUndo = 0x0040;
    private const ushort FofNoConfirmMkDir = 0x0200;
    private const ushort FofWantNukeWarning = 0x4000;

    /// <summary>The user cancelled the operation, which is not a failure.</summary>
    private const int DeOpCancelled = 0x75;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        [MarshalAs(UnmanagedType.LPWStr)] public string pFrom;
        [MarshalAs(UnmanagedType.LPWStr)] public string? pTo;
        public ushort fFlags;
        [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        [MarshalAs(UnmanagedType.LPWStr)] public string? lpszProgressTitle;
    }

    // DllImport and not LibraryImport : the source generated marshaller doesn't handle a structure holding strings.
    [DllImport("shell32.dll", EntryPoint = "SHFileOperationW", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT fileOp);
    #endregion

    public static void Copy(IReadOnlyList<string> sources, string targetDirectory)
        => Run(FoCopy, sources, targetDirectory, FofNoConfirmMkDir | FofAllowUndo);

    public static void Move(IReadOnlyList<string> sources, string targetDirectory)
        => Run(FoMove, sources, targetDirectory, FofNoConfirmMkDir | FofAllowUndo);

    /// <param name="permanent">
    /// True to erase the nodes, false to send them to the recycle bin.
    /// </param>
    /// <remarks>
    /// Erasing asks the user to confirm, the way the file explorer does for a Shift+Delete : NoConfirmation drops the
    /// prompt of every single node, and WantNukeWarning keeps the one warning that the content is not recoverable,
    /// that flag being defined to partially override the other. Answering no cancels the operation silently, the
    /// reload that follows then simply showing the nodes still in place.
    /// </remarks>
    public static void Delete(IReadOnlyList<string> paths, bool permanent)
        => Run(
            FoDelete,
            paths,
            null,
            permanent
                ? (ushort)(FofNoConfirmation | FofWantNukeWarning)
                : (ushort)(FofAllowUndo | FofNoConfirmation));

    /// <summary>
    /// Renames a node in place. <paramref name="newName"/> is a name, not a path : the shell refuses a rename that
    /// would move the node.
    /// </summary>
    public static void Rename(string path, string newName)
    {
        string? directory = Path.GetDirectoryName(path);
        if (directory == null)
            throw new InvalidOperationException($"'{path}' has no parent directory to be renamed in.");

        Run(FoRename, [path], Path.Combine(directory, newName), FofAllowUndo);
    }

    private static void Run(uint function, IReadOnlyList<string> sources, string? target, ushort flags)
    {
        if (sources.Count == 0)
            return;

        var operation = new SHFILEOPSTRUCT
        {
            hwnd = IntPtr.Zero,
            wFunc = function,
            pFrom = ToDoubleNullTerminated(sources),
            pTo = target == null ? null : ToDoubleNullTerminated([target]),
            fFlags = flags,
            hNameMappings = IntPtr.Zero,
            lpszProgressTitle = null
        };

        int result = SHFileOperation(ref operation);

        if (result == DeOpCancelled || operation.fAnyOperationsAborted)
            return;
        if (result != 0)
            throw new IOException($"The operation failed (shell error 0x{result:X}).");
    }

    /// <summary>
    /// The shell reads a list of paths as one string of null separated items, closed by an empty one.
    /// </summary>
    private static string ToDoubleNullTerminated(IReadOnlyList<string> paths)
        => string.Join('\0', paths) + "\0\0";
}
