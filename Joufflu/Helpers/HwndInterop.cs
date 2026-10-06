using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Joufflu.Helpers;

/// <summary>
/// Helper class for interactions with system window events
/// </summary>
public class HwndInterop : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPOS
    {
        public IntPtr hwndInsertAfter;
        public IntPtr hwnd;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }

    private const Int32 WM_WINDOWPOSCHANGING = 0x0046;

    private HwndSource? _source;

    /// <summary>
    /// Is raised when the <see cref="WM_WINDOWPOSCHANGING"/> is occurring.
    /// </summary>
    public event EventHandler<HwndInteropPositionChangingEventArgs>? PositionChanging;

    /// <summary>
    /// Helper class for interactions with system window events
    /// </summary>
    public HwndInterop(Window window)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;

        _source = HwndSource.FromHwnd(handle);
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
        if (msg != WM_WINDOWPOSCHANGING)
            return IntPtr.Zero;

        object? data = Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
        if (data is not WINDOWPOS windowPos) throw new Exception("Could not get window position.");
        PositionChanging?.Invoke(this, new HwndInteropPositionChangingEventArgs((HwndInteropPositionChangingEventArgs.PositionChangeType)windowPos.flags));

        return IntPtr.Zero;
    }
}

public class HwndInteropPositionChangingEventArgs : EventArgs
{
    /// <summary>The <c>SWP_*</c> flags of the position change, of which Windows can set several at once.</summary>
    [Flags]
    public enum PositionChangeType
    {
        /// <summary>
        /// No official documentation found. Seems to occur when maximizing or restoring a window.
        /// </summary>
        MAXIMIZERESTORE = 0x8020,
    }

    public PositionChangeType Type { get; private set; }

    public HwndInteropPositionChangingEventArgs(PositionChangeType positionChangeType)
    {
        Type = positionChangeType;
    }
}
