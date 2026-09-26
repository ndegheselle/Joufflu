# Joufflu.Data

**JSON values as themed trees for [Joufflu](https://www.nuget.org/packages/Joufflu).**

Fill a value in against a JSON Schema, or build one from scratch, instead of typing raw JSON.

[![Joufflu.Data on NuGet](https://img.shields.io/nuget/v/Joufflu.Data?label=Joufflu.Data&logo=nuget)](https://www.nuget.org/packages/Joufflu.Data)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## What's inside

| Piece | Purpose |
|---|---|
| `DataFill` | Fills in the values of a node built from a schema. |
| `DataEdit` | Builds a node from scratch. |
| `DataObject` / `DataArray` / `DataValue` | The edited tree; `ToToken()` returns its JSON. |
| `ToDataNode()` | Builds the tree of an [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) `JsonSchema`. |
| `DataManualValue` | A value a field can be forced to instead of its editor. |

## Getting started

1. Add the package (it pulls in `Joufflu` and `Joufflu.Inputs`):

   ```sh
   dotnet add package Joufflu.Data
   ```

2. Set up the core [Joufflu](https://www.nuget.org/packages/Joufflu) package (`Resources.xaml`
   and `ThemeManager`). There is nothing more to merge.

3. Build a node from a schema and bind it:

   ```csharp
   using Joufflu.Data.Model;
   using NJsonSchema;
   using DataObject = Joufflu.Data.Model.DataObject; // System.Windows has one too

   Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();
   string? json = Node.ToToken()?.ToString();
   ```

   ```xml
   xmlns:data="clr-namespace:Joufflu.Data.Controls;assembly=Joufflu.Data"

   <data:DataFill Node="{Binding Node}" />
   ```

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/data/>

## Acknowledgments

- [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) and [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [Lucide](https://lucide.dev/) icon font
