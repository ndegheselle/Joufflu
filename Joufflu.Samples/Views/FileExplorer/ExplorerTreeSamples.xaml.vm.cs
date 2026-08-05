using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerTreeSamplesViewModel : ObservableObject
{
    /// <summary>Loader driving the tree, with a navigation and history of its own.</summary>
    public IExplorerLoader Loader { get; private set; }

    public string ExplorerTreeCode =>
        "<fileExplorer:ExplorerTree Loader=\"{Binding Loader}\" />";

    public string ExplorerTreeFilesCode =>
        "<fileExplorer:ExplorerTree Loader=\"{Binding Loader}\" VisibleNodes=\"All\" />";

    public ExplorerTreeSamplesViewModel()
    {
        Loader = new DirectoryLoader(Directory.GetCurrentDirectory());
        Loader.OpenRoot();
    }
}
