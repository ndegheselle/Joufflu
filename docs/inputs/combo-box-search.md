---
title: Search combo box
parent: Inputs
grand_parent: Joufflu controls
nav_order: 3
---

# Search combo box

## ComboBoxSearch

An editable combo box that filters its items as you type, on the text
`DisplayMemberPath` shows for each item.

The chevron on the right opens the list, as do <kbd>Up</kbd> and <kbd>Down</kbd>.
Opening it that way shows every choice, even once an item is selected : the text
left behind by a selection is not a search, so it does not filter the list down to
the item already picked.

```xml
<inputs:ComboBoxSearch ItemsSource="{Binding Countries}"
                       SelectedItem="{Binding SelectedCountry}" />
```

## Validation

Binding errors (here from an ObservableValidator) turn the input's border red, and
`ValidationErrorTemplate` adds a corner badge whose tooltip lists them. Pick a
country to clear it. The second input sets `Validation.ErrorTemplate` to
`{x:Null}`: no badge, only the red border.

```xml
<inputs:ComboBoxSearch ItemsSource="{Binding Countries}"
                       SelectedItem="{Binding RequiredCountry}" />

<!-- Opt out of the template: only the input's own red border remains -->
<inputs:ComboBoxSearch Validation.ErrorTemplate="{x:Null}" ... />
```
