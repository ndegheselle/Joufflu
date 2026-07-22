---
title: Overlays
parent: Navigation
nav_order: 2
---

# Overlays

## Modal overlays

Modal content shown above the page: a title bar with a close cross, a content
area and an optional action bar. Multiple overlays stack.

The overlay content owns its buttons and closes itself via the service (e.g.
`overlays.CloseTop(true/false)`). `Show` returns the result the content closed
with.

```csharp
// The overlay content owns its buttons and closes itself
// via the service, e.g. overlays.CloseTop(true/false).
var content = new DeleteConfirmViewModel(overlays, "Delete?");
var options = new OverlayOptions { Title = "Please confirm" };
bool? result = await overlays.Show(content, options);
```

`OverlayOptions` exposes `Title` and `CloseOnClickAway` (set `false` to force the
user through the action buttons).
