---
title: Data display
parent: Data
grand_parent: Joufflu controls
nav_order: 3
---

# Data display

## Show a node

`DataDisplay` shows a node read only: the key, the type and the value of each node.

```csharp
Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();
Node.Load(JToken.Parse(json), ManualValues);
```

```xml
<data:DataDisplay Node="{Binding Node}" />
```

- `Node` takes any `DataNode`: an object shows its properties as the first level, an array or a
  value is that level itself.
- Values read the way their editor shows them: the name of a `Choice` option, a date without
  its time when it has none, a time span as `[d.]hh:mm:ss`.
- A value runs on one line: a line break shows as `↵`, a text past 1000 characters is cut.
- **null** and **undefined** are greyed and italic, apart from a text saying "null".
- A value forced to one of the [manual values](index.md#manual-values) is flagged with a
  feather. A forced object or array shows the entry instead of its properties or items.
- A node with a `Description` shows it on an info icon, as in [Data fill](data-fill.md).

The user can only expand and collapse nodes. The display follows the node: values and keys
changed, children added or removed, a node forced or expanded in code show up before the next
render.

## Show a JToken

A `JToken` bound to `Node` is read as `ToDataNode()` does ([From JSON](index.md#from-json)), so a
plain value is shown without building a node first.

```csharp
public JToken Token { get; } = new JValue("Hello");
```

```xml
<data:DataDisplay Node="{Binding Token}" />
```

## Open a file

Read the file with [`ReadDataNode()`](index.md#from-json) off the UI thread, then show the node:

```csharp
// When FilePath changes
using var reader = new JsonTextReader(File.OpenText(FilePath));
Node = await Task.Run(() => reader.ReadDataNode());
```

```xml
<inputs:FilePicker FilePath="{Binding FilePath}" Options="{Binding JsonFileOptions}" />
<data:DataDisplay Node="{Binding Node}" />
```

- The file is never held as tokens too, only as nodes.
- The tree is laid flat in rows of one height, and only the rows in view are built. A file of
  millions of nodes scrolls, expands and collapses as a small one does.
- Give the display a height (a `Grid` row, a fixed `Height`). In a `StackPanel` or a
  `ScrollViewer` it is measured with endless room, and builds every row.

`inputs:FilePicker` is the [file picker](../inputs/file-picker.md) of `Joufflu.Inputs`.
