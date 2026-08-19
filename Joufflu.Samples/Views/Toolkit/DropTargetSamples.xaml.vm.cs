using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Samples.Views.Toolkit;

public class DropTargetSamplesViewModel : ObservableObject
{
    /// <summary>Names of the last accepted files, listed under the drop zone.</summary>
    public ObservableCollection<string> DroppedFiles { get; } = [];

    /// <summary>
    /// Handles the drop, and decides what the zone accepts through its <c>CanExecute</c> : a drag
    /// of files holding nothing but PDFs.
    /// </summary>
    public IRelayCommand DropFilesCommand { get; }

    public DropTargetSamplesViewModel()
    {
        DropFilesCommand = new RelayCommand<IDataObject>(DropFiles, CanDropFiles);
    }

    // Called on every mouse move of the drag, so it only looks at the paths and never at the files.
    private static bool CanDropFiles(IDataObject? data)
    {
        string[]? files = GetFiles(data);
        return files?.Length > 0 && files.All(IsPdf);
    }

    private void DropFiles(IDataObject? data)
    {
        DroppedFiles.Clear();
        foreach (string file in GetFiles(data) ?? [])
            DroppedFiles.Add(Path.GetFileName(file));
    }

    /// <summary>Paths of a file drag, or <c>null</c> for data that isn't one (a text selection, an image, …).</summary>
    private static string[]? GetFiles(IDataObject? data) => data?.GetData(DataFormats.FileDrop) as string[];

    private static bool IsPdf(string path) => Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public string DropCode =>
        """
        <!-- AllowDrop and the drag events are handled by the behavior -->
        <Border joufflu:DropTarget.Command="{Binding DropFilesCommand}"
                BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}">
            <Border.Style>
                <Style TargetType="Border">
                    <Setter Property="Background" Value="Transparent" />
                    <Style.Triggers>
                        <!-- True only while accepted data hovers: refused files never highlight -->
                        <Trigger Property="joufflu:DropTarget.IsDragOver" Value="True">
                            <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.Primary100Brush}}" />
                            <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.PrimaryBrush}}" />
                        </Trigger>
                    </Style.Triggers>
                </Style>
            </Border.Style>
            <TextBlock VerticalAlignment="Center" Text="Drop .pdf files here" />
        </Border>

        // The command's CanExecute is the whole filter: what it refuses can't be dropped
        public IRelayCommand DropFilesCommand { get; }
            = new RelayCommand<IDataObject>(DropFiles, CanDropFiles);

        private static bool CanDropFiles(IDataObject? data)
        {
            string[]? files = data?.GetData(DataFormats.FileDrop) as string[];
            return files?.Length > 0 && files.All(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
        }

        // Same IDataObject, once the drop landed
        private static void DropFiles(IDataObject? data) { ... }
        """;
}
