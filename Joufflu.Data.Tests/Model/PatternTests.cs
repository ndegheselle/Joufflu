using System.Globalization;
using Joufflu.Data.Converters;
using Joufflu.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Joufflu.Data.Tests.Model;

/// <summary>
/// Manual entries made of a fixed text around a name the user types.
/// </summary>
public class PatternTests
{
    private static readonly DataManualPattern Column = new(null, "$row.columns['{column name}']");

    private static List<DataManualValue> Entries(DataNode node, IEnumerable<DataManualValue> catalog, string? text)
        => (List<DataManualValue>)new DataManualValuesConverter()
            .Convert([node, catalog, text!], typeof(object), null!, CultureInfo.InvariantCulture);

    private static DataValue Required() => new(EnumDataType.String, "a", []) { IsRequired = true };

    [Test]
    public void Untyped_the_pattern_is_a_hint_that_cannot_be_picked()
    {
        List<DataManualValue> entries = Entries(Required(), [Column], "");

        Assert.That(entries, Is.EqualTo(new[] { Column }));
        Assert.That(entries[0].ToString(), Is.EqualTo("$row.columns['column name']"));
        Assert.That(entries[0].IsPickable, Is.False);
    }

    [TestCase("unit price")]
    [TestCase("$row.columns['unit price")]
    [TestCase("$row.columns['unit price']")]
    public void Typed_text_fills_the_pattern_once(string text)
    {
        List<DataManualValue> entries = Entries(Required(), [Column], text);

        Assert.That(entries, Is.EqualTo(new[] { new DataManualValue(null, "$row.columns['unit price']") }));
    }

    [Test]
    public void Typing_the_fixed_text_keeps_the_hint()
        => Assert.That(Entries(Required(), [Column], "$row.col"), Is.EqualTo(new[] { Column }));

    [Test]
    public void Text_of_a_picked_entry_is_not_used_as_a_name()
    {
        var amount = new DataManualValue(null, "$amount");

        Assert.That(Entries(Required(), [amount, Column], "$amount"), Is.EqualTo(new DataManualValue[] { amount, Column }));
    }

    [Test]
    public void A_pattern_only_fills_fields_of_its_type()
    {
        var typed = new DataManualPattern(EnumDataType.Integer, "$row.columns['{column name}']");

        Assert.That(Entries(Required(), [typed], "unit price"), Is.Empty);
    }

    [Test]
    public void A_value_written_by_a_pattern_loads_forced_to_it()
    {
        var tree = new DataObject(null) { Properties = [Required()] };

        tree.Load(JToken.Parse("""{"a":"$row.columns['unit price']"}"""), [Column]);

        DataNode value = tree.Properties[0];
        Assert.That(value.IsManual, Is.True);
        Assert.That(value.ManualEntry, Is.EqualTo(new DataManualValue(null, "$row.columns['unit price']")));
        Assert.That(tree.ToToken()!.ToString(Formatting.None), Is.EqualTo("""{"a":"$row.columns['unit price']"}"""));
    }

    [Test]
    public void A_pattern_needs_a_name_to_fill()
        => Assert.Throws<ArgumentException>(() => _ = new DataManualPattern(null, "$row.columns"));
}
