using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.Samples.Views.FileExplorer;

public class FileExplorerSamplesViewModel : ObservableObject
{
    public IExplorerLoader Loader { get; private set; }
    public string ExplorerListCode =>
        "<fileExplorer:ExplorerList Loader=\"{Binding Loader}\" />";

    public string ExplorerTreeCode =>
        "<fileExplorer:ExplorerTree Loader=\"{Binding Loader}\" />";

    public FileExplorerSamplesViewModel()
    {
        Loader = new DirectoryLoader(Directory.GetCurrentDirectory());
        Loader.OpenRoot();
    }
}
