using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerTreeSamplesViewModel : ObservableObject
{
    /// <summary>Source driving the tree, with a navigation and history of its own.</summary>
    public IExplorerSource Source { get; private set; }

    public string ExplorerTreeCode =>
        "<fileExplorer:ExplorerTree Source=\"{Binding Source}\" />";

    public string ExplorerTreeFilesCode =>
        "<fileExplorer:ExplorerTree Source=\"{Binding Source}\" VisibleNodes=\"All\" />";

    public ExplorerTreeSamplesViewModel(IToastService toasts)
    {
        Source = new FileSystemSource(Directory.GetCurrentDirectory(), toasts);
        Source.Open();
    }
}
