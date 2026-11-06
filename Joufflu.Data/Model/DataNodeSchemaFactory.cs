using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System.Globalization;

namespace Joufflu.Data.Model;

public static class DataFactory
{
    extension(JsonSchema schema)
    {
        /// <summary>
        /// The node [schema] describes, under [key]: objects and arrays become the tree of their
        /// properties and item template, anything else a <see cref="DataValue"/>. What the schema
        /// says of the node (its title, description, rules and input) is read from it, see
        /// <see cref="Controls.DataForm"/>.
        /// </summary>
        public DataNode ToDataNode(string? key = null)
        {
            DataNode node = NodeOf(schema, key);
            // A property of a type of its own (a reference) is described on the property rather
            // than on its type, whose title is the name of a class.
            node.Title = schema.Title;
            node.Description = schema.Description ?? schema.ActualSchema.Description;
            return node;
        }

        public EnumDataType ToType()
        {
            if (schema.IsEnumeration)
                return EnumDataType.Choice;

            // Ignore the Null flag (nullable types)
            var type = schema.Type & ~JsonObjectType.Null;

            if (type.HasFlag(JsonObjectType.String))
            {
                switch (schema.Format)
                {
                    case JsonFormatStrings.DateTime:
                    case JsonFormatStrings.Date:
                        return EnumDataType.DateTime;
                    case JsonFormatStrings.TimeSpan:
                    case JsonFormatStrings.Duration:
                        return EnumDataType.TimeSpan;
                    default:
                        return EnumDataType.String;
                }
            }
            if (type.HasFlag(JsonObjectType.Integer)) return EnumDataType.Integer;
            if (type.HasFlag(JsonObjectType.Number)) return EnumDataType.Number;
            if (type.HasFlag(JsonObjectType.Boolean)) return EnumDataType.Boolean;
            if (type.HasFlag(JsonObjectType.Array)) return EnumDataType.Array;
            if (type.HasFlag(JsonObjectType.Object)) return EnumDataType.Object;

            // If no type is declared, guess from the schema's structure
            if (schema.Item != null || schema.Items.Count > 0) return EnumDataType.Array;
            return EnumDataType.Object;
        }

        /// <summary>
        /// The choices [schema] offers. <c>x-enumNames</c> is optional and pairs with the values by
        /// position, so a name is only taken where there is one to take.
        /// </summary>
        public IReadOnlyList<DataEnumOption> Options()
        {
            if (!schema.IsEnumeration)
                return [];

            string[] names = [.. schema.EnumerationNames];
            return [.. schema.Enumeration.Select((value, index) =>
            new DataEnumOption(index < names.Length ? names[index] : $"{value}", value))];
        }
    }

    private static DataNode NodeOf(JsonSchema declared, string? key)
    {
        JsonSchema schema = declared.ActualSchema;
        bool isNullable = schema.IsNullable(SchemaType.JsonSchema);

        if (schema.IsObject)
        {
            var node = new DataObject(key)
            {
                Properties = [.. schema.ActualProperties.Select(prop =>
                {
                    DataNode property = prop.Value.ToDataNode(prop.Key);
                    property.IsRequired = prop.Value.IsRequired;
                    return property;
                })],
                IsNullable = isNullable
            };

            return node;
        }
        else if (schema.IsArray)
        {
            return new DataArray(
                key,
                schema.Item?.ToDataNode("template") ?? throw new Exception("Schemas with multiple templates are not supported. Only [Item] is supported not [Items]."))
            {
                IsNullable = isNullable
            };
        }

        return new DataValue(schema.ToType(), key, schema.Options(), isNullable)
        {
            Rules = RulesOf(declared, schema),
            Input = DataInputs.InputOf(ExtensionOf(declared, schema, DataInputs.SchemaKey)),
            FileFilter = ExtensionOf(declared, schema, DataInputs.FileFilterSchemaKey) as string ?? "",
        };
    }

    /// <summary>
    /// The rules of a value, read where they are written : on the property itself, or on the type
    /// it points at.
    /// </summary>
    private static DataRules RulesOf(JsonSchema declared, JsonSchema actual)
    {
        return new DataRules(
            declared.Minimum ?? actual.Minimum,
            declared.Maximum ?? actual.Maximum,
            declared.MinLength ?? actual.MinLength,
            declared.MaxLength ?? actual.MaxLength,
            declared.Pattern ?? actual.Pattern);
    }

    /// <summary>The extension [key] of the property, or of the type it points at, null when neither has it.</summary>
    private static object? ExtensionOf(JsonSchema declared, JsonSchema actual, string key)
    {
        if (declared.ExtensionData?.TryGetValue(key, out object? declaredValue) == true)
            return declaredValue;
        if (actual.ExtensionData?.TryGetValue(key, out object? actualValue) == true)
            return actualValue;
        return null;
    }

    extension(DataNode node)
    {
        /// <summary>
        /// The schema [node] follows, the reverse of <c>ToDataNode</c>: objects and arrays give
        /// the schema of their properties and item template, values their type and options.
        /// </summary>
        public JsonSchema ToJsonSchema() => Fill(new JsonSchema(), node);

        /// <summary>
        /// Fills [node] with the values of [token], the reverse of <c>ToToken</c>: the shape stays
        /// the node's. A node matching one of [manualValues], or one its editor or shape can't hold,
        /// is forced; a property [token] leaves out is forced to undefined when not required.
        /// </summary>
        public void Load(JToken? token, IEnumerable<DataManualValue>? manualValues = null)
            => Load(node, token, [.. manualValues ?? []]);
    }

    extension(JToken token)
    {
        /// <summary>
        /// The node [token] reads as, under [key], for <see cref="Controls.DataEdit"/>: types are
        /// taken from the values, arrays get no template and null reads as a nullable string.
        /// </summary>
        public DataNode ToDataNode(string? key = null)
        {
            switch (token)
            {
                case JObject obj:
                    return new DataObject(key)
                    {
                        Properties = [.. obj.Properties().Select(property => property.Value.ToDataNode(property.Name))]
                    };
                case JArray items:
                    var array = new DataArray(key, null);
                    foreach (JToken item in items)
                        array.Add(item.ToDataNode());
                    return array;
            }

            var value = token.Type switch
            {
                JTokenType.Integer => new DataValue(EnumDataType.Integer, key, []),
                JTokenType.Float => new DataValue(EnumDataType.Number, key, []),
                JTokenType.Boolean => new DataValue(EnumDataType.Boolean, key, []),
                JTokenType.Date => new DataValue(EnumDataType.DateTime, key, []),
                JTokenType.TimeSpan => new DataValue(EnumDataType.TimeSpan, key, []),
                JTokenType.Null => new DataValue(EnumDataType.String, key, [], isNullable: true),
                _ => new DataValue(EnumDataType.String, key, []),
            };
            LoadValue(value, token, []);
            return value;
        }
    }

    extension(JsonReader reader)
    {
        /// <summary>
        /// The node the value [reader] is on (the next one when it is on none yet) reads as, the same
        /// one <c>ToDataNode</c> gives for its token, under [key]. Built straight from the reader: a
        /// large file is never held twice, once as tokens and once as nodes.
        /// </summary>
        public DataNode ReadDataNode(string? key = null)
        {
            // From the token the reader is on, as JToken.ReadFrom does, comments skipped.
            bool isOnValue = reader.TokenType is not (JsonToken.None or JsonToken.Comment) || NextToken(reader);
            if (!isOnValue)
                throw new JsonReaderException("There is no JSON value to read.");

            // The same keys come back on every item of an array: one string per key instead of one per item.
            if (reader is JsonTextReader { PropertyNameTable: null } textReader)
                textReader.PropertyNameTable = new SharedNameTable();

            return ReadNode(reader, key);
        }
    }

    /// <summary>Reads the node [reader] is on, leaving the reader on its last token.</summary>
    private static DataNode ReadNode(JsonReader reader, string? key)
    {
        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                return ReadObject(reader, key);
            case JsonToken.StartArray:
                var array = new DataArray(key, null);
                while (NextToken(reader) && reader.TokenType != JsonToken.EndArray)
                    array.Add(ReadNode(reader, null));
                return array;
            case JsonToken.Undefined:
                return JValue.CreateUndefined().ToDataNode(key);
            default:
                // A short lived token, so a value is read exactly as ToDataNode reads it.
                var token = new JValue(reader.Value);
                return token.ToDataNode(key);
        }
    }

    private static DataObject ReadObject(JsonReader reader, string? key)
    {
        DataNodeCollection properties = [];
        // A key given twice keeps its last value, as JObject does.
        Dictionary<string, int> indexes = new(StringComparer.Ordinal);
        while (NextToken(reader) && reader.TokenType == JsonToken.PropertyName)
        {
            string name = (string)reader.Value!;
            NextToken(reader);
            DataNode property = ReadNode(reader, name);

            if (indexes.TryGetValue(name, out int index))
                properties[index] = property;
            else
            {
                indexes[name] = properties.Count;
                properties.Add(property);
            }
        }

        return new DataObject(key) { Properties = properties };
    }

    /// <summary>Keeps every key it is asked for, where <see cref="DefaultJsonNameTable"/> only knows those added up front.</summary>
    private sealed class SharedNameTable : JsonNameTable
    {
        private readonly DefaultJsonNameTable _names = new();

        public override string Get(char[] key, int start, int length)
            => _names.Get(key, start, length) ?? _names.Add(new string(key, start, length));
    }

    /// <summary>Moves [reader] to its next token that is not a comment, false at the end.</summary>
    private static bool NextToken(JsonReader reader)
    {
        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.Comment)
                return true;
        }
        return false;
    }

    private static void Load(DataNode node, JToken? token, IReadOnlyList<DataManualValue> manualValues)
    {
        // Loading again starts from scratch: what an earlier load forced may not be forced by this one.
        node.IsManual = false;
        node.ManualEntry = null;

        switch (node)
        {
            case DataValue value:
                LoadValue(value, token, manualValues);
                break;
            // The root has no row to leave manual mode from: it is never forced.
            case not DataValue when node.Parent is not null && ForcedEntryOf(node, token, manualValues) is DataManualValue entry:
                Force(node, entry);
                break;
            case DataObject obj:
                foreach (DataNode property in obj.Properties)
                    Load(property, property.Key is null ? null : (token as JObject)?[property.Key], manualValues);
                break;
            case DataArray array when token is JArray items:
                array.Values.Clear();
                foreach (JToken item in items)
                {
                    array.Add(array.Template?.Clone() ?? item.ToDataNode());
                    Load(array.Values[^1], item, manualValues);
                }
                break;
        }
    }

    /// <summary>
    /// What an object or an array is forced to: undefined when [token] leaves it out and it is not
    /// required, the manual value [token] matches, or [token] itself when it is not the node's shape.
    /// </summary>
    private static DataManualValue? ForcedEntryOf(DataNode node, JToken? token, IReadOnlyList<DataManualValue> manualValues)
    {
        if (token is null)
            return node.IsRequired ? null : DataManualValue.Undefined;

        if (MatchOf(node, token, manualValues) is DataManualValue entry)
            return entry;

        bool fits = node is DataObject ? token is JObject : token is JArray;
        return fits ? null : RawOf(token);
    }

    /// <summary>
    /// The first of [manualValues] fitting [node] that writes [token], a pattern being filled with
    /// the name [token] gives it.
    /// </summary>
    private static DataManualValue? MatchOf(DataNode node, JToken token, IReadOnlyList<DataManualValue> manualValues)
    {
        object? raw = (token as JValue)?.Value;
        DataManualValue? match = manualValues.FirstOrDefault(entry => entry.Fits(node.Type) && entry switch
        {
            DataManualPattern pattern => pattern.Matches(raw),
            _ => JToken.DeepEquals(DataValue.TokenOf(entry.Value), token)
        });
        return match is DataManualPattern matched ? matched.Fill((string)raw!) : match;
    }

    /// <summary>[token] as an entry of its own, kept as it is rather than lost.</summary>
    private static DataManualValue RawOf(JToken token) => new(null, token is JValue raw ? raw.Value : token);

    private static void LoadValue(DataValue value, JToken? token, IReadOnlyList<DataManualValue> manualValues)
    {
        if (token is null)
        {
            if (!value.IsRequired)
                Force(value, DataManualValue.Undefined);
            return;
        }

        if (MatchOf(value, token, manualValues) is DataManualValue entry)
            Force(value, entry);
        else if (TryValueOf(value, token, out object? converted))
            value.Value = converted;
        else
            Force(value, RawOf(token));
    }

    private static void Force(DataNode node, DataManualValue entry)
    {
        node.IsManual = true;
        node.ManualEntry = entry;
    }

    /// <summary>[token] as the CLR value the editor of [value] works in, false when it holds none.</summary>
    private static bool TryValueOf(DataValue value, JToken token, out object? result)
    {
        result = null;
        if (token.Type == JTokenType.Null)
            return value.IsNullable;

        try
        {
            switch (value.Type)
            {
                case EnumDataType.String when token.Type == JTokenType.String:
                    result = (string?)token;
                    return true;
                case EnumDataType.Integer when token.Type == JTokenType.Integer:
                    result = (long)token;
                    return true;
                case EnumDataType.Number when token.Type is JTokenType.Float or JTokenType.Integer:
                    result = (decimal)token;
                    return true;
                case EnumDataType.Boolean when token.Type == JTokenType.Boolean:
                    result = (bool)token;
                    return true;
                case EnumDataType.DateTime when token.Type == JTokenType.Date:
                    result = (DateTime)token;
                    return true;
                case EnumDataType.DateTime when token.Type == JTokenType.String:
                    bool isDate = DateTime.TryParse((string?)token, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date);
                    result = date;
                    return isDate;
                case EnumDataType.TimeSpan when token.Type == JTokenType.TimeSpan:
                    result = (TimeSpan)token;
                    return true;
                case EnumDataType.TimeSpan when token.Type == JTokenType.String:
                    bool isTime = TimeSpan.TryParse((string?)token, CultureInfo.InvariantCulture, out TimeSpan time);
                    result = time;
                    return isTime;
                case EnumDataType.Choice:
                    DataEnumOption? option = value.Options.FirstOrDefault(option => JToken.DeepEquals(DataValue.TokenOf(option.Value), token));
                    result = option?.Value;
                    return option is not null;
            }
        }
        catch (OverflowException)
        {
        }

        return false;
    }

    /// <summary>
    /// Fills [schema] from [node]. Properties need a <see cref="JsonSchemaProperty"/>, hence a schema
    /// given rather than created.
    /// </summary>
    private static T Fill<T>(T schema, DataNode node) where T : JsonSchema
    {
        schema.Description = node.Description;

        switch (node)
        {
            case DataObject obj:
                schema.Type = JsonObjectType.Object;
                foreach (DataNode property in obj.Properties)
                {
                    // Same as ToToken: no key writes nothing, a duplicate key takes the last one.
                    if (property.Key is null)
                        continue;
                    schema.Properties[property.Key] = Fill(new JsonSchemaProperty(), property);
                    // Only once added: the property needs its parent to be required.
                    schema.Properties[property.Key].IsRequired = property.IsRequired;
                }
                break;
            case DataArray array:
                schema.Type = JsonObjectType.Array;
                if (array.Template is not null)
                    schema.Item = array.Template.ToJsonSchema();
                break;
            case DataValue value:
                FillValue(schema, value);
                break;
        }

        // An enumeration without a type takes null through its values only.
        if (node.IsNullable && schema.Type != JsonObjectType.None)
            schema.Type |= JsonObjectType.Null;

        return schema;
    }

    private static void FillValue(JsonSchema schema, DataValue value)
    {
        switch (value.Type)
        {
            case EnumDataType.String:
                schema.Type = JsonObjectType.String;
                break;
            case EnumDataType.Integer:
                schema.Type = JsonObjectType.Integer;
                break;
            case EnumDataType.Number:
                schema.Type = JsonObjectType.Number;
                break;
            case EnumDataType.Boolean:
                schema.Type = JsonObjectType.Boolean;
                break;
            case EnumDataType.DateTime:
                schema.Type = JsonObjectType.String;
                schema.Format = JsonFormatStrings.DateTime;
                break;
            case EnumDataType.TimeSpan:
                // ToToken writes the "c" format, not an ISO 8601 duration.
                schema.Type = JsonObjectType.String;
                schema.Format = JsonFormatStrings.TimeSpan;
                break;
            case EnumDataType.Choice:
                schema.Type = TypeOf(value.Options);
                foreach (DataEnumOption option in value.Options)
                {
                    schema.Enumeration.Add(option.Value);
                    schema.EnumerationNames.Add(option.Name);
                }
                break;
        }
    }

    /// <summary>The type all the non null [options] share, none if they differ.</summary>
    private static JsonObjectType TypeOf(IReadOnlyList<DataEnumOption> options)
    {
        JsonObjectType[] types = [.. options
            .Where(option => option.Value is not null)
            .Select(option => option.Value switch
            {
                string => JsonObjectType.String,
                bool => JsonObjectType.Boolean,
                int or long or short or byte => JsonObjectType.Integer,
                float or double or decimal => JsonObjectType.Number,
                _ => JsonObjectType.None
            })
            .Distinct()];

        return types.Length == 1 ? types[0] : JsonObjectType.None;
    }
}