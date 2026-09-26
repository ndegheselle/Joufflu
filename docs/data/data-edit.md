---
title: Data edit
parent: Data
nav_order: 2
---

# Data edit

## Build from scratch

`DataEdit` builds a node freely: add properties and array items of any type, rename keys
and set values. A key must be unique within its object.

```xml
<data:DataEdit Node="{Binding Node}" />
```

```csharp
public DataObject Node { get; set; } = new("");
// ...
Json = Node.ToToken()?.ToString();
```

- **Add** on an object picks the type of the new property, named `key`, `key 1`, `key 2`…
- **Add** on an array picks the type of the new item, or clones its `Template` when it has one.
- Every row can be removed.

Without a bound `Node`, `DataEdit` starts from an empty root object. `ManualValues` works as
in [Data fill](data-fill.md).
