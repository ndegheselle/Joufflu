---
title: Combo box tags
parent: Inputs
nav_order: 3
---

# Combo box tags

## ComboBoxTags

Builds on `ComboBoxSearch` to allow selecting several items, shown as removable
tags. Set `AllowAdd="True"` to let users add values that are not in the source
list.

```xml
<inputs:ComboBoxTags AllowAdd="True"
                     ItemsSource="{Binding Countries}"
                     SelectedItems="{Binding SelectedCountries}" />
```
