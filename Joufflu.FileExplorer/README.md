# Joufflu.FileExplorer

File explorer controls for Joufflu : display the nodes loaded by a `IExplorerLoader` as a list or as a
tree, with per item context menus and the standard file commands.

## Setup

Merge the resources in `App.xaml` (after the `Joufflu` ones), they hold the default node templates and
the default context menus :

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
            <ResourceDictionary Source="pack://application:,,,/Joufflu.FileExplorer;component/Resources.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Controls

Both controls display a root element (`IExplorerFolder`, usually the `Root` of a loader) :

```xml
<fe:TreeExplorer Root="{Binding Loader.Root}" />

<fe:ListExplorer Root="{Binding Loader.Root}" NodeOpened="OnNodeOpened" />
```

| | `TreeExplorer` | `ListExplorer` |
|---|---|---|
| Based on | `TreeView` | `ListBox` |
| Shows | the root and its children | the children of `CurrentFolder` (the root by default) |
| Opening a folder | expands it | navigates into it (`CurrentFolder`) |
| Selection | one node | multiple nodes (`SelectionMode` is `Extended`) |

A node is opened with a double click, the `Enter` key or `ExplorerCommands.Open`. Opening a node that is
not a folder raises `NodeOpened`, it is up to the app to decide what to do with it (start it, show a
preview, ...).

## Nodes templates

Nodes are displayed with the data template matching their type, the default ones are provided for
`ExplorerFile` and `ExplorerFolder` (windows icon + name). A custom node only needs its own template :

```xml
<!--  Custom node  -->
<DataTemplate DataType="{x:Type local:ExplorerProject}">
    <StackPanel Orientation="Horizontal">
        <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Box}" />
        <TextBlock Text="{Binding Name}" />
    </StackPanel>
</DataTemplate>

<!--  Custom node with children, has to be hierarchical to be expandable in the tree  -->
<HierarchicalDataTemplate DataType="{x:Type local:ExplorerSolution}" ItemsSource="{Binding Children}">
    <TextBlock Text="{Binding Name}" />
</HierarchicalDataTemplate>
```

The default templates can be reused (or overriden by redefining their key) :

```xml
<DataTemplate DataType="{x:Type loaders:ExplorerFile}">
    <StackPanel Orientation="Horizontal">
        <ContentPresenter ContentTemplate="{DynamicResource {x:Static fe:ExplorerResources.FileTemplate}}" />
        <jontrols:Badge Content="{Binding Tag}" />
    </StackPanel>
</DataTemplate>
```

Setting `ItemTemplate` (or `ItemTemplateSelector`) on the control still works and takes precedence over
the templates resolved by type.

## Context menus

Menus are resolved every time one is opened, in this order :

1. `ItemContextMenuSelector` : a `ExplorerContextMenuSelector` resolving a menu per node.
2. `SelectionContextMenu` when more than one node is selected (defaults to
   `ExplorerResources.SelectionContextMenu`).
3. `IExplorerContextMenuNode.ContextMenu` : a menu carried by the node itself.
4. `ItemContextMenu` : the same menu for every node of the control.
5. `ExplorerResources.FolderContextMenu` or `ExplorerResources.FileContextMenu`.

Right clicking outside of any node opens the menu of the displayed folder, so that `Paste` and
`New folder` stay reachable.

The opened menu gets the node as `DataContext`, plus `Explorer.Nodes` (every targeted node) and
`Explorer.Owner` (the control) as attached properties :

```xml
<ContextMenu x:Key="ProjectContextMenu">
    <!--  Bound to the node  -->
    <MenuItem Header="Build" Command="{Binding BuildCommand}" />
    <Separator />
    <!--  Standard commands, handled by the explorer control  -->
    <MenuItem Header="Copy" Command="{x:Static fe:ExplorerCommands.Copy}" />
    <MenuItem Header="Delete" Command="{x:Static fe:ExplorerCommands.Delete}" />
</ContextMenu>
```

## Commands

`ExplorerCommands` exposes `Open`, `Cut`, `Copy`, `Paste`, `Delete`, `Rename` and `NewFolder`, with their
usual shortcuts. They can be used in any menu (or anywhere else) without setting a `CommandTarget`, the
explorer sets it when it opens a menu.

`Open` is handled by the controls themselves, every other command is forwarded to the `CommandHandler` of
the control. It defaults to `FileSystemCommandHandler`, which applies them on the file system for the
nodes backed by a path (`IExplorerPathNode`), using the clipboard formats of the windows explorer.
`Rename` has no default implementation since it needs an UI.

Inherit `FileSystemCommandHandler` (every operation is virtual) or implement `IExplorerCommandHandler` to
change what the commands do :

```xml
<fe:ListExplorer Root="{Binding Loader.Root}" CommandHandler="{Binding Commands}" />
```

Nodes are not refreshed after an operation, keeping them up to date is the job of the loader.

## Icons

`SystemIconConverter` converts a path (or a `IExplorerPathNode`) to the icon windows uses for it, with a
cache per extension. It is exposed as `ExplorerResources.SystemIconConverter` and used by the default
templates :

```xml
<Image Source="{Binding Converter={StaticResource {x:Static fe:ExplorerResources.SystemIconConverter}}}" />
```
