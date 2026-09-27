---
title: Data display
parent: Data
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

Only expanding and collapsing nodes changes the tree.
