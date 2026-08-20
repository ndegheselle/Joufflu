---
title: Overlays
parent: Navigation
nav_order: 2
---

# Overlays

Modal content shown above the page: a title bar with a close cross and a content
area. Multiple overlays stack.

## Hosting the overlays

Overlays are rendered by an `OverlayContainer` bound to the `OverlayService` you
show them from. Wrap the whole window content in it so an overlay covers
everything — side menu included:

```xml
<nav:OverlayContainer Overlays="{Binding Overlays}">
    <!-- the whole app -->
</nav:OverlayContainer>
```

Toasts have their own [`ToastContainer`](../feedback/toasts.md); wrap it around
this one to keep them above the overlays.

## Showing an overlay

The overlay content owns its buttons and closes itself via the service (e.g.
`overlays.CloseTop(true/false)`). `Show` completes when the overlay closes and
returns the result it closed with — `null` when dismissed.

```csharp
var content = new SampleFormViewModel(overlays);
var options = new OverlayOptions { Title = "Edit profile" };
bool? result = await overlays.Show(content, options);
```

`OverlayOptions` exposes `Title`, `ShowCloseButton`, `CloseOnClickAway` (set
`false` to force the user through the action buttons) and `FullScreen`.

Content implementing `IOverlayContent` carries its own `Options`, so `Show` can be
called without them.

## Standard confirmation

For the common "are you sure?" case, `Confirm` shows a built-in overlay — the
message plus a *Cancel* / *Confirm* pair — with no content of your own:

```csharp
bool? result = await overlays.Confirm(
    "Delete the selected item? This action cannot be undone.",
    "Please confirm",
    EnumConfirmationType.Danger);

if (result == true)
    // confirmed
```

`EnumConfirmationType` colours the confirm button in the matching semantic style:
`Neutral` (the default), `Info`, `Success`, `Warning` or `Danger`.

## Full screen

`FullScreen = true` stretches the overlay over the entire container instead of a
centered, sized panel — a whole-window editor or wizard rather than a dialog:

```csharp
await overlays.Show(content, new OverlayOptions { Title = "Edit", FullScreen = true });
```
