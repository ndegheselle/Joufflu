---
title: Shared width
parent: Toolkit
nav_order: 7
---

# Shared width

## SharedWidth

`SharedWidth` lines up inputs that each get a different amount of room, such as the rows of
a tree at different depths. Wrap each input in a `SharedWidth`: it stretches over its cell
and gives the input the width of the narrowest member of its group, against its right edge.
Their right edges already line up, so their left edges do too.

Each group is the closest ancestor marked `SharedWidth.IsScope`:

```xml
<TreeView toolkit:SharedWidth.IsScope="True" ItemsSource="{Binding Fields}">
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Children}">
            <Grid toolkit:Spacing.Gap="4">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>
                <TextBlock VerticalAlignment="Center" Text="{Binding Name}" />
                <toolkit:SharedWidth Grid.Column="1">
                    <TextBox Text="{Binding Value}" />
                </toolkit:SharedWidth>
            </Grid>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

`DataFill` and `DataEdit` use it for their editors.

- Only visible members count: the rows of a collapsed node are ignored until it expands.
- A member's room is the width its parent arranges it at, so give it a stretching cell
  (`*` column, stretched panel). The shared width follows when a member is added or removed
  or the scope is resized.
- The shared width only changes how the input is laid out, never the size the member asks
  for. A scope sized to its content (a centered dialog, an `Auto` column, a horizontal
  scroll) stays stable instead of bouncing between two widths.
