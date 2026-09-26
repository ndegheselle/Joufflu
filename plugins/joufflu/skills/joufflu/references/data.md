# Data (`Joufflu.Data`)

JSON values edited as a tree. Depends on `Joufflu`, `Joufflu.Inputs`, NJsonSchema and
Newtonsoft.Json. Only the core `Resources.xaml` needs merging.

```xml
xmlns:data="clr-namespace:Joufflu.Data.Controls;assembly=Joufflu.Data"
```

```csharp
using Joufflu.Data.Model;   // DataObject, DataArray, DataValue, EnumDataType, DataManualValue
using NJsonSchema;          // JsonSchema
using DataObject = Joufflu.Data.Model.DataObject; // always alias: clashes with System.Windows.DataObject
```

## Controls

Both edit a `Node` (`DataObject`, two-way) in place and take an optional `ManualValues`.

| Control | Edits |
|---|---|
| `DataFill` | Values of a node built from a schema. Shape is fixed; only array items can be added (cloned from `Template`) or removed. |
| `DataEdit` | Everything: properties and items of any type, keys, values, string options of a `Choice`. Starts from an empty object when no `Node` is bound. |

```xml
<data:DataFill Node="{Binding Node}" ManualValues="{Binding ManualValues}" />
<data:DataEdit Node="{Binding Draft}" />
```

## Tree

```csharp
Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode(); // any JsonSchema
string? json = Node.ToToken()?.ToString();
```

`ToDataNode()` follows `$ref` and sets `IsNullable` and `IsRequired` from the schema.
`node.ToJsonSchema()` does the reverse (types, formats, `Description`, `IsNullable`,
`IsRequired`, enum `Options` with `x-enumNames`; values are not written, time spans use `time-span`).

| Schema | `EnumDataType` | Editor | JSON |
|---|---|---|---|
| `string` | `String` | `TextBox` | string |
| `string` + `date` / `date-time` | `DateTime` | `DatePicker` | ISO 8601 |
| `string` + `time-span` / `duration` | `TimeSpan` | `TimeSpanPicker` | `"c"` format |
| `integer` | `Integer` (`long`) | `NumericUpDown` | number |
| `number` | `Number` (`decimal`) | `DecimalUpDown` | number |
| `boolean` | `Boolean` | `CheckBox` | bool |
| `enum` | `Choice` | `ComboBox` of `Options` (`DataEnumOption(Name, Value)`) | option value |
| `array` | `Array` → `DataArray` (`Values`, `Template`) | tree | array |
| `object` | `Object` → `DataObject` (`Properties`) | tree | object |

Not supported: arrays with a list of item schemas, schemas without a type.

Nullable values start at `null`, others at `""`, `0`, `false`, today, `TimeSpan.Zero` or
the first option. Object keys are unique and case-sensitive (errors on `Key` through
`INotifyDataErrorInfo`); array items are keyed `[0]`, `[1]`…

API: `DataObject.Add` / `Remove` / `UniqueKey`, `DataArray.Add()` / `Add(EnumDataType)` /
`Remove`, `DataValue.AddOption(string)` / `RemoveOption`, `DataNode.Clone()`.

## Manual values

A field can be forced to an entry instead of its editor (feather toggle):

```csharp
public IReadOnlyList<DataManualValue> ManualValues { get; } =
[
    new(EnumDataType.String, "TBD"),   // fits String fields; null type fits any
    new(EnumDataType.Integer, -1L),
];
```

Also offered: `DataManualValue.Undefined` (property not written) when not required,
`DataManualValue.Null` when nullable.
