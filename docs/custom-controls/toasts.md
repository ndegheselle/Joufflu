---
title: Toasts
parent: Custom controls
nav_order: 4
---

# Toasts

## Toast types

Toasts stack in the top-right corner, always above page content and overlays, and
auto-dismiss after a few seconds unless sticky. Show them from an injected
`IToastService`.

```csharp
// Inject IToastService
toasts.Info("A neutral message.", "Heads up");
toasts.Success("Saved.");
toasts.Warning("Careful.");
toasts.Error("Failed.");
toasts.Show(new ToastOptions { Message = "Sticky", Duration = TimeSpan.Zero });
```

A `Duration` of `TimeSpan.Zero` makes a toast sticky: it stays until the user
closes it.
