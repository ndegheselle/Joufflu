---
title: Overlays
parent: Navigation
nav_order: 2
---

# Overlays

Modal content shown above the page: a title bar with a close cross and a content
area. Multiple overlays stack.

## Hosting the overlays

Overlays are rendered by an `OverlayContainer` bound to the `Overlayer` you
show them from. Wrap the whole window content in it so an overlay covers
everything — side menu included:

```xml
<nav:OverlayContainer Overlays="{Binding Overlays}">
    <!-- the whole app -->
</nav:OverlayContainer>
```

The container creates an `Overlayer` of its own when none is bound; bind one from
your shell view model to share it with the pages, behind `IOverlayer`.

Toasts have their own [`ToastContainer`](../feedback/toasts.md); wrap it around
this one to keep them above the overlays.

## Showing an overlay

The overlay content owns its buttons and closes itself through the overlayer, in
one of three ways that tell the caller how it ended:

| Close with | `ShowAsync` returns |
|---|---|
| `overlays.Validate(content)` | `true` |
| `overlays.Cancel(content)` | `false` |
| `overlays.Ignore(content)` | `null` — also what the close cross and a click on the dimmed background do |

Each closes the overlay showing that content, whichever is on top, so content
opening another overlay of its own kind still closes itself.

`OverlayOptions` exposes `Title`, `ShowCloseButton`, `CloseOnClickAway` (set
`false` to force the user through the action buttons) and `FullScreen`.

### Any object

Any object works as content, resolved to its view through an implicit
`DataTemplate`, with the options given at show time:

```csharp
var options = new OverlayOptions { Title = "Edit profile" };
bool? result = await overlays.ShowAsync(content, options);
```

Content implementing `IOverlayContent` carries its own `Options` instead, so
`ShowAsync` can be called without them.

### Awaiting a result

Content implementing `IOverlayContent<TResult>` also exposes the `Result` it was
validated with. Shown through the generic `ShowAsync`, the overlay hands that
result back when validated, and the default of `TResult` when cancelled or
ignored:

```csharp
public partial class SampleFormViewModel : ObservableObject, IOverlayContent<string>
{
    private readonly IOverlayer _overlays;

    public SampleFormViewModel(IOverlayer overlays) => _overlays = overlays;

    public OverlayOptions Options { get; } = new() { Title = "Edit profile", CloseOnClickAway = false };

    [ObservableProperty]
    private string _name = "Joe Doe";

    public string? Result => Name;

    [RelayCommand]
    private void Save() => _overlays.Validate(this);

    [RelayCommand]
    private void Cancel() => _overlays.Cancel(this);
}
```

```csharp
string? name = await overlays.ShowAsync(new SampleFormViewModel(overlays));
if (name != null)
    // saved
```

`IOverlayContent` derives from [`IPage`](navigation-menu.md#page-lifecycle), so content can also react to
being shown and closed through `OnNavigatedTo` / `OnNavigatedFrom`.

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
await overlays.ShowAsync(content, new OverlayOptions { Title = "Edit", FullScreen = true });
```
