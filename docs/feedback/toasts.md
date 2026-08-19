---
title: Toasts
parent: Feedback
nav_order: 3
---

# Toasts

## Toast types

Toasts stack in a corner of the window, always above page content and overlays, and
auto-dismiss after a few seconds unless sticky. Show them from an injected
`IToastService` (in `Joufflu.Feedback.Controls`, from the
[`Joufflu.Feedback`](https://www.nuget.org/packages/Joufflu.Feedback) package).

```csharp
// using Joufflu.Feedback.Controls; — inject IToastService
toasts.Info("A neutral message.", "Heads up");
toasts.Success("Saved.");
toasts.Warning("Careful.");
toasts.Error("Failed.");
toasts.Show(new ToastOptions { Message = "Sticky", Duration = TimeSpan.Zero });
```

A `Duration` of `TimeSpan.Zero` makes a toast sticky: it stays until the user
closes it.

## Where toasts are hosted

A `ToastContainer` bound to the same `ToastService` renders them. It wraps the
content it sits above, so wrapping the whole window keeps the toasts on top of
everything:

```xml
<feedback:ToastContainer Toasts="{Binding Toasts}">
    <!-- the whole app -->
</feedback:ToastContainer>
```

When the app also uses an
[`OverlayContainer`](../navigation/overlays.md#where-overlays-are-hosted), wrap it
*inside* the `ToastContainer` so toasts stay above the modal overlays:

```xml
<feedback:ToastContainer Toasts="{Binding Toasts}">
    <nav:OverlayContainer Overlays="{Binding Overlays}">
        <!-- the whole app -->
    </nav:OverlayContainer>
</feedback:ToastContainer>
```

## Position

`Position` picks the corner the stack sits in: `TopRight` (the default), `TopLeft`,
`BottomRight` or `BottomLeft`. The newest toast always sits closest to that corner,
so the stack grows away from it.

```xml
<feedback:ToastContainer Toasts="{Binding Toasts}" Position="BottomRight">
    <!-- the whole app -->
</feedback:ToastContainer>
```

Bind it to change corners at runtime — the gallery's **Toasts** page does exactly
that.
