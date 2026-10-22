# Joufflu plugin for Claude Code

Skills that teach Claude Code how to build WPF apps with
[Joufflu](https://github.com/ndegheselle/Joufflu): which control to reach for, the right
namespaces and wiring, and how to keep new UI on the design system so it follows theme
changes.

| Skill | Use it to |
|---|---|
| `joufflu` | API reference for every package (natives and named styles, inputs, navigation and overlays, feedback, file explorer, JSON data trees, toolkit attached properties, theming and design tokens). Loaded automatically when a project references Joufflu. |
| `joufflu-new-app` | Scaffold a Joufflu app, or add Joufflu to an existing WPF project: packages, `App.xaml`, `ThemeManager`, a `ThemedWindow` shell with side menu, pages, overlays and toasts. |
| `joufflu-theme` | Create and register a custom palette, override design tokens, add a theme switcher and persist the choice. |
| `joufflu-restyle` | Audit a view and replace hardcoded colours, sizes, margins and hand-made controls with Joufflu tokens, styles and controls. |
| `joufflu-custom-input` | Build an input control of your own that matches the Joufflu inputs: background, border and height tokens, size variants and input padding, states, the validation error style and embedded buttons. |

## Install

From an interactive `claude` terminal:

```
/plugin marketplace add ndegheselle/Joufflu
/plugin install joufflu@joufflu
```

Update later with `/plugin marketplace update joufflu`.

## Maintenance

The skills describe Joufflu's public API. When that API changes, update
`skills/joufflu/references/*.md` (and the workflow skills if they use the changed members)
in the same commit, and bump `version` in `.claude-plugin/plugin.json`. Otherwise Claude
will write code against members that no longer exist.
