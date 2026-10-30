---
title: File picker
parent: Inputs
grand_parent: Joufflu controls
nav_order: 6
---

# File picker

## FilePicker

Selects a file through the system dialog and exposes the chosen path, with a clear
button.

```xml
<inputs:FilePicker FilePath="{Binding FilePath, Mode=TwoWay}" />
```

## Validation

Binding errors (here from an ObservableValidator) turn the input's border red, and
`ValidationErrorTemplate` adds a corner badge whose tooltip lists them. Choose a
file to clear it. The second input sets `Validation.ErrorTemplate` to `{x:Null}`:
no badge, only the red border.

```xml
<inputs:FilePicker FilePath="{Binding RequiredFilePath}" />

<!-- Opt out of the template: only the input's own red border remains -->
<inputs:FilePicker Validation.ErrorTemplate="{x:Null}" FilePath="{Binding RequiredFilePath}" />
```
