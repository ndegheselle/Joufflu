namespace Joufflu.Data.Controls;

/// <summary>
/// Fills in a <see cref="DataControlBase.Node"/> built from a schema as a form : a labelled field
/// per property, objects as groups of fields and arrays as lists of items, rather than as a tree.
/// It reads what the schema says of each field (see <see cref="Model.DataFactory"/>) :
/// <list type="bullet">
/// <item>its <c>title</c> as its label, its key otherwise, and its <c>description</c> under it ;</item>
/// <item>the input it is typed with : a file picker or a text of several lines (<see cref="Model.DataInputs"/>),
/// the editor of its type otherwise ;</item>
/// <item>its rules (<c>minimum</c>, <c>maximum</c>, <c>minLength</c>, <c>maxLength</c>, <c>pattern</c>),
/// what breaks them shown on its editor by <c>ValidationErrorTemplate</c> (<see cref="Model.DataValue.Error"/>,
/// reported on <c>Value</c>).</item>
/// </list>
/// A field offers to be forced to a manual value only when the host gives one that fits it : a
/// form with no <see cref="DataControlBase.ManualValues"/> only edits values.
/// </summary>
public partial class DataForm : DataControlBase
{
    public DataForm()
    {
        InitializeComponent();
    }
}
