using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerTreeSamplesViewModel : ObservableObject
{
    /// <summary>Session driving the tree, with a navigation and history of its own.</summary>
    public ExplorerSession Session { get; private set; }

    public string ExplorerTreeCode =>
        "<fileExplorer:ExplorerTree Session=\"{Binding Session}\" />";

    public string ExplorerTreeFilesCode =>
        "<fileExplorer:ExplorerTree Session=\"{Binding Session}\" VisibleNodes=\"All\" />";

    public ExplorerTreeSamplesViewModel()
    {
        Session = new ExplorerSession(new PhysicalExplorerSource(Directory.GetCurrentDirectory()));
        _ = Session.OpenRootAsync();
    }
}
