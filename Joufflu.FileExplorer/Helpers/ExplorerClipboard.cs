using System.Collections.Specialized;
using System.IO;
using System.Windows;

namespace Joufflu.FileExplorer.Helpers;

/// <summary>
/// Cut and copy of nodes, through the clipboard of Windows.
/// </summary>
/// <remarks>
/// The system clipboard is the only one : keeping a second, in process one alongside it would raise the question of
/// which of the two wins, with no good answer. Using the formats of the file explorer means copying in one and pasting
/// in the other works in both directions, for free.
/// </remarks>
internal static class ExplorerClipboard
{
    /// <summary>
    /// Format the shell tells a cut from a copy with : a single DWORD, 2 for a move and 5 for a copy (1 alone means
    /// copy as well, 4 being the "no preference" bit the file explorer sets).
    /// </summary>
    private const string PreferredDropEffect = "Preferred DropEffect";

    private const int DropEffectCopy = 5;
    private const int DropEffectMove = 2;

    public static bool TrySetPaths(IReadOnlyList<string> paths, bool isMove)
    {
        if (paths.Count == 0)
            return false;

        var files = new StringCollection();
        foreach (string path in paths)
            files.Add(path);

        var data = new DataObject();
        data.SetFileDropList(files);
        data.SetData(PreferredDropEffect, new MemoryStream(BitConverter.GetBytes(isMove ? DropEffectMove : DropEffectCopy)));

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
    /// Paths held by the clipboard, empty when it holds something else. Reading the clipboard is an interop call that
    /// may block or throw, so this is called when a menu opens rather than from a CanExecute.
    /// </summary>
    public static IReadOnlyList<string> GetPaths(out bool isMove)
    {
        isMove = false;

        try
        {
            if (!Clipboard.ContainsFileDropList())
                return [];

            var data = Clipboard.GetDataObject();
            if (data == null)
                return [];

            isMove = ReadIsMove(data);

            StringCollection files = Clipboard.GetFileDropList();
            return files.Cast<string?>().Where(path => path != null).Select(path => path!).ToArray();
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
