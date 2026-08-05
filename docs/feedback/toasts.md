---
title: Toasts
parent: Feedback
nav_order: 3
---

# Toasts

## Toast types

Toasts stack in the top-right corner, always above page content and overlays, and
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
