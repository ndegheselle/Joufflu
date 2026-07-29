---
title: Paging
parent: Navigation
nav_order: 3
---

# Paging

## Paging

Page selector for a set of `Total` items. It exposes `PageNumber` and `Capacity`
(items per page), and the range currently displayed. Pages are elided with a
separator when there are more than seven of them.

```xml
<nav:Paging Total="123" />
```

| Property | Type | Default | Purpose |
|---|---|---|---|
| `Total` | `int` | `-1` | Number of items in the whole set. `-1` hides the range label and leaves the page count unbounded. |
| `PageNumber` | `int` | `1` | Current page, 1-based. Clamped between `1` and the last page. |
| `Capacity` | `int` | `10` | Items per page, picked from `AvailableCapacities` (`5, 10, 25, 50, 100, 200` by default). |
| `PageMax` | `int` (read-only) | — | Last page, derived from `Total` and `Capacity`. |
| `IntervalMin` / `IntervalMax` | `int` (read-only) | — | Bounds of the range currently displayed, shown as `1-10 of 123`. |

## Paged data grid

Paging only tells you which slice to display, it never touches the data itself.
Bind `Total` to the number of items in the whole set, then bind `PageNumber` and
`Capacity` two way so the view model is notified of every change and can load the
matching slice.

```xml
<DataGrid ItemsSource="{Binding PageItems}" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Id" Binding="{Binding Id}" />
        <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
    </DataGrid.Columns>
</DataGrid>
<nav:Paging
    Total="{Binding Total}"
    PageNumber="{Binding PageNumber, Mode=TwoWay}"
    Capacity="{Binding Capacity, Mode=TwoWay}" />
```

```csharp
// The view model reacts to any paging change from its own setters.
public int PageNumber
{
    get => _pageNumber;
    set { if (SetProperty(ref _pageNumber, value)) UpdatePage(); }
}

public int Capacity
{
    get => _capacity;
    set { if (SetProperty(ref _capacity, value)) UpdatePage(); }
}

private void UpdatePage()
{
    // Query only the current page (Skip/Take, SQL OFFSET/FETCH, API page parameters, …)
    PageItems.Clear();
    foreach (var item in _source.Skip((PageNumber - 1) * Capacity).Take(Capacity))
        PageItems.Add(item);
}
```

{: .note }
> `Mode=TwoWay` is required: neither `PageNumber` nor `Capacity` binds two way by
> default, so without it the control would never push the new page back to the
> view model.

## Reacting without bindings

For code-behind, the control raises `PagingChange` with the new page number and
capacity every time either changes:

```csharp
paging.PagingChange += (pageNumber, capacity) => UpdatePage(pageNumber, capacity);
```
