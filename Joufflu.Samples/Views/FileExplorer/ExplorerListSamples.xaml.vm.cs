using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback.Controls;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerListSamplesViewModel : ObservableObject
{
    /// <summary>
    /// Loader shared by every sample on this page, so the control bar navigates the same
    /// loader the lists observe and they all move together.
    /// </summary>
    public IExplorerSource Source { get; private set; }

    public string ExplorerListCode =>
        "<fileExplorer:ExplorerList Loader=\"{Binding Loader}\" />";

    public string ExplorerControlBarCode =>
        """
        <fileExplorer:ExplorerControlBar Loader="{Binding Loader}" />
        <fileExplorer:ExplorerList Loader="{Binding Loader}" />
        """;

    public string ExplorerListFilterCode =>
        "<fileExplorer:ExplorerList Loader=\"{Binding Loader}\" VisibleNodes=\"Files\" />";

    public ExplorerListSamplesViewModel(IToastService toats)
    {
        Source = new FileSystemSource(Directory.GetCurrentDirectory(), toats);
        Source.Open();
    }
}
