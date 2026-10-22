---
title: Tree
parent: File explorer
grand_parent: Joufflu controls
nav_order: 2
---

# Tree

## ExplorerTree

Shows the loaded hierarchy, each level sorted on its own so it stays independent from
the list. A tree shows only folders by default. Selecting a folder opens it, and so
does expanding it, which selects it; a double click expands it.

```xml
<fileExplorer:ExplorerTree Source="{Binding Source}" />
```

## Showing files too

`VisibleNodes` chooses the kinds of node a control shows: `Directories` (the tree
default), `Files`, or `All`. Set it to `All` to list the files alongside the folders.

```xml
<fileExplorer:ExplorerTree Source="{Binding Source}" VisibleNodes="All" />
```
