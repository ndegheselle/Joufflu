using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Joufflu.FileExplorer.Helpers
{
    /// <summary>
    /// Icons the Windows shell associates with a folder or a file type.
    /// </summary>
    /// <remarks>
    /// The shell is never given access to the file itself (SHGFI_USEFILEATTRIBUTES), so no disk is hit and the path
    /// doesn't even have to exist : only its extension matters. The counterpart is that the files carrying their own
    /// icon (an executable, a shortcut, an image) get the generic icon of their type.
    /// </remarks>
    public static class SystemIcons
    {
        public static class Interop
        {
            /// <summary>Maximal Length of unmanaged Windows-Path-strings</summary>
            private const int MAX_PATH = 260;
            /// <summary>Maximal Length of unmanaged Typename</summary>
            private const int MAX_TYPE = 80;

            public enum FileAttribute
            {
                FILE_ATTRIBUTE_DIRECTORY = 0x10,
                FILE_ATTRIBUTE_NORMAL = 0x80
            }

            [Flags]
            public enum SHGFI : int
            {
                /// <summary>get icon</summary>
                Icon = 0x000000100,
                /// <summary>get display name</summary>
                DisplayName = 0x000000200,
                /// <summary>get type name</summary>
                TypeName = 0x000000400,
                /// <summary>get attributes</summary>
                Attributes = 0x000000800,
                /// <summary>get icon location</summary>
                IconLocation = 0x000001000,
                /// <summary>return exe type</summary>
                ExeType = 0x000002000,
                /// <summary>get system icon index</summary>
                SysIconIndex = 0x000004000,
                /// <summary>put a link overlay on icon</summary>
                LinkOverlay = 0x000008000,
                /// <summary>show icon in selected state</summary>
                Selected = 0x000010000,
                /// <summary>get only specified attributes</summary>
                Attr_Specified = 0x000020000,
                /// <summary>get large icon</summary>
                LargeIcon = 0x000000000,
                /// <summary>get small icon</summary>
                SmallIcon = 0x000000001,
                /// <summary>get open icon</summary>
                OpenIcon = 0x000000002,
                /// <summary>get shell size icon</summary>
                ShellIconSize = 0x000000004,
                /// <summary>pszPath is a pidl</summary>
                PIDL = 0x000000008,
                /// <summary>use passed dwFileAttribute</summary>
                UseFileAttributes = 0x000000010,
                /// <summary>apply the appropriate overlays</summary>
                AddOverlays = 0x000000020,
                /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
                OverlayIndex = 0x000000040,
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SHFILEINFO
            {
                public SHFILEINFO(bool b)
                {
                    hIcon = IntPtr.Zero;
                    iIcon = 0;
                    dwAttributes = 0;
                    szDisplayName = "";
                    szTypeName = "";
                }
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_TYPE)]
                public string szTypeName;
            };

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern int SHGetFileInfo(
              string pszPath,
              int dwFileAttributes,
              out SHFILEINFO psfi,
              uint cbfileInfo,
              SHGFI uFlags);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyIcon(IntPtr hIcon);
        }

        /// <summary>
        /// Icons read so far, keyed by folder or extension and size. The failures are cached too, so that a type the
        /// shell has no icon for isn't asked for again. Every icon is frozen, hence shareable by any number of
        /// bindings.
        /// </summary>
        private static readonly Dictionary<string, ImageSource?> _cache = [];
        private static readonly object _cacheLock = new();

        /// <summary>
        /// Icon of a folder or of the type of a file, null when the shell has none.
        /// </summary>
        /// <param name="path">
        /// Path of the file or folder. It doesn't have to exist : for a file only its extension is read, for a folder
        /// nothing at all.
        /// </param>
        /// <param name="isDirectory">
        /// Whether the path is a folder. Given by the caller rather than read from the disk, so that the icon of a
        /// node that has no file behind it can be asked for too.
        /// </param>
        /// <param name="isSmall">True for the 16x16 icon, false for the 32x32 one.</param>
        public static ImageSource? GetIcon(string path, bool isDirectory, bool isSmall)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // Everything sharing a type shares an icon, so the cache is keyed by type and not by path.
            string type = isDirectory ? "<folder>" : Path.GetExtension(path).ToLowerInvariant();
            string key = $"{type}|{isSmall}";
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(key, out ImageSource? cached))
                    return cached;
            }

            ImageSource? icon = ReadIcon(path, isDirectory, isSmall);
            lock (_cacheLock)
            {
                _cache[key] = icon;
            }

            return icon;
        }

        /// <summary>
        /// Empties the cache, for the rare case where the icons of the system changed while the application runs.
        /// </summary>
        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
            }
        }

        private static ImageSource? ReadIcon(string path, bool isDirectory, bool isSmall)
        {
            Interop.FileAttribute attribute = isDirectory
                ? Interop.FileAttribute.FILE_ATTRIBUTE_DIRECTORY
                : Interop.FileAttribute.FILE_ATTRIBUTE_NORMAL;
            Interop.SHGFI flags = Interop.SHGFI.Icon |
                Interop.SHGFI.UseFileAttributes |
                (isSmall ? Interop.SHGFI.SmallIcon : Interop.SHGFI.LargeIcon);

            Interop.SHFILEINFO info = new Interop.SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            Interop.SHGetFileInfo(path, (int)attribute, out info, (uint)cbFileInfo, flags);

            if (info.hIcon == IntPtr.Zero)
                return null;

            try
            {
                ImageSource icon = Imaging.CreateBitmapSourceFromHIcon(
                    info.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                // Frozen : the icon is cached and reused, and can then be read from any thread.
                icon.Freeze();
                return icon;
            }
            catch (COMException)
            {
                return null;
            }
            finally
            {
                // The handle belongs to the caller of SHGetFileInfo.
                Interop.DestroyIcon(info.hIcon);
            }
        }
    }
}
