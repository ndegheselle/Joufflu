---
title: Data fill
parent: Data
nav_order: 1
---

# Data fill

## Fill in from a schema

`DataFill` edits the values of a node built from a JSON schema: keys and shape come from
the schema, only values and array items change. The feather toggle forces a field to one
of the [manual values](index.md#manual-values).

```csharp
var schema = JsonSchema.FromType<Order>();
Node = (DataObject)schema.ToDataNode();
```

```xml
<data:DataFill Node="{Binding Node}" ManualValues="{Binding ManualValues}" />
```

```csharp
Json = Node?.ToToken()?.ToString();
```

- Each row shows the type icon, the key and the editor its type calls for.
- A property with a `description` shows an info icon, the description as its tooltip.
- An array gets an **Add** button cloning its item `Template`; array items can be removed.

To start over, build a new node from the schema: values go back to their defaults.
