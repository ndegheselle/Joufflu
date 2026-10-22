using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Joufflu.Helpers;
using Point = System.Windows.Point;

namespace Joufflu.Controls;

/// <summary>
/// Shows the application icon, or the <see cref="Image.Source"/> it is given.
/// Meant for the title bar of a <see cref="ThemedWindow"/> (through <see cref="ThemedWindow.TitleBarContent"/>):
/// a click opens the system menu of the window and a double click closes it, like a native icon.
/// </summary>
public class AppIcon : Image
{
    // The application icon never changes, extract it once and share it across icons.
    private static readonly Lazy<BitmapSource?> _applicationIcon = new(GetApplicationIcon);

    static AppIcon()
    {
        WidthProperty.OverrideMetadata(typeof(AppIcon), new FrameworkPropertyMetadata(16.0));
        HeightProperty.OverrideMetadata(typeof(AppIcon), new FrameworkPropertyMetadata(16.0));
        HorizontalAlignmentProperty.OverrideMetadata(typeof(AppIcon), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
        VerticalAlignmentProperty.OverrideMetadata(typeof(AppIcon), new FrameworkPropertyMetadata(VerticalAlignment.Center));
        // The default Source, replaced as soon as one is set.
        SourceProperty.OverrideMetadata(typeof(AppIcon), new FrameworkPropertyMetadata(_applicationIcon.Value));
    }

    private static BitmapSource? GetApplicationIcon()
    {
        string? appFilePath = Environment.ProcessPath;
        if (!File.Exists(appFilePath))
            return null;

        // The bitmap copies the icon pixels, so the GDI icon handle can be released right after.
        using System.Drawing.Icon? appIcon = System.Drawing.Icon.ExtractAssociatedIcon(appFilePath);

        if (appIcon == null)
            return null;

        BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(appIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        bitmap.Freeze();
        return bitmap;
    }

    /// <inheritdoc/>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var window = Window.GetWindow(this);
        if (window == null)
            return;

        // Handled so the title bar behind the icon does not start dragging or maximizing the window.
        e.Handled = true;

        if (e.ClickCount == 2)
        {
            window.Close();
            return;
        }

        Point menuPosition = TranslatePoint(new Point(0, ActualHeight), window);
        SystemContextMenuInterop.OpenSystemContextMenu(window, menuPosition);
    }
}
