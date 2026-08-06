using System.Windows.Controls;
using Joufflu.FileExplorer.Controls.Base;

namespace Joufflu.Samples.Views.FileExplorer;

public partial class ExplorerSourceSamples : UserControl
{
    public ExplorerSourceSamples()
    {
        InitializeComponent();
    }

    /// <summary>
    /// A note has no application to be opened with, so the sample shows its content itself : handling
    /// NodeActivated is what takes the default behaviour over.
    /// </summary>
    private void OnNoteActivated(object? sender, ExplorerNodeEventArgs e)
    {
        if (DataContext is ExplorerSourceSamplesViewModel viewModel)
            viewModel.OpenNote(e.Node);

        e.Handled = true;
    }
}
