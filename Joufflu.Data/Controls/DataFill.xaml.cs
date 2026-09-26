namespace Joufflu.Data.Controls;

/// <summary>
/// Fills in the values of a <see cref="DataControlBase.Node"/>, typically built from a schema with
/// <see cref="Model.DataFactory"/>: the keys and the shape are the schema's, only the values and the
/// array items are edited.
/// </summary>
public partial class DataFill : DataControlBase
{
    public DataFill()
    {
        InitializeComponent();
    }
}
