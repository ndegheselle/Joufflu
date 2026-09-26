---
title: Data
nav_order: 7
has_children: true
---

# Data

JSON values edited as a tree, from the optional
[`Joufflu.Data`](https://www.nuget.org/packages/Joufflu.Data) package.

- **Data fill** — fills in a value against a JSON Schema.
- **Data edit** — builds a value from scratch.

Add the package (`dotnet add package Joufflu.Data`). Only the core `Resources.xaml` needs
merging. The snippets use:

```xml
xmlns:data="clr-namespace:Joufflu.Data.Controls;assembly=Joufflu.Data"
```

```csharp
using Joufflu.Data.Model;
using NJsonSchema;
using DataObject = Joufflu.Data.Model.DataObject; // System.Windows has one too
```

## Nodes

Both controls edit a `Node`, a `DataObject` root, in place.

| Node | Holds |
|---|---|
| `DataObject` | `Properties`, keyed by unique, case-sensitive keys |
| `DataArray` | `Values`, keyed `[0]`, `[1]`…, and the `Template` new items are cloned from |
| `DataValue` | a `Value`, or the `Options` of an enum |

`ToToken()` returns the JSON:

```csharp
string? json = Node.ToToken()?.ToString();
```

## From a schema

`ToDataNode()` builds the tree of an [NJsonSchema](https://github.com/RicoSuter/NJsonSchema)
`JsonSchema`:

```csharp
Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();
```

| Schema | Editor | Written as |
|---|---|---|
| `string` | `TextBox` | string |
| `string`, format `date` / `date-time` | `DatePicker` | ISO 8601 string |
| `string`, format `time-span` / `duration` | `TimeSpanPicker` | `[d.]hh:mm:ss` string |
| `integer` | `NumericUpDown` | number |
| `number` | `DecimalUpDown` | number |
| `boolean` | `CheckBox` | `true` / `false` |
| `enum` | `ComboBox`, named by `x-enumNames` | the option's value |
| `array` / `object` | tree | array / object |

New values start at `null` when nullable, else at the type's default (`""`, `0`, `false`,
today, the first option).

{: .note }
> Not supported: arrays with a list of item schemas, and schemas without a type.

## To a schema

`ToJsonSchema()` does the reverse: the schema of a tree, built from a schema or with
`DataEdit`, following the table above (time spans get the `time-span` format):

```csharp
JsonSchema schema = Node.ToJsonSchema();
string json = schema.ToJson();
```

Keys, `Description`, `IsNullable`, `IsRequired` and the enum `Options` are kept; values are not.

## Manual values

`ManualValues` lists what a field can be forced to instead of its editor, like a
placeholder resolved later. Each entry names the type it fits (`null` fits any):

```csharp
public IReadOnlyList<DataManualValue> ManualValues { get; } =
[
    new(EnumDataType.String, "TBD"),
    new(EnumDataType.Integer, -1L),
];
```

The feather toggle swaps a field's editor for the entries that fit it, plus **undefined**
(the property is left out) when not required and **null** when nullable. It is disabled
when nothing fits.
