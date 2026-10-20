---
title: Combo box tags
parent: Inputs
grand_parent: Joufflu controls
nav_order: 4
---

# Combo box tags

## ComboBoxTags

Builds on `ComboBoxSearch` to select several items, shown as removable tags. Set
`AllowAdd="True"` to let users add values not in the source list.

```xml
<inputs:ComboBoxTags AllowAdd="True"
                     ItemsSource="{Binding Countries}"
                     SelectedItems="{Binding SelectedCountries}" />
```

<kbd>Enter</kbd> adds the typed text when `AllowAdd` is set, and <kbd>Backspace</kbd>
on an empty text removes the last tag.

Bind `SelectedItems` to a mutable `IList` (the gallery uses an
`ObservableCollection<object>`; with `AllowAdd` its element type must accept a
`string`). The control edits that list in place. Changes made to its contents from
the view model are not picked up: assign a new collection instead.
