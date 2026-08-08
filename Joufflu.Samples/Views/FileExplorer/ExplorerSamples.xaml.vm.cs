using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Loaders;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerSamplesViewModel : ObservableObject
{
    /// <summary>Loader of the complete explorer, keeping a navigation and history of its own.</summary>
    public IExplorerLoader Loader { get; private set; }

    public string ExplorerCode =>
        "<fileExplorer:Explorer Loader=\"{Binding Loader}\" />";

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

    public ExplorerSamplesViewModel()
    {
        Loader = new DirectoryLoader(Directory.GetCurrentDirectory());
        Loader.OpenRoot();
    }
}
