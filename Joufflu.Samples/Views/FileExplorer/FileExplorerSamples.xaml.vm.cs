using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Controls;
using Joufflu.FileExplorer.Loaders;
using System.IO;

namespace Joufflu.Samples.Views.FileExplorer;

public class FileExplorerSamplesViewModel : ObservableObject
{
    public IExplorerLoader Loader { get; private set; }
    public string ExplorerListCode =>
        "<fileExplorer:ExplorerList />";

    public FileExplorerSamplesViewModel()
    {
        Loader = new DirectoryLoader(Directory.GetCurrentDirectory());
        Loader.Load();
    }
}
