using Joufflu.Data.Model;
using NJsonSchema;

namespace Joufflu.Data.Tests.Model;

/// <summary>What <c>DataForm</c> reads of a schema : the label, the input and the rules of a field.</summary>
public class FormSchemaTests
{
    private static async Task<DataNode> PropertyOf(string propertySchema)
    {
        JsonSchema schema = await JsonSchema.FromJsonAsync("""{"type":"object","properties":{"Field":""" + propertySchema + "}}");
        return ((DataObject)schema.ToDataNode()).Properties.Single();
    }

    [Test]
    public async Task A_field_is_labelled_by_its_title_then_by_its_key()
    {
        DataNode titled = await PropertyOf("""{"type":"string","title":"Nice name"}""");
        DataNode untitled = await PropertyOf("""{"type":"string"}""");

        Assert.Multiple(() =>
        {
            Assert.That(titled.Label, Is.EqualTo("Nice name"));
            Assert.That(untitled.Label, Is.EqualTo("Field"));
        });
    }

    [Test]
    public async Task A_field_pointing_at_a_type_is_described_by_the_property_first()
    {
        JsonSchema schema = await JsonSchema.FromJsonAsync("""
            {"type":"object",
             "properties":{"Table":{"description":"The table to read","allOf":[{"$ref":"#/definitions/TableRef"}]}},
             "definitions":{"TableRef":{"type":"object","title":"TableRef","description":"A table","properties":{"Id":{"type":"string"}}}}}
            """);

        DataNode table = ((DataObject)schema.ToDataNode()).Properties.Single();

        Assert.Multiple(() =>
        {
            Assert.That(table.Description, Is.EqualTo("The table to read"));
            Assert.That(table.Label, Is.EqualTo("Table"), "the title of a type is the name of a class");
        });
    }

    [TestCase("""{"type":"string","x-input":"file","x-file-filter":"CSV|*.csv"}""", EnumDataInput.File)]
    [TestCase("""{"type":"string","x-input":"multiline"}""", EnumDataInput.Multiline)]
    [TestCase("""{"type":"string"}""", EnumDataInput.Text)]
    public async Task The_input_of_a_string_is_read_from_the_schema(string propertySchema, EnumDataInput expected)
    {
        DataValue field = (DataValue)await PropertyOf(propertySchema);

        Assert.That(field.Input, Is.EqualTo(expected));
    }

    [Test]
    public async Task A_file_keeps_the_filter_of_its_dialog()
    {
        DataValue field = (DataValue)await PropertyOf("""{"type":"string","x-input":"file","x-file-filter":"CSV|*.csv"}""");

        Assert.That(field.FileFilter, Is.EqualTo("CSV|*.csv"));
    }

    [TestCase(0L, "At least 1.")]
    [TestCase(101L, "At most 100.")]
    [TestCase(50L, null)]
    public async Task A_number_out_of_its_range_tells_why(long value, string? expected)
    {
        DataValue field = (DataValue)await PropertyOf("""{"type":"integer","minimum":1.0,"maximum":100.0}""");

        field.Value = value;

        Assert.That(field.Error, Is.EqualTo(expected));
    }

    [TestCase("", "Required.")]
    [TestCase("abcdef", "At most 5 characters.")]
    [TestCase("ab-c", "Doesn't match the expected format (^[a-z]+$).")]
    [TestCase("abc", null)]
    public async Task A_text_breaking_its_rules_tells_why(string value, string? expected)
    {
        DataValue field = (DataValue)await PropertyOf("""{"type":"string","minLength":1,"maxLength":5,"pattern":"^[a-z]+$"}""");

        field.Value = value;

        Assert.That(field.Error, Is.EqualTo(expected));
    }

    [Test]
    public async Task A_forced_field_is_not_checked()
    {
        DataValue field = (DataValue)await PropertyOf("""{"type":"integer","minimum":1.0}""");

        field.IsManual = true;
        field.ManualEntry = new DataManualValue(EnumDataType.Integer, "$previous.Count");

        Assert.That(field.Error, Is.Null);
    }

    [Test]
    public async Task A_copy_keeps_what_the_form_reads()
    {
        DataValue field = (DataValue)await PropertyOf("""{"type":"string","title":"Path","x-input":"file","x-file-filter":"CSV|*.csv","minLength":1}""");

        DataValue copy = field.Clone();

        Assert.Multiple(() =>
        {
            Assert.That(copy.Label, Is.EqualTo("Path"));
            Assert.That(copy.Input, Is.EqualTo(EnumDataInput.File));
            Assert.That(copy.FileFilter, Is.EqualTo("CSV|*.csv"));
            Assert.That(copy.Rules, Is.EqualTo(field.Rules));
        });
    }
}
