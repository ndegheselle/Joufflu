using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.Samples.Views.FileExplorer;

public class FileExplorerSamplesViewModel : ObservableObject
{
    public IExplorerLoader Loader { get; private set; }
    public string ExplorerListCode =>
        "<fileExplorer:ExplorerList />";

    public FileExplorerSamplesViewModel()
    {
        Loader = new DirectoryLoader(Directory.GetCurrentDirectory());
        Loader.OpenRoot();
    }
}
