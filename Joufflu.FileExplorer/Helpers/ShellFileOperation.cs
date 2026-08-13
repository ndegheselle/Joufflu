using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Joufflu.FileExplorer.Helpers
{
    /// <summary>
    /// File operations handed over to the Windows shell, on a whole batch of paths at once.
    /// </summary>
    /// <remarks>
    /// The shell takes a list of paths and not a single one, so the batch comes with one progress window and one
    /// confirmation for all of it, where a call per path displays as many of them.
    /// </remarks>
    internal static class ShellFileOperation
    {
        public static class Interop
        {
            /// <summary>Copy the paths of pFrom to the ones of pTo.</summary>
            public const uint FO_COPY = 0x0002;
            /// <summary>Move the paths of pFrom to the ones of pTo.</summary>
            public const uint FO_MOVE = 0x0001;
            /// <summary>Delete the paths of pFrom.</summary>
            public const uint FO_DELETE = 0x0003;

            [Flags]
            public enum FOF : ushort
            {
                None = 0x0000,
                /// <summary>pTo holds one destination per source, instead of a single target directory.</summary>
                MultiDestFiles = 0x0001,
                /// <summary>Send the deleted paths to the recycle bin instead of destroying them.</summary>
                AllowUndo = 0x0040,
                /// <summary>Create the destination directories without asking.</summary>
                NoConfirmMkDir = 0x0200,
                /// <summary>Confirm before permanently destroying a path, even when the confirmations are off.</summary>
                WantNukeWarning = 0x4000,
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SHFILEOPSTRUCT
            {
                public IntPtr hwnd;
                public uint wFunc;
                /// <summary>Paths separated by a null character, the last one followed by an empty one.</summary>
                public string pFrom;
                /// <summary>Destinations, written as pFrom is. Null for a deletion.</summary>
                public string? pTo;
                public FOF fFlags;
                [MarshalAs(UnmanagedType.Bool)]
                public bool fAnyOperationsAborted;
                public IntPtr hNameMappings;
                public string? lpszProgressTitle;
            }

            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            public static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);
        }

        /// <summary>The user cancelled the operation, from the confirmation or from the progress window.</summary>
        private const int DE_OPCANCELLED = 0x75;
        private const int ERROR_CANCELLED = 1223;

        /// <summary>
        /// Delete paths, all of them in one operation : the standard progress window and confirmation of Windows are
        /// displayed once for the whole batch.
        /// </summary>
        /// <param name="paths">Files and directories to delete.</param>
        /// <param name="toRecycleBin">True to send the paths to the recycle bin, false to destroy them.</param>
        /// <returns>False when the user cancelled, in which case nothing has been deleted.</returns>
        /// <exception cref="IOException">The shell refused the operation.</exception>
        public static bool Delete(IReadOnlyList<string> paths, bool toRecycleBin = true)
        {
            if (paths.Count == 0)
                return false;

            return Run(
                Interop.FO_DELETE,
                paths,
                destinations: null,
                toRecycleBin ? Interop.FOF.AllowUndo : Interop.FOF.WantNukeWarning);
        }

        /// <summary>
        /// Copy or move paths to their destinations, all of them in one operation : the standard progress window of
        /// Windows is displayed once for the whole batch, along with the "replace or skip" prompt when a name is
        /// already taken.
        /// </summary>
        /// <param name="sources">Files and directories to transfer.</param>
        /// <param name="destinations">Path each source is transferred to, one per source.</param>
        /// <param name="isMove">True to move the sources, false to copy them.</param>
        /// <returns>False when the user cancelled, in which case nothing has been transferred.</returns>
        /// <exception cref="IOException">The shell refused the operation.</exception>
        public static bool Transfer(IReadOnlyList<string> sources, IReadOnlyList<string> destinations, bool isMove)
        {
            if (sources.Count == 0)
                return false;
            if (sources.Count != destinations.Count)
                throw new ArgumentException("Each source needs a destination of its own.", nameof(destinations));

            // MultiDestFiles : the destinations are the new paths of the sources, one for one, and not a single
            // directory they all land in. That's what allows a source to be renamed while it is transferred.
            return Run(
                isMove ? Interop.FO_MOVE : Interop.FO_COPY,
                sources,
                destinations,
                Interop.FOF.MultiDestFiles | Interop.FOF.NoConfirmMkDir);
        }

        private static bool Run(
            uint function,
            IReadOnlyList<string> sources,
            IReadOnlyList<string>? destinations,
            Interop.FOF flags)
        {
            Interop.SHFILEOPSTRUCT operation = new Interop.SHFILEOPSTRUCT
            {
                hwnd = GetOwnerHandle(),
                wFunc = function,
                pFrom = ToPathList(sources),
                pTo = destinations != null ? ToPathList(destinations) : null,
                fFlags = flags,
            };

            int result = Interop.SHFileOperation(ref operation);

            if (operation.fAnyOperationsAborted || result == DE_OPCANCELLED || result == ERROR_CANCELLED)
                return false;

            // The codes aren't Win32 ones and have no message of their own, hence the raw value : they are the DE_
            // ones of SHFileOperation, and are only meant to tell one refusal from another.
            if (result != 0)
                throw new IOException($"The operation failed (0x{result:X}).");

            return true;
        }

        /// <summary>
        /// Paths as the shell reads them : separated by a null character, the last one followed by an empty one. The
        /// marshaller adds the terminating null of the string itself, which closes the list.
        /// </summary>
        private static string ToPathList(IReadOnlyList<string> paths)
            => string.Join('\0', paths.Select(Path.GetFullPath)) + '\0';

        /// <summary>
        /// Window the dialogs of the shell belong to, so that they are modal to the application instead of floating
        /// on their own. Zero when there is none.
        /// </summary>
        private static IntPtr GetOwnerHandle()
        {
            try
            {
                Window? window = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
                    ?? Application.Current?.MainWindow;

                return window != null ? new WindowInteropHelper(window).Handle : IntPtr.Zero;
            }
            catch (Exception)
            {
                // No application, or read from another thread than the one owning the windows.
                return IntPtr.Zero;
            }
        }
    }
}
