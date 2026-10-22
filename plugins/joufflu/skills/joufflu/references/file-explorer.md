# File explorer (`Joufflu.FileExplorer`)

Needs `Joufflu.Feedback` too (sources report errors as toasts).

```xml
xmlns:fileExplorer="clr-namespace:Joufflu.FileExplorer.Controls;assembly=Joufflu.FileExplorer"
xmlns:base="clr-namespace:Joufflu.FileExplorer.Controls.Base;assembly=Joufflu.FileExplorer"
xmlns:fileSystem="clr-namespace:Joufflu.FileExplorer.FileSystem;assembly=Joufflu.FileExplorer"
xmlns:icons="clr-namespace:Joufflu.FileExplorer.Icons;assembly=Joufflu.FileExplorer"
```

```csharp
using Joufflu.FileExplorer.Nodes;       // IExplorerSource, IExplorerNode, IExplorerDirectory, IExplorerFile
using Joufflu.FileExplorer.FileSystem;  // FileSystemSource, FileSystemWatcherSource, FileSystemFile, FileSystemDirectory
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
public IExplorerSource Source { get; }
// in the constructor, where toasts is in scope
Source = new FileSystemSource(@"C:\Projects", toasts);
await Source.Open();   // must be called to load the root
```

- `FileSystemSource`: snapshot of a folder (loads two levels ahead). Copy/move/rename/delete
  go through the Windows shell (progress window, conflict prompt, recycle bin). External
  changes appear when the folder is reopened.
- `FileSystemWatcherSource`: same, watching the disk live. It is `IDisposable`: dispose it
  (e.g. when the page closes) to stop watching.
- `IExplorerSource`: `Root`, `Current`, `Open()`, `Open(node)`,
  `Transfer(paths, target, isMove)`, and plain operations `Rename(node, name)`,
  `Remove(nodes)`, `CreateDirectory(parent)`, `Copy(nodes)`, `Cut(nodes)`, `Paste(target)`,
  `CanPaste()` — no commands, the controls build them. Implement it for a non-disk source.

`FileSystemSource` extension points (protected virtual): `OpenDirectory`,
`ApplyDirectory(directory, DirectoryRead)`, `CreateNode(FileSystemInfo, parent)`, `OpenFile`, plus
`public virtual Open(node)`. Opening reads the disk off the UI thread (`ReadDirectory`), then
calls `ApplyDirectory` on the UI thread for the directory and each sub directory read with it.

## Nodes

`IExplorerNode`: `Name`, `Path`, `ModifiedAt`, `Parent` (`IExplorerDirectory?`).
`IExplorerDirectory` adds `Children` and `DirectoryTree` (itself and its ancestors, for the
breadcrumb); `IExplorerFile` adds `Size`. Disk implementations:
`FileSystemFile`, `FileSystemDirectory`.

Custom (virtual) nodes: implement `IExplorerNode` (or the file/directory interfaces) and add
them from an overridden `ApplyDirectory`:

```csharp
public class VirtualFilesSource : FileSystemSource
{
    public VirtualFilesSource(string root, IToastService? toasts) : base(root, toasts) { }

    protected override void ApplyDirectory(IExplorerDirectory directory, DirectoryRead read)
    {
        base.ApplyDirectory(directory, read);
        directory.Children.Add(new VirtualFile(directory, "notes.md"));
    }
}
```

## Templates and context menus

Node visuals are implicit `DataTemplate`s keyed by node type; redeclare one in a control's
`Resources` to replace it there only. Context menus are `DataTemplate`s keyed with
`{base:ContextMenuTemplateKey local:MyNode}`; their `DataContext` is an
`ExplorerMenuContext` (`Source`, `Node`, `Nodes`, and the item commands `OpenCommand`,
`CopyCommand`, `CutCommand`, `PasteCommand`, `RemoveCommand`, `CreateDirectoryCommand`,
`BeginRenameCommand`, plus the disk-only `OpenInExplorerCommand`, `OpenWithDefaultCommand`,
`CopyPathCommand`). Lookup goes from the
most specific type up to `object`; the built-in single-node menus are keyed on
`FileSystemDirectory`/`FileSystemFile`, so a single node of a custom type shows no menu until
you key one on its type (multi-selection and empty-area menus are keyed on `object`).

```xml
<fileExplorer:Explorer Source="{Binding Source}">
    <fileExplorer:Explorer.Resources>
        <!-- System icons instead of font icons -->
        <DataTemplate DataType="{x:Type fileSystem:FileSystemFile}">
            <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="4">
                <Image Width="16" Height="16"
                       Source="{Binding Converter={x:Static icons:ExplorerIconConverter.Small}}" />
                <TextBlock Text="{Binding Name}" />
            </StackPanel>
        </DataTemplate>

        <DataTemplate x:Key="{base:ContextMenuTemplateKey local:VirtualFile}">
            <ContextMenu>
                <MenuItem Header="Copy path" Command="{Binding CopyPathCommand}"
                          CommandParameter="{Binding Node}" />
            </ContextMenu>
        </DataTemplate>
    </fileExplorer:Explorer.Resources>
</fileExplorer:Explorer>
```

In a shared `ExtraColumns` cell, show type-specific content with a `DataTrigger` on
`{Binding Converter={StaticResource NodeTypeConverter}}` (`conv:TypeConverter`) against
`{x:Type local:VirtualFile}`.
