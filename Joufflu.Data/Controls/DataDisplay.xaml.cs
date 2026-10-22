using System.Windows;
using System.Windows.Controls;
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
        typeof(DataObject),
        typeof(DataDisplay),
        new PropertyMetadata(null));

    /// <summary>The root object shown by the tree.</summary>
    public DataObject? Node
    {
        get => (DataObject?)GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

    public DataDisplay()
    {
        InitializeComponent();
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        if (newContent is FrameworkElement content)
            content.DataContext = this;
    }
}
