using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
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

    public string ExplorerListColumnsCode =>
        """
        <fileExplorer:ExplorerList Source="{Binding Source}">
            <fileExplorer:ExplorerList.ExtraColumns>
                <GridViewColumn Header="Full path" Width="260" DisplayMemberBinding="{Binding Path}" />
                <GridViewColumn Header="In folder">
                    <GridViewColumn.CellTemplate>
                        <DataTemplate>
                            <TextBlock Text="{Binding Parent.Name}" />
                        </DataTemplate>
                    </GridViewColumn.CellTemplate>
                </GridViewColumn>
            </fileExplorer:ExplorerList.ExtraColumns>
        </fileExplorer:ExplorerList>
        """;

    public string ExplorerListFilterCode =>
        "<fileExplorer:ExplorerList Source=\"{Binding Source}\" VisibleNodes=\"Files\" />";

    public ExplorerListSamplesViewModel(IToastService toats)
    {
        Source = new FileSystemSource(Directory.GetCurrentDirectory(), toats);
        Source.Open();
    }
}
