using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.FileExplorer.Sources;

namespace Joufflu.Samples.Views.FileExplorer;

public class ExplorerSamplesViewModel : ObservableObject
{
    /// <summary>Session of the complete explorer, keeping a navigation and history of its own.</summary>
    public ExplorerSession Session { get; private set; }

    public string ExplorerCode =>
        """
        <fileExplorer:Explorer Session="{Binding Session}" />
        """
        + """


        // A session holds where the user is, a source where the nodes come from.
        Session = new ExplorerSession(new PhysicalExplorerSource(Directory.GetCurrentDirectory()));
        _ = Session.OpenRootAsync();
        """;

    public string CustomNodeTemplateCode =>
        """
        <fileExplorer:ExplorerList Session="{Binding Session}">
            <fileExplorer:ExplorerList.Resources>
                <DataTemplate x:Key="NodeWithSystemIcon">
                    <StackPanel Orientation="Horizontal" joufflu:Spacing.Gap="4">
                        <Image Width="16" Height="16"
                               Source="{Binding Converter={x:Static converters:ExplorerIconConverter.Small}}" />
                        <TextBlock Text="{Binding Name}" />
                    </StackPanel>
                </DataTemplate>
                <!-- One entry is enough : the implicit template lookup walks base classes, and every
                     node of a physical source derives from ExplorerNode. -->
                <DataTemplate DataType="{x:Type data:ExplorerNode}">
                    <ContentPresenter Content="{Binding}" ContentTemplate="{StaticResource NodeWithSystemIcon}" />
                </DataTemplate>
            </fileExplorer:ExplorerList.Resources>
        </fileExplorer:ExplorerList>
        """;

    public string DragAndDropCode =>
        """
        <!-- Two sessions, so the two explorers navigate on their own. -->
        <fileExplorer:Explorer Session="{Binding Session}" />
        <fileExplorer:Explorer Session="{Binding OtherSession}" />
        """;

    /// <summary>Second session of the drag and drop sample, on the parent folder so the two differ.</summary>
    public ExplorerSession OtherSession { get; private set; }

    public ExplorerSamplesViewModel()
    {
        string current = Directory.GetCurrentDirectory();
        Session = new ExplorerSession(new PhysicalExplorerSource(current));
        OtherSession = new ExplorerSession(
            new PhysicalExplorerSource(Directory.GetParent(current)?.FullName ?? current));

        // Not awaited : the session reports its own failures through LastError.
        _ = Session.OpenRootAsync();
        _ = OtherSession.OpenRootAsync();
    }
}
