---
title: Navigation
nav_order: 5
has_children: true
---

# Navigation

Navigation building blocks from `Joufflu.Navigation`:

- **Navigation menu** — a collapsible side menu driven by a `Navigator`.
- **Overlays** — modal dialogs stacked above the current page, plus a standard confirmation.
- **Paging** — a page selector for browsing large sets of data.

The package also ships `FullContainer`, a page host that puts its header in the
window's title bar strip — see
[Application shell](../toolkit/application-shell.md#fullcontainer).

The snippets use the `nav` XML namespace, plus `vm` for your own view models:

```xml
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:vm="clr-namespace:MyApp.ViewModels"
```
