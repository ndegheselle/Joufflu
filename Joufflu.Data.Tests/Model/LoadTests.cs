using Joufflu.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Joufflu.Data.Tests.Model;

/// <summary>
/// Filling a tree built from a schema with a JSON value, and writing it back.
/// </summary>
public class LoadTests
{
    private const string Schema = """
        {
          "type": "object",
          "required": ["Count"],
          "properties": {
            "Count": { "type": "integer" },
            "Price": { "type": "number" },
            "Done": { "type": "boolean" },
            "Born": { "type": "string", "format": "date-time" },
            "Wait": { "type": "string", "format": "time-span" },
            "Colour": { "enum": ["red", "green"] },
            "Note": { "type": "string" },
            "Tags": { "type": "array", "items": { "type": "string" } }
          }
        }
        """;

    private DataObject _tree = null!;

    [SetUp]
    public async Task SetUp()
    {
        JsonSchema schema = await JsonSchema.FromJsonAsync(Schema);
        _tree = (DataObject)schema.ToDataNode();
    }

    private DataNode Property(string key) => _tree.Properties.Single(property => property.Key == key);
    private DataValue Value(string key) => (DataValue)Property(key);
    private string Written() => _tree.ToToken()!.ToString(Formatting.None);

    [Test]
    public void Fills_each_value_in_the_type_of_its_editor()
    {
        _tree.Load(JToken.Parse("""
            {"Count":3,"Price":1.5,"Done":true,"Born":"2024-05-01T10:00:00","Wait":"01:30:00","Colour":"green"}
            """));

        Assert.That(Value("Count").Value, Is.EqualTo(3L));
        Assert.That(Value("Price").Value, Is.EqualTo(1.5m));
        Assert.That(Value("Done").Value, Is.EqualTo(true));
        Assert.That(Value("Born").Value, Is.EqualTo(new DateTime(2024, 5, 1, 10, 0, 0)));
        Assert.That(Value("Wait").Value, Is.EqualTo(new TimeSpan(1, 30, 0)));
        Assert.That(Value("Colour").Value, Is.EqualTo("green"));
    }

    [Test]
    public void Fills_an_array_from_its_template_keyed_by_index()
    {
        _tree.Load(JToken.Parse("""{"Count":0,"Tags":["a","b"]}"""));

        DataArray tags = (DataArray)Property("Tags");
        Assert.That(tags.Values.Select(item => item.Key), Is.EqualTo(new[] { "[0]", "[1]" }));
        Assert.That(tags.Values.Select(item => ((DataValue)item).Value), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void A_value_matching_a_manual_value_is_forced_to_it()
    {
        var reference = new DataManualValue(null, "$row.amount");

        _tree.Load(JToken.Parse("""{"Count":"$row.amount"}"""), [reference]);

        Assert.That(Value("Count").IsManual, Is.True);
        Assert.That(Value("Count").ManualEntry, Is.EqualTo(reference));
    }

    [Test]
    public void A_value_its_editor_cannot_hold_is_kept_as_it_is()
    {
        _tree.Load(JToken.Parse("""{"Count":"many"}"""));

        Assert.That(Value("Count").IsManual, Is.True);
        Assert.That(Written(), Does.Contain("\"Count\":\"many\""));
    }

    [Test]
    public void A_left_out_optional_property_is_undefined_and_writes_nothing()
    {
        _tree.Load(JToken.Parse("""{"Count":1}"""));

        Assert.That(Value("Note").ManualEntry, Is.EqualTo(DataManualValue.Undefined));
        Assert.That(Written(), Does.Not.Contain("Note"));
    }

    [Test]
    public void A_left_out_required_property_keeps_its_default()
    {
        _tree.Load(JToken.Parse("{}"));

        Assert.That(Value("Count").IsManual, Is.False);
        Assert.That(Value("Count").Value, Is.EqualTo(0L));
    }

    [Test]
    public void Loading_again_drops_what_an_earlier_load_forced()
    {
        _tree.Load(JToken.Parse("""{"Count":1}"""));

        _tree.Load(JToken.Parse("""{"Count":1,"Note":"hello"}"""));

        Assert.That(Value("Note").IsManual, Is.False);
        Assert.That(Value("Note").Value, Is.EqualTo("hello"));
    }

    [Test]
    public void Dates_and_durations_written_load_back_the_same()
    {
        var born = new DateTime(2024, 5, 1, 10, 0, 0);
        var wait = new TimeSpan(1, 30, 0);
        Value("Born").Value = born;
        Value("Wait").Value = wait;

        JToken written = _tree.ToToken()!;
        _tree.Load(written);

        Assert.That(Value("Born").Value, Is.EqualTo(born));
        Assert.That(Value("Wait").Value, Is.EqualTo(wait));
    }

    [Test]
    public void Json_without_a_schema_reads_as_the_types_of_its_values()
    {
        DataObject tree = (DataObject)JToken.Parse("""{"a":1,"b":1.5,"c":true,"d":null,"e":"x"}""").ToDataNode();

        Assert.That(tree.Properties.Select(property => property.Type), Is.EqualTo(new[]
        {
            EnumDataType.Integer, EnumDataType.Number, EnumDataType.Boolean, EnumDataType.String, EnumDataType.String,
        }));
        Assert.That(((DataValue)tree.Properties[3]).Value, Is.Null);
    }
}
