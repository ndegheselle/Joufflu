---
title: Typography
parent: Native controls
nav_order: 1
---

# Typography

There are no named text styles: a `TextBlock` takes its size from the same
`Sizing.Size` the controls use, and muted text is dimmed with a token rather than
given a colour of its own. A plain `<TextBlock>` is the default body size.

## Sizes

A `TextBlock` takes its font size from `Sizing.Size`, like the controls: `xs` to
`xl` read the `FontSize` [tokens](../themes/tokens.md#font-sizes), `md` keeps the
inherited body size. `Sizing.Size` is inherited, so a panel set to a size sizes
the text inside it too.

```xml
<TextBlock toolkit:Sizing.Size="xl" Text="xl" />
<TextBlock toolkit:Sizing.Size="lg" Text="lg" />
<TextBlock Text="md, the inherited body size" />
<TextBlock toolkit:Sizing.Size="sm" Text="sm" />
<TextBlock toolkit:Sizing.Size="xs" Text="xs" />
```

## Headings

A heading is a size and a weight: `xl` for a page title, `lg` for a section, the
body size for a minor heading.

```xml
<TextBlock toolkit:Sizing.Size="xl" FontWeight="Bold" Text="Page title" />
<TextBlock toolkit:Sizing.Size="lg" FontWeight="SemiBold" Text="Section" />
<TextBlock FontWeight="SemiBold" Text="Minor heading" />
```

## Muted text

Secondary text (hints, descriptions, captions) is dimmed with the `MutedOpacity`
token rather than given a colour of its own, so it follows whatever foreground it
sits in.

```xml
<TextBlock Text="Default body text" />
<TextBlock Opacity="{DynamicResource {x:Static joufflu:Colors.MutedOpacity}}"
           Text="Muted text" />
```

{: .note }
> The sizes come from an implicit `TextBlock` style. Inside a control template of
> your own, set `FontSize` from the token directly
> (`{DynamicResource {x:Static joufflu:Dimensions.FontSizeLg}}`).
