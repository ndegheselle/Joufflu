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

`OverlayOptions` exposes `Title`, `ShowCloseButton`, `CloseOnClickAway` (set
`false` to force the user through the action buttons) and `FullScreen`.

## Where overlays are hosted

Overlays are rendered by the `OverlayContainer` bound to the same
`OverlayService`. Wrap the whole window content in it so an overlay covers
everything — side menu included:

```xml
<nav:OverlayContainer Overlays="{Binding Overlays}" Toasts="{Binding Toasts}">
    <!-- the whole app -->
</nav:OverlayContainer>
```

## Full screen

`FullScreen = true` stretches the overlay over the entire container instead of a
centered, sized panel — a whole-window editor or wizard rather than a dialog:

```csharp
await overlays.Show(content, new OverlayOptions { Title = "Edit", FullScreen = true });
```
