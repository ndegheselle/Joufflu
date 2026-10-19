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
}
