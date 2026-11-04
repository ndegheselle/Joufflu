using System.Windows;
using System.Windows.Controls;
using Joufflu.Data.Model;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Controls;

/// <summary>
/// Shows a <see cref="Node"/> read only: the key, the type and the value of each node, a forced
/// value flagged as such. Nothing is edited but whether a node is expanded.
/// <para>
/// Only the content binds to the control itself: the control keeps its host's DataContext, so
/// <see cref="Node"/> can be bound from the outside.
/// </para>
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

    private static readonly DependencyPropertyKey RootsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Roots),
        typeof(IEnumerable<DataNode>),
        typeof(DataDisplay),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RootsProperty = RootsPropertyKey.DependencyProperty;

    /// <summary>The first level of the tree, what <see cref="Node"/> shows.</summary>
    public IEnumerable<DataNode>? Roots => (IEnumerable<DataNode>?)GetValue(RootsProperty);

    public DataDisplay()
    {
        InitializeComponent();
    }

    private static void OnNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        IEnumerable<DataNode>? roots = e.NewValue switch
        {
            DataObject obj => obj.Properties,
            DataNode node => [node],
            _ => null
        };
        d.SetValue(RootsPropertyKey, roots);
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        if (newContent is FrameworkElement content)
            content.DataContext = this;
    }
}
