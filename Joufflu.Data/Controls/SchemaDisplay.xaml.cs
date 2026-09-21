using Joufflu.Data.Model;
using NJsonSchema;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Controls;

/// <summary>
/// Logique d'interaction pour SchemaDisplay.xaml
/// <para>
/// A schema shown as the tree it describes — what each field is called, what it has to be, and what
/// is asked of it — read only: <see cref="DataEdit"/> is where a value is filled in against one.
/// </para>
/// </summary>
public partial class SchemaDisplay : UserControl
{
    public static readonly DependencyProperty SchemaProperty = DependencyProperty.Register(
        nameof(Schema),
        typeof(JsonSchema),
        typeof(SchemaDisplay),
        new PropertyMetadata(null, (d, _) => ((SchemaDisplay)d).Refresh()));

    /// <summary>The schema being shown, null while there is none.</summary>
    public JsonSchema? Schema
    {
        get => (JsonSchema?)GetValue(SchemaProperty);
        set => SetValue(SchemaProperty, value);
    }

    /// <summary>
    /// The schema read as a tree: the root node, and nothing while there is no schema. Held as a
    /// collection because that is what the tree is fed.
    /// </summary>
    public ObservableCollection<SchemaNode> Nodes { get; } = [];

    public SchemaDisplay()
    {
        InitializeComponent();
    }

    private void Refresh()
    {
        Nodes.Clear();
        if (Schema is JsonSchema schema)
            Nodes.Add(schema.ToSchemaNode());
    }
}
