using Joufflu.Data.Model;

namespace Joufflu.Data.Controls;

/// <summary>
/// Builds a <see cref="DataControlBase.Node"/> from scratch: keys, types and values are all edited.
/// Starts from an empty root object when none is given.
/// </summary>
public partial class DataEdit : DataControlBase
{
    public DataEdit()
    {
        SetCurrentValue(NodeProperty, new DataObject(""));
        InitializeComponent();
    }
}
