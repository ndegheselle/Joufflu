using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Joufflu.Data.Model;

namespace Joufflu.Data.Controls;

/// <summary>
/// Shows a <see cref="Node"/> read only: the key, the type and the value of each node, a forced
/// value flagged as such. Nothing is edited but whether a node is expanded.
/// </summary>
public partial class DataDisplay : UserControl
{
    public static readonly DependencyProperty NodeProperty = DependencyProperty.Register(
        nameof(Node),
        typeof(DataNode),
        typeof(DataDisplay),
        new PropertyMetadata(null, OnNodeChanged));

    /// <summary>
    /// The node shown by the tree: an object shows its properties as the first level, any other
    /// node is that level itself.
    /// </summary>
    public DataNode? Node
    {
        get => (DataNode?)GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

    private readonly DataDisplayRows _rows;

    public DataDisplay()
    {
        InitializeComponent();

        // Applied before the next render, once whatever changed the tree in the meantime is done.
        _rows = new DataDisplayRows(apply => Dispatcher.BeginInvoke(DispatcherPriority.DataBind, apply));
        RowList.ItemsSource = _rows;

        // Only followed while shown: a tree kept by its host would otherwise keep the rows alive.
        Loaded += (_, _) => _rows.Show(Node);
        Unloaded += (_, _) => _rows.Show(null);
    }

    private static void OnNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var display = (DataDisplay)d;
        if (display.IsLoaded)
            display._rows.Show((DataNode?)e.NewValue);
    }
}
