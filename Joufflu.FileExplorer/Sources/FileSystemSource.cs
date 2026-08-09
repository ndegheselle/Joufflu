using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Feedback.Controls;
using Joufflu.FileExplorer.Data;
using System.IO;
using System.Windows;

namespace Joufflu.FileExplorer.Sources
{
    public partial class FileSystemSource : ObservableObject, IExplorerSource
    {
        private readonly string rootDirectoryPath;
        private readonly IToastService? toasts;

        [ObservableProperty]
        private IExplorerDirectory? root;
        [ObservableProperty]
        private IExplorerDirectory? current;

        public FileSystemSource(string rootDirectoryPath, IToastService? toasts) {
            this.rootDirectoryPath = rootDirectoryPath;
            this.toasts = toasts;
        }

        #region Open
        public Task Open()
        {
            Root = new FileSystemDirectory(new DirectoryInfo(rootDirectoryPath), null);
            return Open(Root);
        }

        [RelayCommand]
        public async Task Open(IExplorerNode node)
        {
            if (node is IExplorerDirectory directory)
            {
                await OpenDirectory(directory, 2);
            }
            else if (node is IExplorerFile file)
            {
            }
        }

        [RelayCommand]
        public void OpenInExplorer(IExplorerNode node)
        { throw new NotImplementedException(); }

        [RelayCommand]
        public void OpenWithDefaultSoftware(IExplorerFile file)
        { throw new NotImplementedException(); }

        /// <summary>
        /// Open a directory by loading all it's childrens and setting it as <see cref="Current"/>. 
        /// Recursif on <paramref name="depth"/> sub directories.
        /// </summary>
        protected virtual Task OpenDirectory(IExplorerDirectory directory, int depth)
        {
            directory.Children.Clear();
            var dirInfo = new DirectoryInfo(directory.Path);
            foreach (var entry in dirInfo.EnumerateFileSystemInfos())
            {
                if (entry is FileInfo fi)
                {
                    directory.Children.Add(new FileSystemFile(fi, directory));
                }
                else if (entry is DirectoryInfo di)
                {
                    FileSystemDirectory subDirectory = new FileSystemDirectory(di, directory);
                    directory.Children.Add(subDirectory);
                    if (depth > 0)
                        OpenDirectory(subDirectory, depth - 1);
                }
            }

            Current = directory;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Open a file (for exemple a preview). Current implementation is empty.
        /// </summary>
        protected virtual Task OpenFile(IExplorerFile file)
        {  return Task.CompletedTask; }

        #endregion

        #region Copy / Paste

        [RelayCommand]
        public Task Copy(IEnumerable<IExplorerNode> nodes)
        { throw new NotImplementedException(); }

        [RelayCommand]
        public Task Cut(IEnumerable<IExplorerNode> nodes)
        { throw new NotImplementedException(); }

        // XXX : how to know if copy or cut ?
        [RelayCommand]
        public Task Past(IExplorerDirectory target, IEnumerable<IExplorerNode> nodes)
        { throw new NotImplementedException(); }

        #endregion

        #region Misc

        [RelayCommand]
        public void CopyPath(IExplorerNode node)
        { 
            Clipboard.SetText(node.Path);
            toasts?.Info("Path copied to clipboard.");
        }

        [RelayCommand]
        public void Rename(IExplorerNode node)
        { throw new NotImplementedException(); }

        #endregion
    }
}