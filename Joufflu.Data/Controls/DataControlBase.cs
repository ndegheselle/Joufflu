using System.Windows;
using System.Windows.Controls;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Controls;

/// <summary>
/// What <see cref="DataFill"/> and <see cref="DataEdit"/> share: the node they show and what its
/// fields can be forced to.
/// <para>
/// Only the content binds to the control itself: the control keeps its host's DataContext, so
/// <see cref="Node"/> and <see cref="ManualValues"/> can be bound from the outside.
/// </para>
/// </summary>
public class DataControlBase : UserControl
{
    public static readonly DependencyProperty NodeProperty = DependencyProperty.Register(
        nameof(Node),
        typeof(DataObject),
        typeof(DataControlBase),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>The root object shown by the tree.</summary>
    public DataObject? Node
    {
        get => (DataObject?)GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

    public static readonly DependencyProperty ManualValuesProperty = DependencyProperty.Register(
        nameof(ManualValues),
        typeof(IEnumerable<DataManualValue>),
        typeof(DataControlBase),
        new PropertyMetadata(null));

    /// <summary>
    /// What a field can be forced to on top of null and undefined, which are offered where the
    /// schema allows them. Each entry says which type it stands for, so a field is only offered the
    /// ones that fit it.
    /// </summary>
    public IEnumerable<DataManualValue>? ManualValues
    {
        get => (IEnumerable<DataManualValue>?)GetValue(ManualValuesProperty);
        set => SetValue(ManualValuesProperty, value);
    }

    /// <summary>The JSON of [Node] so far, null while there is no node.</summary>
    public JToken? ToToken() => Node?.ToToken();

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        if (newContent is FrameworkElement content)
            content.DataContext = this;
    }
}
