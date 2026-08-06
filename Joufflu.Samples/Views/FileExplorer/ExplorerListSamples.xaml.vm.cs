using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerListSamplesViewModel : ObservableObject
{
    /// <summary>
    /// Session shared by every sample on this page, so the control bar navigates the same session the lists observe
    /// and they all move together.
    /// </summary>
    public ExplorerSession Session { get; private set; }

    public string ExplorerListCode =>
        "<fileExplorer:ExplorerList Session=\"{Binding Session}\" />";

    public string ExplorerControlBarCode =>
        """
        <!-- One session shared by the two controls is all it takes to keep them together. -->
        <fileExplorer:ExplorerControlBar Session="{Binding Session}" />
        <fileExplorer:ExplorerList Session="{Binding Session}" />
        """;

    public string ExplorerListFilterCode =>
        "<fileExplorer:ExplorerList Session=\"{Binding Session}\" VisibleNodes=\"Files\" />";

    public string ExtraColumnsCode =>
        """
        <fileExplorer:ExplorerList Session="{Binding Session}">
            <fileExplorer:ExplorerList.ExtraColumns>
                <GridViewColumn Header="Path" DisplayMemberBinding="{Binding Path}" />
            </fileExplorer:ExplorerList.ExtraColumns>
        </fileExplorer:ExplorerList>
        """;

    public ExplorerListSamplesViewModel()
    {
        Session = new ExplorerSession(new PhysicalExplorerSource(Directory.GetCurrentDirectory()));
        _ = Session.OpenRootAsync();
    }
}
