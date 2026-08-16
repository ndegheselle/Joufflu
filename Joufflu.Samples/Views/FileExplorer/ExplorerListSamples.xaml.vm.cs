using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback.Controls;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerListSamplesViewModel : ObservableObject
{
    /// <summary>
    /// Source shared by every sample on this page, so the lists observe the same one and they all
    /// move together.
    /// </summary>
    public IExplorerSource Source { get; private set; }

    public string ExplorerListCode =>
        "<fileExplorer:ExplorerList Source=\"{Binding Source}\" />";

    public string ExplorerListFilterCode =>
        "<fileExplorer:ExplorerList Source=\"{Binding Source}\" VisibleNodes=\"Files\" />";

    public ExplorerListSamplesViewModel(IToastService toats)
    {
        Source = new FileSystemSource(Directory.GetCurrentDirectory(), toats);
        Source.Open();
    }
}
