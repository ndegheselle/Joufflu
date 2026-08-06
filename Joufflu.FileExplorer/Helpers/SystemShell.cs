using System.Diagnostics;
using System.IO;

namespace Joufflu.FileExplorer.Helpers;

/// <summary>
/// Handing a path over to Windows : showing it in the file explorer, opening it with the application it is
/// associated with.
/// </summary>
/// <remarks>
/// These are the same for every source, and only need a path on this machine : they belong to the explorer, not to a
/// source. Both swallow their failures, an unopenable file being a dead end rather than an error to report.
/// </remarks>
public static class SystemShell
{
    /// <summary>
    /// Opens the file explorer on the node, selected inside its parent folder.
    /// </summary>
    public static bool ShowInFileExplorer(string path)
    {
        try
        {
            // Quoted because a path may hold spaces, and /select so that a folder shows inside its parent rather
            // than opening, which is what the file explorer itself does for "Show in folder".
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true })
                ?.Dispose();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Opens a file with the application it is associated with, or a folder in the file explorer.
    /// </summary>
    public static bool OpenWithDefaultApplication(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
            return false;

        try
        {
            // UseShellExecute is what resolves the association ; without it this would only run executables.
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true })?.Dispose();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
