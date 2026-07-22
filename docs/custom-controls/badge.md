---
title: Badge
parent: Custom controls
nav_order: 2
---

# Badge

## Variants

A pill themed from the semantic brushes. Set `Variant` (`Default`, `Primary`,
`Secondary`, `Success`, `Info`, `Warning`, `Danger`).

```xml
<controls:Badge>Default</controls:Badge>
<controls:Badge Variant="Primary">Primary</controls:Badge>
<controls:Badge Variant="Success">Active</controls:Badge>
<controls:Badge Variant="Danger">3</controls:Badge>
```

## Sizes

Sized through the inherited `ControlProperties.Size` attached property.

```xml
<controls:Badge Variant="Primary" joufflu:ControlProperties.Size="xs">xs</controls:Badge>
<controls:Badge Variant="Primary" joufflu:ControlProperties.Size="lg">lg</controls:Badge>
```
