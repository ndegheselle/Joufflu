using System.Runtime.InteropServices;
using System.Windows;

namespace Joufflu.Helpers;

/// <summary>
/// A display of the system, reduced to what <see cref="Controls.ThemedWindow"/> needs: the bounds of the
/// display a point is on.
/// Adapted from https://github.com/micdenny/WpfScreenHelper/
/// </summary>
internal class ScreenInterop
{
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr MonitorFromPoint(POINTSTRUCT pt, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hmonitor, [In, Out] MONITORINFO info);

    private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINTSTRUCT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private class MONITORINFO
    {
        public int cbSize = Marshal.SizeOf<MONITORINFO>();
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    /// <summary>
    /// Gets the bounds of the display.
    /// </summary>
    public Rect Bounds { get; }

    private ScreenInterop(Rect bounds)
    {
        Bounds = bounds;
    }

    /// <summary>
    /// Retrieves the display that contains the specified point, or the closest one when no display contains it.
    /// </summary>
    public static ScreenInterop FromPoint(Point point)
    {
        var pt = new POINTSTRUCT { x = (int)point.X, y = (int)point.Y };
        IntPtr monitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);

        var info = new MONITORINFO();
        GetMonitorInfo(monitor, info);

        RECT bounds = info.rcMonitor;
        return new ScreenInterop(new Rect(bounds.left, bounds.top, bounds.right - bounds.left, bounds.bottom - bounds.top));
    }
}
