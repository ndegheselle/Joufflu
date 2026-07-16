---
title: Search & combo
parent: Inputs
nav_order: 2
---

# Search & combo

## Search

A text box with a built-in delay before it raises `SearchChanged`, to limit calls
to an API or database. <kbd>Escape</kbd> clears it.

```xml
<inputs:Search />
```

```csharp
// code-behind: search.SearchChanged += text => Filter(text);
```

## ComboBoxSearch

An editable combo box that filters its items as you type. Accepts a
`FilterMemberPath` that acts like `DisplayMemberPath` for the filter.

```xml
<inputs:ComboBoxSearch ItemsSource="{Binding Countries}"
                       SelectedItem="{Binding SelectedCountry}" />
```
