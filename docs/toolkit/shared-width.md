---
title: Shared width
parent: Toolkit
nav_order: 7
---

# Shared width

## SharedWidth.IsShared

`SharedWidth.IsShared` lines up inputs that each get a different amount of room, such as
the rows of a tree at different depths. Every element marked `IsShared` takes the width of
the narrowest one in its group and is right aligned. Their right edges already line up, so
their left edges do too.

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
                <TextBox Grid.Column="1" toolkit:SharedWidth.IsShared="True" Text="{Binding Value}" />
            </Grid>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

`DataFill` and `DataEdit` use it for their editors.

- Only visible members count: the rows of a collapsed node are ignored until it expands.
- A member's room is the layout slot its parent gives it, so give it a stretching cell
  (`*` column, stretched panel). The shared width follows when a member is added or removed
  or the scope is resized.
- The behaviour sets `HorizontalAlignment` and `Width` on each member, so don't set them yourself.
