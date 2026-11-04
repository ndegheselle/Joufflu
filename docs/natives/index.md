---
title: Native controls
nav_order: 5
has_children: true
---

# Native controls

WPF's built-in controls, restyled to match the design system. No special types
needed — a plain `<Button>`, `<TextBox>` or `<ComboBox>` picks up the themed look
as soon as the styles are merged (see [Getting started](../index.md#getting-started)).

This section covers the core visuals and the button variants:

- **Typography** — text sizes, muted text and the `TextMuted` / `Title` styles.
- **Font icon** — the bundled Lucide icon font, sized like the text, and the `Chevron` of expandable headers.
- **Card** — the themed container for grouped content.
- **Buttons** — the solid, soft, outline and ghost variants in the semantic colours, and their toggle button twins.

The other natives (text box, combo box, check box, list, tree, data grid, tabs,
menus…) are restyled transparently and need nothing beyond the merged styles; they
are shown in the **Native controls** group of the **`Joufflu.Samples`** gallery
rather than here. A few of them have a named style or a setting worth knowing:

```xml
<!-- Multi-line text: accepts Return, takes the height it is given, scrolls instead of wrapping -->
<TextBox Height="120" Style="{StaticResource MultilineTextBox}" />

<!-- A destructive menu item -->
<MenuItem Header="Delete" Style="{StaticResource DangerMenuItem}" />

<!-- A long list: scroll by item and only build the items in view -->
<ListBox ScrollViewer.CanContentScroll="True" ItemsSource="{Binding Items}" />
```

Binding errors turn an input's border red and add a corner badge listing them, the
`ValidationErrorTemplate`; see [Create a custom control](../toolkit/custom-control.md)
to give an input of your own the same.
