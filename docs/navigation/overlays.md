---
title: Overlays
parent: Navigation
nav_order: 2
---

# Overlays

## Modal overlays

Overlays are modal content shown above the page. They have a title bar with a
close cross, a content area and an optional action bar. Multiple overlays stack.

The overlay content owns its buttons and closes itself via the service (for
example `overlays.CloseTop(true/false)`). `Show` returns the result the content
closed with.

```csharp
// The overlay content owns its buttons and closes itself
// via the service, e.g. overlays.CloseTop(true/false).
var content = new DeleteConfirmViewModel(overlays, "Delete?");
var options = new OverlayOptions { Title = "Please confirm" };
bool? result = await overlays.Show(content, options);
```

`OverlayOptions` exposes a `Title` and `CloseOnClickAway` (set it to `false` to
force the user to use the action buttons).
