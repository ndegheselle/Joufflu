# Data (`Joufflu.Data`)

JSON values edited as a tree instead of a raw text box. Depends on `Joufflu`,
`Joufflu.Inputs`, NJsonSchema and Newtonsoft.Json. Nothing to merge beyond the core
`Resources.xaml`.

```xml
xmlns:data="clr-namespace:Joufflu.Data.Controls;assembly=Joufflu.Data"
```

```csharp
using Joufflu.Data.Model;   // DataNode, DataObject, DataArray, DataValue, EnumDataType, DataManualValue, DataFactory
using NJsonSchema;          // JsonSchema
// Always alias it: System.Windows.DataObject clashes in any WPF file.
using DataObject = Joufflu.Data.Model.DataObject;
```

## Controls

Both take a `Node` (`DataObject`, two-way by default) and edit it in place, plus an optional
`ManualValues`. They keep the host's `DataContext`.

| Control | Edits |
|---|---|
| `DataFill` | Values of a node built from a schema. Keys and shape are fixed; only array items can be added (cloned from the array's `Template`) or removed. Shows `Description` as an info tooltip. |
| `DataEdit` | Everything: add properties/items of any `EnumDataType`, rename keys, set values, remove rows. Starts from an empty `new DataObject("")` when no `Node` is bound. |

```xml
<data:DataFill Node="{Binding Node}" ManualValues="{Binding ManualValues}" />
<data:DataEdit Node="{Binding Draft}" />
```

## Building and reading the tree

```csharp
Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();   // any JsonSchema, e.g. from FromJsonAsync
string? json = Node.ToToken()?.ToString();                      // JToken? of the whole tree
```

`ToDataNode(string? key = null)` is a C# 14 extension on `JsonSchema` (class `DataFactory`).
It follows `$ref`, sets `IsNullable` from the schema and `IsRequired` from the parent's
`required` list.

| Schema | `EnumDataType` | Editor | JSON written |
|---|---|---|---|
| `string` | `String` | `TextBox` | string |
| `string` + `date` / `date-time` | `DateTime` | `DatePicker` | ISO 8601 (`"O"`) |
| `string` + `time-span` / `duration` | `TimeSpan` | `TimeSpanPicker` | `"c"` format |
| `integer` | `Integer` (`long`) | `NumericUpDown` | number |
| `number` | `Number` (`decimal`) | `DecimalUpDown` | number |
| `boolean` | `Boolean` | `CheckBox` | bool |
| `enum` | `Choice` | `ComboBox` over `Options` (`DataEnumOption(Name, Value)`, names from `x-enumNames`) | the option's value |
| `array` | `Array` → `DataArray` (`Values`, `Template`) | tree | array |
| `object` | `Object` → `DataObject` (`Properties`) | tree | object |

Limits: tuple arrays (`items` as a list) throw; a schema with no type is not supported.
New values start at `null` when nullable, else `""`, `0`, `false`, today, `TimeSpan.Zero`
or the first option.

Tree API: `DataObject.Add(node)` / `Remove(node)` / `UniqueKey("key")`, `DataArray.Add()`
(clone of `Template`) / `Add(EnumDataType)` / `Remove(node)`, `DataNode.Clone()`. Object keys
are case-sensitive and must be unique (duplicates are reported through
`INotifyDataErrorInfo` on `Key`); array items are keyed `[0]`, `[1]`… and re-numbered on
removal.

## Manual values

A field can be forced to an entry instead of its editor (feather toggle on the row):

```csharp
public IReadOnlyList<DataManualValue> ManualValues { get; } =
[
    new(EnumDataType.String, "TBD"),   // Type = the field type it fits; null fits any
    new(EnumDataType.Integer, -1L),
];
```

On top of these, a row offers `DataManualValue.Undefined` when `!IsRequired` (the property is
not written) and `DataManualValue.Null` when `IsNullable`. A forced entry sets
`DataValue.Value` and is written as the JSON of its own value. The toggle is disabled when
nothing fits.
