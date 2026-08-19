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
var content = new SampleFormViewModel(overlays);
var options = new OverlayOptions { Title = "Edit profile" };
bool? result = await overlays.Show(content, options);
```

`OverlayOptions` exposes `Title`, `ShowCloseButton`, `CloseOnClickAway` (set
`false` to force the user through the action buttons) and `FullScreen`.

## Standard confirmation

For the common "are you sure?" case, `Confirm` shows a built-in overlay — a
message and a cancel/confirm pair — without any content of your own:

```csharp
bool? result = await overlays.Confirm("Delete the selected item? This action cannot be undone.", "Please confirm");
if (result == true)
    // confirmed
```

The content behind it is `ConfirmationContent`, which is its own `OverlayOptions`.
Build it yourself to change the button texts or the overlay chrome, and pass it as
both the content and the options:

```csharp
var confirmation = new ConfirmationContent(overlays, "Delete the selected item?")
{
    Title = "Delete item",
    ConfirmText = "Delete",
    CloseOnClickAway = false
};

bool? result = await overlays.Show(confirmation, confirmation);
```

An empty `CancelText` hides the cancel button, which turns the overlay into an
acknowledge-only message.

## Where overlays are hosted

Overlays are rendered by the `OverlayContainer` bound to the same
`OverlayService`. Wrap the whole window content in it so an overlay covers
everything — side menu included:

```xml
<nav:OverlayContainer Overlays="{Binding Overlays}">
    <!-- the whole app -->
</nav:OverlayContainer>
```

Toasts have their own [`ToastContainer`](../feedback/toasts.md); wrap it around
this one to keep them above the overlays.

## Full screen

`FullScreen = true` stretches the overlay over the entire container instead of a
centered, sized panel — a whole-window editor or wizard rather than a dialog:

```csharp
await overlays.Show(content, new OverlayOptions { Title = "Edit", FullScreen = true });
```
