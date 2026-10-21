---
title: Search
parent: Inputs
grand_parent: Joufflu controls
nav_order: 2
---

# Search

A text box that waits for typing to settle (300 ms) before reporting the search,
to limit calls to an API or database. <kbd>Escape</kbd> clears it.

```xml
<!-- MVVM: SearchText is two-way by default and only updates once typing settles -->
<inputs:Search SearchText="{Binding Query}" />
<!-- or a command, executed with the text -->
<inputs:Search SearchCommand="{Binding SearchCommand}" />
```

```csharp
// code-behind: search.SearchChanged += text => Filter(text);
```
