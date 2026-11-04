---
title: File picker
parent: Inputs
grand_parent: Joufflu controls
nav_order: 6
---

# File picker

## FilePicker

Selects a file through the system dialog and exposes the chosen path, with a clear
button. The path can also be typed.

```xml
<inputs:FilePicker FilePath="{Binding FilePath, Mode=TwoWay}" />
```

## File types

`Options` takes a `FilePickerOptions`: `Filter` lists the file types the dialog
offers, in `OpenFileDialog.Filter` syntax, and `DefaultExtension` is the extension it
appends to a typed name.

```xml
<inputs:FilePicker FilePath="{Binding ImagePath}" Options="{Binding ImageOptions}" />
```

```csharp
public FilePickerOptions ImageOptions { get; } = new()
{
    Filter = "Images (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*",
    DefaultExtension = ".png"
};
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
