using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback.Controls;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.DragAndDrop;

namespace Joufflu.Samples.Views.FileExplorer;

public partial class DragAndDropSamplesViewModel : ObservableObject
{
    /// <summary>
    /// Nodes of the current directory, reordered by the drag and drop.
    /// </summary>
    public ObservableCollection<IExplorerNode> Nodes { get; } = [];

    public DragHandler NodeDragHandler { get; }

    public DropHandler NodeDropHandler { get; }

    public DropHandler FileDropHandler { get; }

    [ObservableProperty]
    private string droppedFilesText = "Drop files here";

    public string ReorderCode =>
        """
        <ListBox
            ItemsSource="{Binding Nodes}"
            dnd:Drag.Handler="{Binding NodeDragHandler}"
            dnd:Drag.AdornerTemplate="{StaticResource NodeDragAdorner}"
            dnd:Drop.Handler="{Binding NodeDropHandler}"
            ItemContainerStyle="{StaticResource DroppableNodeItem}" />

        <Style x:Key="DroppableNodeItem" TargetType="ListBoxItem" BasedOn="{StaticResource {x:Type ListBoxItem}}">
            <Style.Triggers>
                <Trigger Property="dnd:Drop.IsHovering" Value="True">
                    <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.InfoBrush}}" />
                </Trigger>
            </Style.Triggers>
        </Style>
        """;

    public string FileDropCode =>
        """
        <Border dnd:Drop.Handler="{Binding FileDropHandler}">
            <Border.Style>
                <Style TargetType="Border">
                    <Setter Property="Background" Value="Transparent" />
                    <Style.Triggers>
                        <Trigger Property="dnd:Drop.IsDropTarget" Value="True">
                            <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.InfoBrush}}" />
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </Border.Style>
        </Border>
        """;

    public DragAndDropSamplesViewModel(IToastService toasts)
    {
        LoadCurrentDirectory();

        // Only a move makes sense when reordering, the cursor shows it thanks to GiveFeedback
        NodeDragHandler = new DragHandler { AllowedEffects = DragDropEffects.Move };
        NodeDropHandler = new ReorderDropHandler(Nodes);
        FileDropHandler = new FilePathsDropHandler(
            paths =>
            {
                DroppedFilesText = string.Join(Environment.NewLine, paths);
                toasts.Info($"{paths.Count} path(s) dropped.");
            });
    }

    private void LoadCurrentDirectory()
    {
        var directory = new FileSystemDirectory(new DirectoryInfo(Directory.GetCurrentDirectory()), null);
        foreach (var entry in new DirectoryInfo(directory.Path).EnumerateFileSystemInfos())
        {
            if (entry is FileInfo file)
                Nodes.Add(new FileSystemFile(file, directory));
            else if (entry is DirectoryInfo subDirectory)
                Nodes.Add(new FileSystemDirectory(subDirectory, directory));
        }
    }
}

/// <summary>
/// Moves the dragged node before the one it is dropped on, or at the end when dropped outside of a row.
/// </summary>
public class ReorderDropHandler : DropHandler<IExplorerNode>
{
    private readonly ObservableCollection<IExplorerNode> _nodes;

    public ReorderDropHandler(ObservableCollection<IExplorerNode> nodes)
    {
        _nodes = nodes;
        // Highlight the row under the cursor rather than the whole list
        HoverContainerType = typeof(ListBoxItem);
    }

    protected override bool CanDrop(IExplorerNode data, DropContext context)
    {
        // Dropping a node on itself changes nothing
        return _nodes.Contains(data) && context.GetTarget<IExplorerNode>() != data;
    }

    protected override void ApplyDrop(IExplorerNode data, DropContext context)
    {
        int oldIndex = _nodes.IndexOf(data);
        var target = context.GetTarget<IExplorerNode>();
        int newIndex = target != null ? _nodes.IndexOf(target) : _nodes.Count - 1;

        if (oldIndex < 0 || newIndex < 0)
            return;

        _nodes.Move(oldIndex, newIndex);
    }
}

/// <summary>
/// Accepts the files dropped from outside of the application and reports their paths.
/// </summary>
public class FilePathsDropHandler : DropHandler
{
    private readonly Action<IReadOnlyList<string>> _onDropped;

    public FilePathsDropHandler(Action<IReadOnlyList<string>> onDropped) { _onDropped = onDropped; }

    protected override bool CanDrop(DropContext context) => context.FilePaths.Count > 0;

    protected override void ApplyDrop(DropContext context) => _onDropped(context.FilePaths);
}
