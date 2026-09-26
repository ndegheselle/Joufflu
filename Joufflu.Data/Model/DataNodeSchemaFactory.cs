using NJsonSchema;

namespace Joufflu.Data.Model;

public static class DataFactory
{
    extension(JsonSchema schema)
    {
        /// <summary>
        /// The node [schema] describes. Whether it is required is the parent's to say — a schema
        /// lists the properties it requires, so a property cannot read it off itself — hence
        /// [isRequired], passed down as the tree is built.
        /// </summary>
        public DataNode ToDataNode(string? key = null)
        {
            schema = schema.ActualSchema;
            bool isNullable = schema.IsNullable(SchemaType.JsonSchema);

            if (schema.IsObject)
            {
                var node = new DataObject(key)
                {
                    Properties = [.. schema.ActualProperties.Select(prop => prop.Value.ToDataNode(prop.Key))],
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

            return new DataValue(schema.ToType(), key, schema.Options()) { IsNullable = isNullable };
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
}