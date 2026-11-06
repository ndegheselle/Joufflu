---
title: Data form
parent: Data
grand_parent: Joufflu controls
nav_order: 4
---

# Data form

## A form from a schema

`DataForm` fills in a node [built from a schema](index.md#from-a-schema), like
[`DataFill`](data-fill.md), but as a form rather than a tree: a labelled field per property,
objects as groups of fields, arrays as lists of items.

```xml
<data:DataForm Node="{Binding Node}" ManualValues="{Binding ManualValues}" />
```

Each field reads what the schema says of it:

| Schema | Field |
|---|---|
| `title` (`[Display(Name = ...)]`) | Its label, the key otherwise. |
| `description` (`[Description]`) | Under the label. |
| `required` (`[Required]`) | A `*` after the label. |
| `minimum`, `maximum` (`[Range]`) | Checked as it is typed: "At least 1.", "At most 20." |
| `minLength`, `maxLength`, `pattern` | Checked the same way: "Required.", "At most 5 characters.", "Doesn't match the expected format (...)". |
| `x-input: file`, `x-file-filter` | A `FilePicker`, its dialog showing the filter. |
| `x-input: multiline` | A text box of several lines. |

The `x-input` keys have no JSON Schema equivalent. NJsonSchema writes them from attributes:

```csharp
public class Shipment
{
    [Display(Name = "Customer name")]
    [Description("Who receives the parcel.")]
    [Required]
    public string Customer { get; set; } = "";

    [JsonSchemaExtensionData(DataInputs.SchemaKey, DataInputs.File)]
    [JsonSchemaExtensionData(DataInputs.FileFilterSchemaKey, "PDF files|*.pdf|All files|*.*")]
    public string DeliveryNote { get; set; } = "";

    [Range(1, 20)]
    public int Parcels { get; set; } = 1;
}
```

- The broken rule of a value is `DataValue.Error`, null when it breaks none. A forced value is
  never checked.
- **Add** on an array clones its `Template`. Each item, forced or not, has its own remove button.
- A field offers to be forced only to the [manual values](index.md#manual-values) that fit it:
  `null` and undefined alone don't clutter a form. With no `ManualValues`, the form only edits
  values. A field forced already keeps its toggle, to bring its editor back.
- The form scrolls on its own: give it a height.
