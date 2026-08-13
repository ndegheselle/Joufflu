using System.Collections.Specialized;
using System.IO;
using System.Windows;

namespace Joufflu.FileExplorer.Helpers
{
    /// <summary>
    /// Cut and copy of nodes, through the clipboard of Windows.
    /// </summary>
    /// <remarks>
    /// The system clipboard is the only one : keeping a second, in process one alongside it would raise the question
    /// of which of the two wins, with no good answer. Using the formats of the file explorer means copying in one and
    /// pasting in the other works in both directions, for free.
    /// </remarks>
    internal static class ExplorerClipboard
    {
        /// <summary>
        /// Format the shell tells a cut from a copy with : a single DWORD, 2 for a move and 5 for a copy (1 alone
        /// means copy as well, 4 being the "no preference" bit the file explorer sets).
        /// </summary>
        private const string PreferredDropEffect = "Preferred DropEffect";

        private const int DropEffectCopy = 5;
        private const int DropEffectMove = 2;

        /// <summary>
        /// Put paths in the clipboard, as a cut when <paramref name="isMove"/>. Returns whether the clipboard
        /// accepted them.
        /// </summary>
        public static bool TrySetPaths(IReadOnlyList<string> paths, bool isMove)
        {
            if (paths.Count == 0)
                return false;

            StringCollection files = [.. paths];

            DataObject data = new DataObject();
            data.SetFileDropList(files);
            data.SetData(
                PreferredDropEffect,
                new MemoryStream(BitConverter.GetBytes(isMove ? DropEffectMove : DropEffectCopy)));

            try
            {
                // Copy and not SetDataObject alone : the content must outlive this application.
                Clipboard.SetDataObject(data, copy: true);
                return true;
            }
            catch (Exception)
            {
                // Another process can be holding the clipboard open.
                return false;
            }
        }

        /// <summary>
        /// Paths held by the clipboard, empty when it holds something else.
        /// </summary>
        /// <param name="isMove">True when the paths have been cut, false when they have been copied.</param>
        public static IReadOnlyList<string> GetPaths(out bool isMove)
        {
            isMove = false;

            try
            {
                if (!Clipboard.ContainsFileDropList())
                    return [];

                IDataObject? data = Clipboard.GetDataObject();
                if (data == null)
                    return [];

                isMove = ReadIsMove(data);

                StringCollection files = Clipboard.GetFileDropList();
                return [.. files.Cast<string?>().Where(path => path != null).Select(path => path!)];
            }
            catch (Exception)
            {
                return [];
            }
        }

        private static bool ReadIsMove(IDataObject data)
        {
            if (data.GetData(PreferredDropEffect) is not MemoryStream stream || stream.Length < 4)
                return false;

            return BitConverter.ToInt32(stream.ToArray(), 0) == DropEffectMove;
        }
    }
}
