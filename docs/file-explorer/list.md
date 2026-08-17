---
title: List
parent: File explorer
nav_order: 2
---

# List

## ExplorerList

Lists the nodes of the opened folder, directories first then by natural name order,
with each file's size. Double click a folder to open it, right click a node for its
menu.

```xml
<fileExplorer:ExplorerList Source="{Binding Source}" />
```

`SelectedNodes` is bindable, so a status bar can count the selection, and `View` gives
the sorted and filtered nodes as the list displays them.

## Extra columns

`ExtraColumns` adds columns after the ones of the list (name, modification date, size).
Each cell is bound to the node of its row, so a node type of your own shows the data it
carries: here the path every node has, and a cell template reading the parent folder.

```xml
<fileExplorer:ExplorerList Source="{Binding Source}">
    <fileExplorer:ExplorerList.ExtraColumns>
        <GridViewColumn Header="Full path" Width="260" DisplayMemberBinding="{Binding Path}" />
        <GridViewColumn Header="In folder">
            <GridViewColumn.CellTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Parent.Name}" />
                </DataTemplate>
            </GridViewColumn.CellTemplate>
        </GridViewColumn>
    </fileExplorer:ExplorerList.ExtraColumns>
</fileExplorer:ExplorerList>
```

`Explorer` has an `ExtraColumns` of its own, handed over to its list.

## Filtering the nodes

`VisibleNodes` chooses the kinds of node shown: `All` by default, or `Files` /
`Directories` to keep only one. Here the list keeps only the files.

```xml
<fileExplorer:ExplorerList Source="{Binding Source}" VisibleNodes="Files" />
```
