---
title: Typography
parent: Native controls
nav_order: 4
---

# Typography

Named `TextBlock` styles form a small typographic scale. A plain `<TextBlock>`
without a style is the default body size; apply a named style through the `Style`
property to give text a role.

## Headings

`H1` down to `H6` step the font size and weight from a page title to a minor
subheading.

```xml
<TextBlock Style="{StaticResource H1}" Text="H1 — The quick brown fox" />
<TextBlock Style="{StaticResource H2}" Text="H2 — The quick brown fox" />
<TextBlock Style="{StaticResource H3}" Text="H3 — The quick brown fox" />
<TextBlock Style="{StaticResource H4}" Text="H4 — The quick brown fox" />
<TextBlock Style="{StaticResource H5}" Text="H5 — The quick brown fox" />
<TextBlock Style="{StaticResource H6}" Text="H6 — The quick brown fox" />
```

## Body text

`Lead` introduces a section with a larger, lighter paragraph. `Muted`
de-emphasizes secondary text at the default size, and `Small` is for fine print,
captions and footnotes.

```xml
<TextBlock Style="{StaticResource Lead}" Text="Lead — a larger, lighter introductory paragraph." />
<TextBlock Text="Default — the standard body text size used across the app." />
<TextBlock Style="{StaticResource Muted}" Text="Muted — secondary text, dimmed to sit quietly next to primary content." />
<TextBlock Style="{StaticResource Small}" Text="Small — fine print, captions and footnotes." />
```
