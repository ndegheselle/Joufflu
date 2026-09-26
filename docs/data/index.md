---
title: Data
nav_order: 7
has_children: true
---

# Data

JSON values edited as a themed tree instead of a raw text box, from the optional
[`Joufflu.Data`](https://www.nuget.org/packages/Joufflu.Data) package.

- **Data fill** — fills in a value against a JSON Schema: the shape is the schema's, only the values change.
- **Data edit** — builds a value from scratch: keys, types and values are all edited.

Add the package (`dotnet add package Joufflu.Data`, it pulls in `Joufflu.Inputs`). There
is nothing more to merge than the core `Resources.xaml`. The snippets use these
namespaces:

```xml
xmlns:data="clr-namespace:Joufflu.Data.Controls;assembly=Joufflu.Data"
```

```csharp
using Joufflu.Data.Model;
using NJsonSchema;
// System.Windows has a DataObject of its own.
using DataObject = Joufflu.Data.Model.DataObject;
```

## Nodes

Both controls show a `Node`, a `DataObject` root, and edit it in place. A tree is made
of `DataNode`s:

| Node | Holds |
|---|---|
| `DataObject` | `Properties`, each under a unique `Key` |
| `DataArray` | `Values`, keyed `[0]`, `[1]`… and the `Template` new items are cloned from |
| `DataValue` | a `Value`, or the `Options` of a closed list |

Each node has a `Type` (`EnumDataType`), a `Description`, and `IsNullable` /
`IsRequired` as the schema says. `ToToken()` returns the JSON the tree amounts to:

```csharp
string? json = Node.ToToken()?.ToString();
```

## From a schema

`ToDataNode()` builds the tree a [NJsonSchema](https://github.com/RicoSuter/NJsonSchema)
`JsonSchema` describes, so a schema derived from a .NET type works as it is:

```csharp
var schema = JsonSchema.FromType<Order>();
Node = (DataObject)schema.ToDataNode();
```

| Schema | `EnumDataType` | Editor | Written as |
|---|---|---|---|
| `string` | `String` | `TextBox` | string |
| `string`, format `date` / `date-time` | `DateTime` | `DatePicker` | ISO 8601 string |
| `string`, format `time-span` / `duration` | `TimeSpan` | `TimeSpanPicker` | `[d.]hh:mm:ss` string |
| `integer` | `Integer` | `NumericUpDown` | number |
| `number` | `Number` | `DecimalUpDown` | number |
| `boolean` | `Boolean` | `CheckBox` | `true` / `false` |
| `enum` | `Choice` | `ComboBox` of the options | the option's value |
| `array` | `Array` | tree of its items | array |
| `object` | `Object` | tree of its properties | object |

A `$ref` is followed. The option names come from `x-enumNames` when the schema has them.
A new value starts at `null` if the schema is nullable, at the type's default (`""`, `0`,
`false`, today, the first option) otherwise.

{: .note }
> An array must describe its items with a single schema (`items` as an object): a list of
> item schemas throws. A schema declaring no type is not supported either.

## Keys

The keys of an object are case-sensitive and must be unique: a duplicate is flagged on
its text box through `INotifyDataErrorInfo`. Array items are keyed by position and
re-numbered when one is removed.

## Manual values

`ManualValues` lists what a field can be forced to instead of its editor, for example a
placeholder the host resolves later. Each `DataManualValue` names the type it fits
(`null` fits any):

```csharp
public IReadOnlyList<DataManualValue> ManualValues { get; } =
[
    new(EnumDataType.String, "TBD"),
    new(EnumDataType.Integer, -1L),
];
```

The feather toggle of a row swaps its editor for the list of entries that fit it, plus:

- **undefined** when the parent object does not require the field — the property is left out;
- **null** when the schema is nullable.

A forced entry is written as the JSON of its own value. The toggle is disabled when
nothing fits.
