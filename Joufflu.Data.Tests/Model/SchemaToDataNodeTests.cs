using Joufflu.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Joufflu.Data.Tests.Model;

public class SchemaToDataNodeTests
{
    private static async Task<DataObject> TreeOf(string json)
    {
        JsonSchema schema = await JsonSchema.FromJsonAsync(json);
        return (DataObject)schema.ToDataNode();
    }

    private static async Task<DataNode> PropertyOf(string propertySchema)
    {
        DataObject tree = await TreeOf($$"""{"type":"object","properties":{"Field":{{propertySchema}}}}""");
        return tree.Properties.Single();
    }

    [Test]
    public async Task A_property_without_type_becomes_an_empty_value()
    {
        DataObject tree = await TreeOf("""{"type":"object","properties":{"Left":{}}}""");

        DataValue left = (DataValue)tree.Properties.Single();
        Assert.That(left.Value, Is.Null);
    }

    [Test]
    public async Task A_property_without_type_loads_whatever_it_is_given()
    {
        DataObject tree = await TreeOf("""{"type":"object","properties":{"Left":{},"Right":{}}}""");

        tree.Load(JToken.Parse("""{"Left":"$row.amount","Right":10}"""), []);

        string written = tree.ToToken()!.ToString(Formatting.None);
        Assert.That(written, Is.EqualTo("""{"Left":"$row.amount","Right":10}"""));
    }

    [TestCase("""{"type":"string","format":"date-time"}""", EnumDataType.DateTime)]
    [TestCase("""{"type":"string","format":"date"}""", EnumDataType.DateTime)]
    [TestCase("""{"type":"string","format":"duration"}""", EnumDataType.TimeSpan)]
    [TestCase("""{"type":"string","format":"time-span"}""", EnumDataType.TimeSpan)]
    [TestCase("""{"type":"string","format":"email"}""", EnumDataType.String)]
    public async Task A_string_format_picks_the_editor(string propertySchema, EnumDataType expected)
    {
        DataNode field = await PropertyOf(propertySchema);

        Assert.That(field.Type, Is.EqualTo(expected));
    }

    [Test]
    public async Task A_nullable_type_keeps_its_type_and_starts_null()
    {
        DataValue field = (DataValue)await PropertyOf("""{"type":["integer","null"]}""");

        Assert.That(field.Type, Is.EqualTo(EnumDataType.Integer));
        Assert.That(field.IsNullable, Is.True);
        Assert.That(field.Value, Is.Null);
    }

    [Test]
    public async Task An_enum_becomes_a_choice_named_by_position_falling_back_to_the_value()
    {
        DataValue field = (DataValue)await PropertyOf("""{"enum":["r","g","b"],"x-enumNames":["Red","Green"]}""");

        Assert.That(field.Type, Is.EqualTo(EnumDataType.Choice));
        Assert.That(field.Options.Select(option => option.Name), Is.EqualTo(new[] { "Red", "Green", "b" }));
    }

    [Test]
    public async Task A_required_property_is_marked_required()
    {
        DataObject tree = await TreeOf(
            """{"type":"object","required":["Name"],"properties":{"Name":{"type":"string"},"Note":{"type":"string"}}}""");

        Assert.That(tree.Properties.Select(property => property.IsRequired), Is.EqualTo(new[] { true, false }));
    }

    [Test]
    public async Task An_array_takes_its_items_schema_as_template()
    {
        DataArray field = (DataArray)await PropertyOf("""{"type":"array","items":{"type":"integer"}}""");

        Assert.That(field.Template?.Type, Is.EqualTo(EnumDataType.Integer));
        Assert.That(field.Values, Is.Empty);
    }

    [Test]
    public async Task A_tree_survives_a_round_trip_through_json_schema()
    {
        DataObject tree = await TreeOf("""
            {
              "type": "object",
              "required": ["Name"],
              "properties": {
                "Name": { "type": "string" },
                "Age": { "type": ["integer", "null"] },
                "Born": { "type": "string", "format": "date-time" },
                "Wait": { "type": "string", "format": "time-span" },
                "Colour": { "enum": ["r", "g"], "x-enumNames": ["Red", "Green"] },
                "Tags": { "type": "array", "items": { "type": "string" } }
              }
            }
            """);

        DataObject again = (DataObject)tree.ToJsonSchema().ToDataNode();

        Assert.That(Shape(again), Is.EqualTo(Shape(tree)));
        DataValue colour = (DataValue)again.Properties.Single(property => property.Key == "Colour");
        Assert.That(colour.Options.Select(option => option.Name), Is.EqualTo(new[] { "Red", "Green" }));
        DataArray tags = (DataArray)again.Properties.Single(property => property.Key == "Tags");
        Assert.That(tags.Template?.Type, Is.EqualTo(EnumDataType.String));
    }

    private static IEnumerable<(string?, EnumDataType, bool, bool)> Shape(DataObject tree)
        => tree.Properties.Select(property => (property.Key, property.Type, property.IsNullable, property.IsRequired));
}
