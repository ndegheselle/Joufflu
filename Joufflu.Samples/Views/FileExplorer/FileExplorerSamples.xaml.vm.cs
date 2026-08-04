using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.Samples.Views.FileExplorer;

public class FileExplorerSamplesViewModel : ObservableObject
{
    /// <summary>
    /// Loader shared by the samples showing a single control, so that navigating in one of them moves the others.
    /// </summary>
    public IExplorerLoader Loader { get; private set; }

    /// <summary>
    /// Loader of the complete explorer, its own so that it keeps a navigation and a history of its own.
    /// </summary>
    public IExplorerLoader ExplorerLoader { get; private set; }

    public string ExplorerListCode =>
        "<fileExplorer:ExplorerList Loader=\"{Binding Loader}\" />";

    public string ExplorerTreeCode =>
        "<fileExplorer:ExplorerTree Loader=\"{Binding Loader}\" />";

    public string ExplorerControlBarCode =>
        """
        <fileExplorer:ExplorerControlBar Loader="{Binding Loader}" />
        <fileExplorer:ExplorerList Loader="{Binding Loader}" />
        """;

    public string ExplorerCode =>
        "<fileExplorer:Explorer Loader=\"{Binding ExplorerLoader}\" />";

    public string CustomNodeTemplateCode =>
        """
        <fileExplorer:ExplorerList Loader="{Binding Loader}">
            <fileExplorer:ExplorerList.Resources>
                <DataTemplate x:Key="NodeWithSystemIcon">
                    <StackPanel Orientation="Horizontal" joufflu:Spacing.Gap="4">
                        <Image Width="16" Height="16"
                               Source="{Binding Converter={x:Static converters:ExplorerIconConverter.Small}}" />
                        <TextBlock Text="{Binding Name}" />
                    </StackPanel>
                </DataTemplate>
                <!-- Replaces the implicit template of the node type, in this control only -->
                <DataTemplate DataType="{x:Type fileExplorer:PhysicalFile}">
                    <ContentPresenter Content="{Binding}" ContentTemplate="{StaticResource NodeWithSystemIcon}" />
                </DataTemplate>
                <DataTemplate DataType="{x:Type fileExplorer:PhysicalDirectory}">
                    <ContentPresenter Content="{Binding}" ContentTemplate="{StaticResource NodeWithSystemIcon}" />
                </DataTemplate>
            </fileExplorer:ExplorerList.Resources>
        </fileExplorer:ExplorerList>
        """;

    public FileExplorerSamplesViewModel()
    {
        Loader = new DirectoryLoader(Directory.GetCurrentDirectory());
        Loader.OpenRoot();

        ExplorerLoader = new DirectoryLoader(Directory.GetCurrentDirectory());
        ExplorerLoader.OpenRoot();
    }
}
