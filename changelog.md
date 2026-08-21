# Version 0.5.0

- Add a standard confirm overlay, `IOverlayService.Confirm`, with an `EnumConfirmationType` colouring its confirm button
- Add `DropTarget.Command`, turning any element into a drop target
- Add a `FullContainer` to simplify content placement when using `AllowContentOverTitleBar`
- Support `Paging` without a known total
- Move `Dropdown` from `Joufflu.Navigation` to `Joufflu.Inputs` (namespace `Joufflu.Inputs.Controls`)
- Remove the `Dimensions.BorderThicknessRight` key, now owned by `NavigationMenu` which was its only user

# Version 0.4.0

- Change the navigation to use types instead of string keys
- Split `OverlayContainer` into separate overlay and `ToastContainer` containers
- Move the tooltip into the `Joufflu` core package

# Version 0.2.0

- Add the `Joufflu.FileExplorer` package : `Explorer`, `ExplorerList`, `ExplorerTree` and `ExplorerControlBar` sharing an `IExplorerSource`, with node visuals and context menus keyed on the node type, drag and drop, keyboard shortcuts, and file operations handed over to the Windows shell
- Add the `xl` control size and its `ControlFontSizeXl` dimension
- Size `FontIcon` from the design system instead of a fixed value
- Improve the toasts look, with a progress bar of their remaining duration
- Restyle the native `ListView` and `TreeView` (rounded border, centered cell content)
- Add `MoreVisualTreeHelper.FindSelfOrParent`, and a logical tree fallback to its parent lookup

# Version 0.1.2

- Move `Badge`, `Spinner`, toasts and the tooltip attached properties out of the core `Joufflu` package into a new `Joufflu.Feedback` package (namespace `Joufflu.Feedback.Controls`)

# Version 0.1.1

- Add tooltip
- Add soft and outline button styles
- Improve theme manager custom themes handling
- Improve `ThemedWindow` handling of `AllowContentOverTitleBar`

# Version 0.1.0

- First version