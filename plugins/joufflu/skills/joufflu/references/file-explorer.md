# File explorer (`Joufflu.FileExplorer`)

Needs `Joufflu.Feedback` too (sources report errors as toasts).

```xml
xmlns:fileExplorer="clr-namespace:Joufflu.FileExplorer.Controls;assembly=Joufflu.FileExplorer"
xmlns:base="clr-namespace:Joufflu.FileExplorer.Controls.Base;assembly=Joufflu.FileExplorer"
xmlns:data="clr-namespace:Joufflu.FileExplorer.Data;assembly=Joufflu.FileExplorer"
xmlns:converters="clr-namespace:Joufflu.FileExplorer.Converters;assembly=Joufflu.FileExplorer"
```

```csharp
using Joufflu.FileExplorer.Sources;   // IExplorerSource, FileSystemSource, FileSystemWatcherSource
using Joufflu.FileExplorer.Data;      // IExplorerNode, IExplorerDirectory, IExplorerFile, FileSystemFile, FileSystemDirectory
```

## Controls

All bind a `Source` (`IExplorerSource`). Controls sharing one source stay in sync
(selecting in the tree, double-clicking in the list or using the breadcrumb opens the
folder for all).

| Control | Shows |
|---|---|
| `Explorer` | Everything: control bar (breadcrumb), tree, list, status bar. Has `ExtraColumns` (passed to its list) |
| `ExplorerList` | Nodes of the opened folder (folders first, natural order, size column). `SelectedNodes` bindable, `View` = sorted/filtered nodes, `ExtraColumns` |
| `ExplorerTree` | Loaded hierarchy; folders only by default. Select opens, double-click expands |
| `ExplorerControlBar` | Breadcrumb/navigation bar alone |

`VisibleNodes`: `All`, `Files`, `Directories` (tree default `Directories`, list default `All`).

```xml
<fileExplorer:Explorer Source="{Binding Source}" />

<fileExplorer:ExplorerList Source="{Binding Source}" VisibleNodes="Files">
    <fileExplorer:ExplorerList.ExtraColumns>
        <GridViewColumn Header="Full path" Width="260" DisplayMemberBinding="{Binding Path}" />
    </fileExplorer:ExplorerList.ExtraColumns>
</fileExplorer:ExplorerList>
```

Built in: per-node-type context menus, drag and drop (between controls and with Windows
Explorer), shortcuts `F2` rename, `Ctrl+C`/`Ctrl+X`/`Ctrl+V`, `Delete`.

## Sources

```csharp
// toasts: an IToastService (null accepted)
public IExplorerSource Source { get; } = new FileSystemSource(@"C:\Projects", toasts);
await Source.Open();   // must be called to load the root
```

- `FileSystemSource`: snapshot of a folder (loads two levels ahead). Copy/move/rename/delete
  go through the Windows shell (progress window, conflict prompt, recycle bin). External
  changes appear when the folder is reopened.
- `FileSystemWatcherSource`: same, watching the disk live. It is `IDisposable`: dispose it
  (e.g. when the page closes) to stop watching.
- `IExplorerSource`: `Root`, `Current`, `Open()`, `Open(node)`,
  `Transfer(paths, target, isMove)`, and the commands `RenameCommand`, `RemoveCommand`,
  `CreateDirectoryCommand`, `CopyCommand`, `CutCommand`, `PasteCommand`, `OpenCommand`,
  `OpenInExplorerCommand`, `CopyPathCommand`, `OpenWithDefaultCommand`. Implement it for a
  non-disk source.

`FileSystemSource` extension points (protected virtual): `OpenDirectory`,
`LoadDirectory(directory, depth)`, `CreateNode(FileSystemInfo, parent)`, `OpenFile`, plus
`public virtual Open(node)`.

## Nodes

`IExplorerNode`: `Name`, `Path`, `ModifiedAt`, `Parent` (`IExplorerDirectory?`).
`IExplorerDirectory` adds `Children`; `IExplorerFile` adds `Size`. Disk implementations:
`FileSystemFile`, `FileSystemDirectory`.

Custom (virtual) nodes: implement `IExplorerNode` (or the file/directory interfaces) and add
them from an overridden `LoadDirectory`:

```csharp
public class VirtualFilesSource : FileSystemSource
{
    public VirtualFilesSource(string root, IToastService? toasts) : base(root, toasts) { }

    protected override void LoadDirectory(IExplorerDirectory directory, int depth)
    {
        base.LoadDirectory(directory, depth);
        directory.Children.Add(new VirtualFile(directory, "notes.md"));
    }
}
```

## Templates and context menus

Node visuals are implicit `DataTemplate`s keyed by node type; redeclare one in a control's
`Resources` to replace it there only. Context menus are `DataTemplate`s keyed with
`{base:ContextMenuTemplateKey local:MyNode}`; their `DataContext` is an
`ExplorerMenuContext` (`Source`, `Node`, `Nodes`). Lookup goes from the most specific type
up to `object`, so types without a menu fall back to the defaults.

```xml
<fileExplorer:Explorer Source="{Binding Source}">
    <fileExplorer:Explorer.Resources>
        <!-- System icons instead of font icons -->
        <DataTemplate DataType="{x:Type data:FileSystemFile}">
            <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="4">
                <Image Width="16" Height="16"
                       Source="{Binding Converter={x:Static converters:ExplorerIconConverter.Small}}" />
                <TextBlock Text="{Binding Name}" />
            </StackPanel>
        </DataTemplate>

        <DataTemplate x:Key="{base:ContextMenuTemplateKey local:VirtualFile}">
            <ContextMenu>
                <MenuItem Header="Copy path" Command="{Binding Source.CopyPathCommand}"
                          CommandParameter="{Binding Node}" />
            </ContextMenu>
        </DataTemplate>
    </fileExplorer:Explorer.Resources>
</fileExplorer:Explorer>
```

In a shared `ExtraColumns` cell, show type-specific content with a `DataTrigger` on
`{Binding Converter={StaticResource NodeTypeConverter}}` (`conv:TypeConverter`) against
`{x:Type local:VirtualFile}`.
