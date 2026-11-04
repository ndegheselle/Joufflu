---
title: Spinner
parent: Feedback
grand_parent: Joufflu controls
nav_order: 2
---

# Spinner

## Loading indicator

An indeterminate loading indicator. Colour comes from `Foreground` (the primary
brush by default), diameter from `Sizing.Size`.

```xml
<feedback:Spinner toolkit:Sizing.Size="xs" />
<feedback:Spinner toolkit:Sizing.Size="sm" />
<feedback:Spinner />
<feedback:Spinner toolkit:Sizing.Size="lg" />
<feedback:Spinner toolkit:Sizing.Size="lg"
                  Foreground="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}" />
```

A collapsed or hidden spinner stops turning, so it can stay in the view and be shown
through `Visibility` while work runs.
