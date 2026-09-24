# Feedback (`Joufflu.Feedback`)

```xml
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
```

```csharp
using Joufflu.Feedback;            // ToastService, IToastService, ToastOptions, ToastType
using Joufflu.Feedback.Controls;   // ToastContainer, ToastPosition, Badge, BadgeVariant, Spinner
```

## Badge

Pill in the semantic colours. `Variant`: `Default`, `Primary`, `Secondary`, `Success`,
`Info`, `Warning`, `Danger`. Sized by `toolkit:Sizing.Size`.

```xml
<feedback:Badge Variant="Success">Active</feedback:Badge>
<feedback:Badge Variant="Danger" toolkit:Sizing.Size="xs" Content="{Binding UnreadCount}" />
```

## Spinner

Indeterminate loading indicator; colour from `Foreground`, diameter from `Sizing.Size`.

```xml
<feedback:Spinner toolkit:Sizing.Size="lg"
                  Visibility="{Binding IsLoading, Converter={x:Static conv:VisibilityConverter.Default}}" />
```

## Toasts

Host once, around the whole app (and around the `OverlayContainer` if any):

```xml
<feedback:ToastContainer Toasts="{Binding Toasts}" Position="BottomRight">
    <!-- the whole app -->
</feedback:ToastContainer>
```

`Position` (`ToastPosition`): `TopRight` (default), `TopLeft`, `BottomRight`, `BottomLeft`.
The newest toast sits closest to the corner.

Own one `ToastService` in the shell view model and inject it as `IToastService`:

```csharp
_toasts.Info("A neutral message.", "Heads up");   // (message, title = "")
_toasts.Success("Saved.");
_toasts.Warning("Careful.");
_toasts.Error("Failed to save.", "Error");
_toasts.Show(new ToastOptions { Message = "Sticky", Type = ToastType.Info, Duration = TimeSpan.Zero });
```

- Default duration 5 s; `TimeSpan.Zero` = sticky until closed.
- Each call returns a `ToastInstance`; `_toasts.Close(instance)` removes it early.
- `ToastType`: `Info`, `Success`, `Warning`, `Danger` (`Error(...)` shows a `Danger` toast).

Use toasts for transient outcomes ("Saved", "Copy failed"); use an overlay (`Confirm`) when
the user must decide.
