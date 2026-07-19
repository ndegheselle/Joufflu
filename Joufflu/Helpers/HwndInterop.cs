using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Joufflu.Helpers;

/// <summary>
/// Helper class for interactions with system window events
/// </summary>
public class HwndInterop : IDisposable
{
    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hwndInsertAfter;
        public IntPtr hwnd;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }

    private const Int32 WM_SYSCOMMAND = 0x112;
    private const Int32 WM_SIZE = 0x0005;
    private const Int32 WM_WINDOWPOSCHANGING = 0x0046;

    private const Int32 SC_MAXIMIZE = 0xF030;
    private const Int32 SC_RESTORE = 0xF120;
    private const Int32 SC_MINIMIZE = 0xF020;

    private readonly IntPtr _handle;
    private HwndSource? _source;

    /// <summary>
    /// Is raised when the <see cref="WM_SIZE"/> is occuring.
    /// </summary>
    public event EventHandler<HwndInteropSizeChangedEventArgs>? SizeChanged;

    /// <summary>
    /// Is raised when the <see cref="WM_WINDOWPOSCHANGING"/> is occuring.
    /// </summary>
    public event EventHandler<HwndInteropPositionChangingEventArgs>? PositionChanging;

    /// <summary>
    /// Helper class for interactions with system window events
    /// </summary>
    public HwndInterop(Window window)
    {
        _handle = new WindowInteropHelper(window).Handle;

        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(WndProc);
    }

    /// <summary>Removes the message hook so the window can be collected.</summary>
    public void Dispose()
    {
        _source?.RemoveHook(WndProc);
        _source = null;
        GC.SuppressFinalize(this);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_SIZE:
                SizeChanged?.Invoke(this, new HwndInteropSizeChangedEventArgs((HwndInteropSizeChangedEventArgs.ResizeRequestType)wParam));
                break;
            case WM_WINDOWPOSCHANGING:
                object? data = Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                if (data is not WINDOWPOS windowPos) throw new Exception("Could not get window position.");
                PositionChanging?.Invoke(this, new HwndInteropPositionChangingEventArgs((HwndInteropPositionChangingEventArgs.PositionChangeType)windowPos.flags));
                break;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Sends a system event to maximize the window.
    /// </summary>
    public void Maximize()
    {
        SendMessage(_handle, WM_SYSCOMMAND, (IntPtr)SC_MAXIMIZE, IntPtr.Zero);
    }

    /// <summary>
    /// Sends a system event to restore the window.
    /// </summary>
    public void Restore()
    {
        SendMessage(_handle, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
    }

    /// <summary>
    /// Sends a system event to minimize the window.
    /// </summary>
    public void Minimize()
    {
        SendMessage(_handle, WM_SYSCOMMAND, (IntPtr)SC_MINIMIZE, IntPtr.Zero);
    }
}

public class HwndInteropSizeChangedEventArgs : EventArgs
{
    /// <summary>
    /// The type of resizing requested.
    /// </summary>
    public enum ResizeRequestType
    {
        /// <summary>
        /// The window has been resized, but neither the <see cref="Minimized"/> nor <see cref="Maximized"/> value applies.
        /// </summary>
        Restored = 0,

        /// <summary>
        /// The window has been minimized.

        /// </summary>
        Minimized = 1,

        /// <summary>
        /// The window has been maximized.
        /// </summary>
        Maximized = 2,

        /// <summary>
        /// Message is sent to all pop-up windows when some other window has been restored to its former size.
        /// </summary>
        MaxShow = 3,

        /// <summary>
        /// Message is sent to all pop-up windows when some other window is maximized.
        /// </summary>
        MaxHide = 4,
    }

    /// <summary>
    /// The type of resizing requested.
    /// </summary>
    public ResizeRequestType Type { get; private set; }

    public HwndInteropSizeChangedEventArgs(ResizeRequestType resizeRequestType)
    {
        Type = resizeRequestType;
    }
}

public class HwndInteropPositionChangingEventArgs : EventArgs
{
    public enum PositionChangeType
    {
        /// <summary>
        /// Draws a frame (defined in the window's class description) around the window. Same as the <see cref="FRAMECHANGED"/> flag.
        /// </summary>
        DRAWFRAME = 0x0020,

        /// <summary>
        /// Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed.
        /// </summary>
        FRAMECHANGED = DRAWFRAME,

        /// <summary>
        /// Hides the window.
        /// </summary>
        HIDEWINDOW = 0x0080,

        /// <summary>
        /// Does not activate the window.
        /// </summary>
        NOACTIVATE = 0x0010,

        /// <summary>
        /// Discards the entire contents of the client area.
        /// </summary>
        NOCOPYBITS = 0x0100,

        /// <summary>
        /// Retains the current position (ignores the x and y members).
        /// </summary>
        NOMOVE = 0x0002,

        /// <summary>
        /// Does not change the owner window's position in the Z order.
        /// </summary>
        NOOWNERZORDER = 0x0200,

        /// <summary>
        /// Does not redraw changes.
        /// </summary>
        SWP_NOREDRAW = 0x0008,

        /// <summary>
        /// Does not change the owner window's position in the Z order. Same as the <see cref="NOOWNERZORDER"/> flag.
        /// </summary>
        NOREPOSITION = NOOWNERZORDER,

        /// <summary>
        /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        NOSENDCHANGING = 0x0400,

        /// <summary>
        /// Retains the current size (ignores the cx and cy members).
        /// </summary>
        NOSIZE = 0x0001,

        /// <summary>
        /// Retains the current Z order (ignores the hwndInsertAfter member).
        /// </summary>
        NOZORDER = 0x0004,

        /// <summary>
        /// Displays the window.
        /// </summary>
        SHOWWINDOW = 0x0040,

        /// <summary>
        /// No official documentation found. Seems to occur whe maximizing or restoring a window.
        /// </summary>
        MAXIMIZERESTORE = 0x8020,
    }

    public PositionChangeType Type { get; private set; }

    public HwndInteropPositionChangingEventArgs(PositionChangeType positionChangeType)
    {
        Type = positionChangeType;
    }
}