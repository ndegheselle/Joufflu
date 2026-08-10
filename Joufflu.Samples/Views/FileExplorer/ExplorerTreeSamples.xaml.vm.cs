using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerTreeSamplesViewModel : ObservableObject
{
    /// <summary>Loader driving the tree, with a navigation and history of its own.</summary>
    public IExplorerSource Loader { get; private set; }

    public string ExplorerTreeCode =>
        "<fileExplorer:ExplorerTree Loader=\"{Binding Loader}\" />";

    public string ExplorerTreeFilesCode =>
        "<fileExplorer:ExplorerTree Loader=\"{Binding Loader}\" VisibleNodes=\"All\" />";

    public ExplorerTreeSamplesViewModel()
    {
    }
}
