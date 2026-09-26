# Joufflu.Data

**JSON values as themed trees for [Joufflu](https://www.nuget.org/packages/Joufflu).**

Fill a value in against a JSON Schema, or build one from scratch, in a tree instead of a
raw JSON text box. Follows the Joufflu design system and themes with it.

[![Joufflu.Data on NuGet](https://img.shields.io/nuget/v/Joufflu.Data?label=Joufflu.Data&logo=nuget)](https://www.nuget.org/packages/Joufflu.Data)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## What's inside

| Piece | Purpose |
|---|---|
| `DataFill` | Fills in the values of a node built from a schema: the shape is the schema's, only values and array items change. |
| `DataEdit` | Builds a node from scratch: keys, types and values are all edited. |
| `DataObject` / `DataArray` / `DataValue` | The tree both controls edit; `ToToken()` returns its JSON. |
| `ToDataNode()` | Builds the tree an [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) `JsonSchema` describes. |
| `DataManualValue` | What a field can be forced to instead of its editor (plus undefined and null where the schema allows). |

## Getting started

1. Add the package (it pulls in `Joufflu` and `Joufflu.Inputs`):

   ```sh
   dotnet add package Joufflu.Data
   ```

2. Merge the core `Joufflu` resources in `App.xaml` and initialize `ThemeManager` (see the
   [Joufflu package](https://www.nuget.org/packages/Joufflu)). `Joufflu.Data` has nothing more
   to merge.

3. Fill a value in against a schema:

   ```csharp
   using Joufflu.Data.Model;
   using NJsonSchema;
   using DataObject = Joufflu.Data.Model.DataObject; // System.Windows has one too

   Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();
   // ...
   string? json = Node.ToToken()?.ToString();
   ```

   ```xml
   xmlns:data="clr-namespace:Joufflu.Data.Controls;assembly=Joufflu.Data"

   <data:DataFill Node="{Binding Node}" ManualValues="{Binding ManualValues}" />
   ```

   Or build one freely with `<data:DataEdit Node="{Binding Node}" />`.

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/data/>

## Acknowledgments

- [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) and [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [Lucide](https://lucide.dev/) icon font
