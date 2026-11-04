---
title: Text editable
parent: Inputs
grand_parent: Joufflu controls
nav_order: 5
---

# Text editable

## TextEditable

Displays a value that turns into an editable text box on demand, for use outside
a form. A double click or the pencil button starts the edit; <kbd>Enter</kbd> or the
check commits it, <kbd>Escape</kbd> or the cross cancels it.

```xml
<inputs:TextEditable Text="{Binding EditableText, Mode=TwoWay}" />
```

`Text` only changes on commit. The `TextChanged` event then reports the new `Text`
and the `OldText`.

## Validation

Binding errors (here from an ObservableValidator) turn the input's border red, and
`ValidationErrorTemplate` adds a corner badge whose tooltip lists them. Shorten
the title to 10 characters to clear it. The second input sets
`Validation.ErrorTemplate` to `{x:Null}`: no badge, only the red border.

```xml
<inputs:TextEditable Text="{Binding ShortText, Mode=TwoWay}" />

<!-- Opt out of the template: only the input's own red text and border remain -->
<inputs:TextEditable Validation.ErrorTemplate="{x:Null}" Text="{Binding ShortText, Mode=TwoWay}" />
```
