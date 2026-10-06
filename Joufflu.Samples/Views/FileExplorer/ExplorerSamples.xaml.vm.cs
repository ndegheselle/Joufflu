using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
using Joufflu.FileExplorer.FileSystem;
using Joufflu.FileExplorer.Nodes;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerSamplesViewModel : ObservableObject
{
    /// <summary>Source of the complete explorer, keeping a navigation of its own.</summary>
    public IExplorerSource Source { get; private set; }

    public string ExplorerCode =>
        "<fileExplorer:Explorer Source=\"{Binding Source}\" />";

    public string CustomNodeTemplateCode =>
        """
        <fileExplorer:Explorer Source="{Binding Source}">
            <fileExplorer:Explorer.Resources>
                <DataTemplate x:Key="NodeWithSystemIcon">
                    <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="4">
                        <Image Width="16" Height="16"
                               Source="{Binding Converter={x:Static icons:ExplorerIconConverter.Small}}" />
                        <TextBlock Text="{Binding Name}" />
                    </StackPanel>
                </DataTemplate>
                <!-- Replaces the implicit template of the node type, in this control only -->
                <DataTemplate DataType="{x:Type fileSystem:FileSystemFile}">
                    <ContentPresenter Content="{Binding}" ContentTemplate="{StaticResource NodeWithSystemIcon}" />
                </DataTemplate>
                <DataTemplate DataType="{x:Type fileSystem:FileSystemDirectory}">
                    <ContentPresenter Content="{Binding}" ContentTemplate="{StaticResource NodeWithSystemIcon}" />
                </DataTemplate>
            </fileExplorer:Explorer.Resources>
        </fileExplorer:Explorer>
        """;

    public ExplorerSamplesViewModel(IToastService toasts)
    {
        Source = new FileSystemSource(Directory.GetCurrentDirectory(), toasts);
        Source.Open();
    }
}
