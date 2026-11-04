---
title: Data display
parent: Data
grand_parent: Joufflu controls
nav_order: 3
---

# Data display

## Show a node

`DataDisplay` shows a node read only: the key, the type and the value of each node.

```xml
<data:DataDisplay Node="{Binding Node}" />
```

```csharp
Node = (DataObject)JsonSchema.FromType<Order>().ToDataNode();
Node.Load(JToken.Parse(json), ManualValues);
```

- Values read the way their editor shows them: the name of a `Choice` option, a date without
  its time when it has none, a time span as `[d.]hh:mm:ss`.
- **null**, and **undefined** on a field forced to nothing, are greyed and italic, apart from
  a text saying "null".
- A value forced to one of the [manual values](index.md#manual-values) is flagged with a
  feather. A forced object or array shows the entry instead of its properties or items.
- A node with a `Description` shows it on an info icon, as in [Data fill](data-fill.md).

- `Node` takes any `DataNode`: an object shows its properties as the first level, an array or a
  value is shown as that level itself. A `JToken` can be bound to it directly, read as `ToDataNode` does.
- A value runs on one line: a line break shows as `↵`, a text past 1000 characters is cut.

Only expanding and collapsing nodes changes the tree. The display follows the node: children added
or removed, a node forced or expanded in code show up before the next render.

## Large files

Read the file with `ReadDataNode()` off the UI thread, then show the node:

```csharp
using var reader = new JsonTextReader(File.OpenText(path));
Node = await Task.Run(() => reader.ReadDataNode());
```

- `ReadDataNode()` builds the node straight from the reader, without a `JToken` in between: a
  file is held once, as nodes, and read about a third faster.
- The tree is laid flat in rows of one height, and only the rows in view are built. A file of
  millions of nodes scrolls, expands and collapses as a small one does.
- Give the display a height (a `Grid` row, a fixed `Height`). In a `StackPanel` or a
  `ScrollViewer` it is measured with endless room, and builds every row.
