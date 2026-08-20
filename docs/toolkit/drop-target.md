---
title: Drop target
parent: Toolkit
nav_order: 4
---

# Drop target

## DropTarget.Command

`DropTarget.Command` makes **any** element a drop target: `AllowDrop` and the four
drag events (`DragEnter`, `DragOver`, `DragLeave`, `Drop`) are handled for you, and
the command is executed with the dropped `IDataObject` as parameter.

The command's `CanExecute` is what decides the accepted data: what it refuses can't
be dropped — the cursor shows a no-drop sign — and never lights the zone up.

```xml
<!-- AllowDrop and the drag events are handled by the behavior -->
<Border joufflu:DropTarget.Command="{Binding DropFilesCommand}"
        BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}">
    <TextBlock VerticalAlignment="Center" Text="Drop .pdf files here" />
</Border>
```

```csharp
// The command's CanExecute is the whole filter: what it refuses can't be dropped
public IRelayCommand DropFilesCommand { get; }
    = new RelayCommand<IDataObject>(DropFiles, CanDropFiles);

private static bool CanDropFiles(IDataObject? data)
{
    string[]? files = data?.GetData(DataFormats.FileDrop) as string[];
    return files?.Length > 0 && files.All(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
}

// Same IDataObject, once the drop landed
private static void DropFiles(IDataObject? data) { ... }
```

{: .note }
> `CanExecute` is called on every mouse move of the drag, so keep it cheap and side
> effect free — look at the paths, not at the files.

## DropTarget.IsDragOver

`IsDragOver` is `true` while **accepted** data hovers the element, which is all a
trigger needs to highlight a valid drop. It inherits, so template and content
children see it too.

```xml
<Border joufflu:DropTarget.Command="{Binding DropFilesCommand}">
    <!-- Background and BorderBrush are styled, not set on the Border, so the trigger can override them -->
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Background" Value="Transparent" />
            <Style.Triggers>
                <!-- True only while accepted data hovers: refused files never highlight -->
                <Trigger Property="joufflu:DropTarget.IsDragOver" Value="True">
                    <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.Primary100Brush}}" />
                    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.PrimaryBrush}}" />
                </Trigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <TextBlock VerticalAlignment="Center" Text="Drop .pdf files here" />
</Border>
```

## DropTarget.Effect

`Effect` is the drop effect reported to the drag source for an accepted drop, which
drives the mouse cursor — `Copy` by default. Data the source doesn't allow this
effect for is rejected, so a `Move` target only accepts drags that can be moved.

```xml
<Border joufflu:DropTarget.Command="{Binding MoveCommand}"
        joufflu:DropTarget.Effect="Move" />
```
