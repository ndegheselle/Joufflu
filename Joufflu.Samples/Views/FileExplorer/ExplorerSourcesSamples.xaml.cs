using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Samples.Views.FileExplorer;

public partial class ExplorerSourcesSamples : UserControl
{
    public ExplorerSourcesSamples()
    {
        InitializeComponent();
    }
}

/// <summary>
/// Template of the cells of the "Pinned" column : the state it displays belongs to <see cref="VirtualFile"/> only,
/// where an extra column is shared by every row. Choosing the template by node type, rather than hiding a box bound
/// on every row, keeps the binding out of the nodes that have no such property.
/// </summary>
public class PinnedCellTemplateSelector : DataTemplateSelector
{
    /// <summary>Template of the rows carrying the state.</summary>
    public DataTemplate? PinnedTemplate { get; set; }

    /// <summary>Template of the other rows, empty : a null one would fall back on the text of the node.</summary>
    public DataTemplate? EmptyTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item is VirtualFile ? PinnedTemplate : EmptyTemplate;
    }
}
