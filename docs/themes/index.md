---
title: Themes
nav_order: 4
has_children: true
---

# Themes

The theme layer of the core `Joufflu` package: the window wrapping the app, the
palettes switched at runtime, the tokens a theme is made of and the live editor
generating your own. `ThemeManager` lives in the `Joufflu.Themes` namespace,
`ThemedWindow` in `Joufflu.Controls` and the token keys on `Joufflu.Colors`,
`Joufflu.Brushes` and `Joufflu.Dimensions`.

- **Application shell** — the window styles, and the overlay/toast containers wrapping the app.
- **Theme** — `ThemeManager` for System/Light/Dark plus registering custom themes, and how to bind a theme switcher UI to it.
- **Design tokens** — the reference of every `Colors`, `Brushes` and `Dimensions` key a theme is made of, and the role each one plays in the control styles.
- **Customize theme** — the live theme editor and preset themes.

Snippets use the `joufflu` XML namespace for the design-system keys, plus `nav`
and `feedback` for the application shell containers:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
```
