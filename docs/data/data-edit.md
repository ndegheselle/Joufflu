---
title: Data edit
parent: Data
nav_order: 2
---

# Data edit

## Build from scratch

`DataEdit` builds a node freely: add properties and array items of any type, rename keys,
set values, remove rows.

```xml
<data:DataEdit Node="{Binding Node}" />
```

```csharp
public DataObject Node { get; set; } = new("");
```

- New properties are named `key`, `key 1`, `key 2`…; a duplicate key is flagged on its text box.
- **Add** on an array picks the item type, or clones its `Template` when it has one.

Without a bound `Node`, it starts from an empty object. `ManualValues` works as in
[Data fill](data-fill.md).
