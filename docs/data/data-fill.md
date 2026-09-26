---
title: Data fill
parent: Data
nav_order: 1
---

# Data fill

## Fill in from a schema

`DataFill` edits the values of a node [built from a schema](index.md#from-a-schema): keys
and shape are fixed, only values and array items change.

```xml
<data:DataFill Node="{Binding Node}" ManualValues="{Binding ManualValues}" />
```

- A property's `description` shows as an info tooltip.
- **Add** on an array clones its `Template`; array items can be removed.
- The feather toggle forces a field to one of the [manual values](index.md#manual-values).

To reset, build a new node from the schema.
