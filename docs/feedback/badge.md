---
title: Badge
parent: Feedback
grand_parent: Joufflu controls
nav_order: 1
---

# Badge

## Variants

A pill themed from the semantic brushes. Pick a variant with the `Variant`
property (`Default` when unset).

```xml
<feedback:Badge>Default</feedback:Badge>
<feedback:Badge Variant="Primary">Primary</feedback:Badge>
<feedback:Badge Variant="Secondary">Secondary</feedback:Badge>
<feedback:Badge Variant="Success">Success</feedback:Badge>
<feedback:Badge Variant="Info">Info</feedback:Badge>
<feedback:Badge Variant="Warning">Warning</feedback:Badge>
<feedback:Badge Variant="Danger">Danger</feedback:Badge>
```

## Sizes

Sized through the inherited `Sizing.Size` attached property.

```xml
<feedback:Badge Variant="Primary" toolkit:Sizing.Size="xs">xs</feedback:Badge>
<feedback:Badge Variant="Primary" toolkit:Sizing.Size="sm">sm</feedback:Badge>
<feedback:Badge Variant="Primary">md</feedback:Badge>
<feedback:Badge Variant="Primary" toolkit:Sizing.Size="lg">lg</feedback:Badge>
```
