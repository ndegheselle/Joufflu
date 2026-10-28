using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Themes;
using Microsoft.Win32;
using JBrushes = Joufflu.Brushes;
using JColors = Joufflu.Colors;
using JDimensions = Joufflu.Dimensions;

namespace Joufflu.Samples.Views.Themes;

/// <summary>A single editable theme colour, wired to a live resource key.</summary>
public class ThemeColorEntry : ObservableObject
{
    private readonly Action<ThemeColorEntry> _onChanged;

    public string Label { get; }

    /// <summary>The <c>Joufflu.Colors</c> accessor name, e.g. <c>PrimaryColor</c>.</summary>
    public string ResourceName { get; }

    public ComponentResourceKey Key { get; }

    private Color _color;
    public Color Color
    {
        get => _color;
        set
        {
            if (SetProperty(ref _color, value))
                _onChanged(this);
        }
    }

    public ThemeColorEntry(string label, string resourceName, ComponentResourceKey key, Color color, Action<ThemeColorEntry> onChanged)
    {
        Label = label;
        ResourceName = resourceName;
        Key = key;
        _color = color;
        _onChanged = onChanged;
    }

    /// <summary>Sets the colour without re-applying it to resources (used when seeding/resetting).</summary>
    public void SetColorSilently(Color color) => SetProperty(ref _color, color, nameof(Color));
}

/// <summary>A single editable numeric dimension (radius, spacing, height, font size…).</summary>
public class ThemeDimensionEntry : ObservableObject
{
    private readonly Action<ThemeDimensionEntry> _onChanged;

    public string Label { get; }

    /// <summary>Identifier used to route the value to the right resource key(s).</summary>
    public string ResourceName { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public string Unit { get; }

    private double _value;
    public double Value
    {
        get => _value;
        set
        {
            // Snap to whole numbers — every dimension here is an integer count of DIPs.
            double snapped = Math.Round(value);
            if (SetProperty(ref _value, snapped))
                _onChanged(this);
        }
    }

    public ThemeDimensionEntry(string label, string resourceName, double min, double max, double value, Action<ThemeDimensionEntry> onChanged, string unit = "px")
    {
        Label = label;
        ResourceName = resourceName;
        Minimum = min;
        Maximum = max;
        _value = value;
        _onChanged = onChanged;
        Unit = unit;
    }

    public void SetValueSilently(double value) => SetProperty(ref _value, Math.Round(value), nameof(Value));
}

/// <summary>
/// One size step of a <see cref="ThemeScaleEntry"/>: the resource it feeds and the ratio its value
/// is derived at from the scale's base value.
/// </summary>
public class ThemeScaleStep : ObservableObject
{
    /// <summary>Size suffix shown to the user, e.g. <c>xs</c>.</summary>
    public string Label { get; }

    /// <summary>Identifier used to route the derived value to its resource key.</summary>
    public string ResourceName { get; }

    /// <summary>Multiplier applied to the scale's base value, <c>1</c> for the step the base is read from.</summary>
    public double Ratio { get; }

    private double _value;
    /// <summary>The derived value (horizontal amount for a padding scale), in DIPs.</summary>
    public double Value
    {
        get => _value;
        private set => SetProperty(ref _value, value);
    }

    private double _vertical;
    /// <summary>The derived vertical amount; padding scales only.</summary>
    public double Vertical
    {
        get => _vertical;
        private set => SetProperty(ref _vertical, value);
    }

    private string _display = "";
    /// <summary>The exact derived value, formatted for the preview next to the slider.</summary>
    public string Display
    {
        get => _display;
        private set => SetProperty(ref _display, value);
    }

    public ThemeScaleStep(string label, string resourceName, double ratio)
    {
        Label = label;
        ResourceName = resourceName;
        Ratio = ratio;
    }

    /// <summary>Derives this step from <paramref name="value"/> (and <paramref name="vertical"/> when paired).</summary>
    internal void Derive(double value, double? vertical, string unit)
    {
        Value = ThemeScales.Derive(value, Ratio);
        Vertical = vertical is null ? 0 : ThemeScales.Derive(vertical.Value, Ratio);
        Display = vertical is null ? $"{Num(Value)} {unit}" : $"{Num(Value)},{Num(Vertical)} {unit}";
    }

    private static string Num(double value)
        => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>
/// A family of dimensions (control height, font size, control padding…) driven by a single base
/// value: every size step is derived from it by a fixed ratio, so one slider — two for the paddings,
/// one per axis — replaces one slider per size.
/// </summary>
public class ThemeScaleEntry : ObservableObject
{
    private readonly Action<ThemeScaleEntry> _onChanged;

    public string Label { get; }

    public double Minimum { get; }

    public double Maximum { get; }

    public string Unit { get; }

    private bool _isShown = true;
    /// <summary>False while the scale is replaced by other editors (the control padding in the complete mode).</summary>
    public bool IsShown
    {
        get => _isShown;
        set => SetProperty(ref _isShown, value);
    }

    /// <summary>Steps derived from the base value, smallest first.</summary>
    public ObservableCollection<ThemeScaleStep> Steps { get; }

    /// <summary>True when the scale drives two axes (a padding), so a second slider is shown.</summary>
    public bool HasVertical => _verticalBase is not null;

    /// <summary>Axis marker shown next to the base slider, empty for a single-axis scale.</summary>
    public string AxisLabel => HasVertical ? "H" : "";

    /// <summary>Label of the base slider — the size step the base value is the exact value of.</summary>
    public string BaseLabel { get; }

    private double _base;
    /// <summary>The value every step is derived from (the <c>md</c> step's own value).</summary>
    public double Base
    {
        get => _base;
        set
        {
            if (SetProperty(ref _base, Math.Round(value)))
                Refresh();
        }
    }

    private double? _verticalBase;
    /// <summary>The vertical base value; padding scales only.</summary>
    public double VerticalBase
    {
        get => _verticalBase ?? 0;
        set
        {
            if (_verticalBase is null || Math.Round(value) == _verticalBase)
                return;
            _verticalBase = Math.Round(value);
            OnPropertyChanged();
            Refresh();
        }
    }

    public ThemeScaleEntry(
        string label,
        string baseLabel,
        double min,
        double max,
        double @base,
        double? verticalBase,
        Action<ThemeScaleEntry> onChanged,
        string unit,
        params ThemeScaleStep[] steps)
    {
        Label = label;
        BaseLabel = baseLabel;
        Minimum = min;
        Maximum = max;
        _base = Math.Round(@base);
        _verticalBase = verticalBase is null ? null : Math.Round(verticalBase.Value);
        _onChanged = onChanged;
        Unit = unit;
        Steps = new ObservableCollection<ThemeScaleStep>(steps);
        DeriveSteps();
    }

    /// <summary>Re-seeds the base values without pushing them back into the live resources.</summary>
    public void SetBaseSilently(double @base, double? verticalBase = null)
    {
        SetProperty(ref _base, Math.Round(@base), nameof(Base));
        if (_verticalBase is not null && verticalBase is not null)
        {
            _verticalBase = Math.Round(verticalBase.Value);
            OnPropertyChanged(nameof(VerticalBase));
        }
        DeriveSteps();
    }

    private void Refresh()
    {
        DeriveSteps();
        _onChanged(this);
    }

    private void DeriveSteps()
    {
        foreach (var step in Steps)
            step.Derive(_base, _verticalBase, Unit);
    }
}

public class ThemeColorGroup
{
    public string Header { get; }
    public ObservableCollection<ThemeColorEntry> Colors { get; }
    public ThemeColorGroup(string header, ObservableCollection<ThemeColorEntry> colors)
    {
        Header = header;
        Colors = colors;
    }
}

public class ThemeDimensionGroup
{
    public string Header { get; }
    public ObservableCollection<ThemeDimensionEntry> Dimensions { get; }
    public ThemeDimensionGroup(string header, ObservableCollection<ThemeDimensionEntry> dimensions)
    {
        Header = header;
        Dimensions = dimensions;
    }
}

/// <summary>
/// One category of the complete mode (Display, Input or Action) : its own colours and shape. The padding is the md one,
/// per axis : the inside of a display, the xs to lg paddings of an input or an action derived from it by the ratios of
/// the control padding.
/// </summary>
public class ThemeCategoryEditor
{
    /// <summary>The category name, the prefix of its keys (<c>Display</c>, <c>Input</c>, <c>Action</c>).</summary>
    public string Name { get; }

    public string Description { get; }

    public ThemeColorEntry Surface1 { get; }
    public ThemeColorEntry Surface2 { get; }
    public ThemeColorEntry Border { get; }
    public IReadOnlyList<ThemeColorEntry> Colors { get; }

    public ThemeDimensionEntry Radius { get; }
    public ThemeDimensionEntry Thickness { get; }
    public ThemeDimensionEntry PaddingHorizontal { get; }
    public ThemeDimensionEntry PaddingVertical { get; }
    public IReadOnlyList<ThemeDimensionEntry> Dimensions { get; }

    public ThemeCategoryEditor(
        string name,
        string description,
        (ComponentResourceKey Surface1, ComponentResourceKey Surface2, ComponentResourceKey Border) keys,
        Action<ThemeColorEntry> onColorChanged,
        Action<ThemeDimensionEntry> onDimensionChanged)
    {
        Name = name;
        Description = description;
        Surface1 = new ThemeColorEntry("Surface 1", $"{name}Surface1Color", keys.Surface1, default, onColorChanged);
        Surface2 = new ThemeColorEntry("Surface 2", $"{name}Surface2Color", keys.Surface2, default, onColorChanged);
        Border = new ThemeColorEntry("Border", $"{name}BorderColor", keys.Border, default, onColorChanged);
        Colors = [Surface1, Surface2, Border];

        Radius = new ThemeDimensionEntry("Corner radius", $"{name}Radius", 0, 24, 0, onDimensionChanged);
        Thickness = new ThemeDimensionEntry("Border thickness", $"{name}Thickness", 0, 4, 0, onDimensionChanged);
        PaddingHorizontal = new ThemeDimensionEntry("Padding (H)", $"{name}PaddingHorizontal", 0, 40, 0, onDimensionChanged);
        PaddingVertical = new ThemeDimensionEntry("Padding (V)", $"{name}PaddingVertical", 0, 40, 0, onDimensionChanged);
        Dimensions = [Radius, Thickness, PaddingHorizontal, PaddingVertical];
    }

    public ThemeCategoryValues Values => new(
        Surface1.Color, Surface2.Color, Border.Color, Radius.Value, Thickness.Value, PaddingHorizontal.Value, PaddingVertical.Value);

    /// <summary>Seeds the editors without pushing them back into the live resources.</summary>
    public void SetSilently(ThemeCategoryValues values)
    {
        Surface1.SetColorSilently(values.Surface1);
        Surface2.SetColorSilently(values.Surface2);
        Border.SetColorSilently(values.Border);
        SetShapeSilently(values);
    }

    /// <summary>Seeds the shape alone, keeping the colours.</summary>
    public void SetShapeSilently(ThemeCategoryValues values)
    {
        Radius.SetValueSilently(values.Radius);
        Thickness.SetValueSilently(values.Thickness);
        PaddingHorizontal.SetValueSilently(values.PaddingHorizontal);
        PaddingVertical.SetValueSilently(values.PaddingVertical);
    }

    /// <summary>Seeds the colours alone, keeping the shape.</summary>
    public void SetColorsSilently(ThemeCategoryValues values)
    {
        Surface1.SetColorSilently(values.Surface1);
        Surface2.SetColorSilently(values.Surface2);
        Border.SetColorSilently(values.Border);
    }
}

/// <summary>
/// Drives the "Customize theme" page: edits live colour/dimension resources application-wide
/// (so the whole gallery becomes the preview) and emits a drop-in ResourceDictionary.
/// </summary>
public class ThemeCustomizerViewModel : ObservableObject
{
    // When true, entry changes are seeding/resetting and must not push back into resources.
    private bool _suppress;

    private readonly List<ThemeColorEntry> _allColors = new();
    private readonly List<ThemeDimensionEntry> _allDimensions = new();
    private readonly List<ThemeScaleEntry> _allScales = new();

    // Kept by name, rather than recognised by the names of their steps : some resources follow a scale on top of its
    // own steps, and the generated dictionary needs each one's base.
    private ThemeScaleEntry _heightScale = null!;
    private ThemeScaleEntry _fontSizeScale = null!;
    private ThemeScaleEntry _paddingScale = null!;

    private readonly ThemeCategoryEditor _display;
    private readonly ThemeCategoryEditor _input;
    private readonly ThemeCategoryEditor _action;

    /// <summary>The neutrals: the surfaces, the text and the border.</summary>
    public ObservableCollection<ThemeColorGroup> NeutralColorGroups { get; } = new();

    /// <summary>The accents, a fill and the content drawn on it each.</summary>
    public ObservableCollection<ThemeColorGroup> AccentColorGroups { get; } = new();

    /// <summary>The categories edited on their own in the complete mode.</summary>
    public IReadOnlyList<ThemeCategoryEditor> Categories { get; }

    private bool _isComplete;
    /// <summary>
    /// The complete mode : every category (Display, Input, Action) gets its own colours and shape, starting from the
    /// global ones. Back in the simple mode, the categories follow the global colours and shape again.
    /// </summary>
    public bool IsComplete
    {
        get => _isComplete;
        set
        {
            if (!SetProperty(ref _isComplete, value))
                return;

            // Each category has its own padding in the complete mode.
            _paddingScale.IsShown = !value;
            if (_suppress)
                return;
            if (value)
                SeedCategories();
            ApplyColors();
            ApplyShape();
        }
    }

    public ObservableCollection<ThemeDimensionGroup> DimensionGroups { get; } = new();
    /// <summary>Size families driven by a single base value (control height, font size, control padding).</summary>
    public ObservableCollection<ThemeScaleEntry> Scales { get; } = new();

    /// <summary>Selectable preset palettes, sourced from the themes registered with <see cref="ThemeManager"/>.</summary>
    public IReadOnlyList<ThemePreset> Presets { get; }

    private ThemePreset? _selectedPreset;
    /// <summary>The currently selected preset; assigning a non-null value applies its palette.</summary>
    public ThemePreset? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (SetProperty(ref _selectedPreset, value) && value is not null && !_suppress)
                ApplyPreset(value);
        }
    }

    public ICommand ResetCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand SaveCommand { get; }

    public ThemeCustomizerViewModel()
    {
        BuildColorGroups();
        BuildDimensionGroups();
        BuildScales();

        _display = new ThemeCategoryEditor(
            "Display",
            "Cards, popups, expanders, group boxes, and the frame of lists, trees, grids, menus and tabs. Their items follow the actions. No states.",
            (JColors.DisplaySurface1Color, JColors.DisplaySurface2Color, JColors.DisplayBorderColor),
            OnCategoryColorChanged, OnCategoryDimensionChanged);
        _input = new ThemeCategoryEditor(
            "Input",
            "Text boxes, combo boxes, check boxes, radio buttons, sliders. No hover, a strong border on focus. The padding is the md one, the other sizes follow it.",
            (JColors.InputSurface1Color, JColors.InputSurface2Color, JColors.InputBorderColor),
            OnCategoryColorChanged, OnCategoryDimensionChanged);
        _action = new ThemeCategoryEditor(
            "Action",
            "Buttons and toggle buttons. A stronger background on hover, a strong border on focus, moved down when pressed. The padding is the md one, the other sizes follow it.",
            (JColors.ActionSurface1Color, JColors.ActionSurface2Color, JColors.ActionBorderColor),
            OnCategoryColorChanged, OnCategoryDimensionChanged);
        Categories = [_display, _input, _action];
        SeedCategories();

        // Built after the colour groups so _allColors tells us which keys to read from each theme.
        Presets = BuildPresets();

        ResetCommand = new RelayCommand(Reset);
        CopyCommand = new RelayCommand(Copy);
        SaveCommand = new RelayCommand(Save);
    }

    #region Definitions

    private void BuildColorGroups()
    {
        ThemeColorGroup Group(string header, params (string label, string name, ComponentResourceKey key)[] items)
        {
            var colors = new ObservableCollection<ThemeColorEntry>();
            foreach (var (label, name, key) in items)
            {
                var entry = new ThemeColorEntry(label, name, key, ReadColor(key), OnColorChanged);
                colors.Add(entry);
                _allColors.Add(entry);
            }
            return new ThemeColorGroup(header, colors);
        }

        NeutralColorGroups.Add(Group("Surface",
            ("Base", "BaseColor", JColors.BaseColor),
            ("Surface 1", "Surface1Color", JColors.Surface1Color),
            ("Surface 2", "Surface2Color", JColors.Surface2Color)));

        NeutralColorGroups.Add(Group("Text and border",
            ("Foreground", "ForegroundColor", JColors.ForegroundColor),
            ("Stroke", "StrokeColor", JColors.StrokeColor)));

        AccentColorGroups.Add(Group("Primary",
            ("Primary", "PrimaryColor", JColors.PrimaryColor),
            ("Primary content", "PrimaryContentColor", JColors.PrimaryContentColor)));

        AccentColorGroups.Add(Group("Secondary",
            ("Secondary", "SecondaryColor", JColors.SecondaryColor),
            ("Secondary content", "SecondaryContentColor", JColors.SecondaryContentColor)));

        AccentColorGroups.Add(Group("Success",
            ("Success", "SuccessColor", JColors.SuccessColor),
            ("Success content", "SuccessContentColor", JColors.SuccessContentColor)));

        AccentColorGroups.Add(Group("Info",
            ("Info", "InfoColor", JColors.InfoColor),
            ("Info content", "InfoContentColor", JColors.InfoContentColor)));

        AccentColorGroups.Add(Group("Warning",
            ("Warning", "WarningColor", JColors.WarningColor),
            ("Warning content", "WarningContentColor", JColors.WarningContentColor)));

        AccentColorGroups.Add(Group("Danger",
            ("Danger", "DangerColor", JColors.DangerColor),
            ("Danger content", "DangerContentColor", JColors.DangerContentColor)));
    }

    private void BuildDimensionGroups()
    {
        ThemeDimensionGroup Group(string header, params ThemeDimensionEntry[] items)
        {
            var dims = new ObservableCollection<ThemeDimensionEntry>();
            foreach (var entry in items)
            {
                dims.Add(entry);
                _allDimensions.Add(entry);
            }
            return new ThemeDimensionGroup(header, dims);
        }

        ThemeDimensionEntry Dim(string label, string name, double min, double max) =>
            new(label, name, min, max, ReadDouble(DimensionKey(name)), OnDimensionChanged);

        DimensionGroups.Add(Group("Shape",
            Dim("Corner radius", "Radius", 0, 24),
            Dim("Border thickness", "Thickness", 0, 4),
            Dim("Spacing", "Spacing", 0, 32)));
    }

    /// <summary>
    /// Declares the size families, each driven by its <c>md</c> value, the other steps keeping the ratio
    /// <see cref="ThemeScales"/> gives them.
    /// </summary>
    private void BuildScales()
    {
        ThemeScaleEntry Scale(string label, double min, double max, double @base, double? verticalBase, IReadOnlyList<ThemeScaleSize> sizes)
        {
            ThemeScaleStep[] steps = [.. sizes.Select(size => new ThemeScaleStep(size.Label, size.ResourceName, size.Ratio))];
            var entry = new ThemeScaleEntry(label, "md", min, max, @base, verticalBase, OnScaleChanged, "px", steps);
            Scales.Add(entry);
            _allScales.Add(entry);
            return entry;
        }

        _heightScale = Scale("Control height", 16, 72, ReadDouble(JDimensions.HeightMd), null, ThemeScales.Height);
        _fontSizeScale = Scale("Font size", 8, 32, ReadDouble(JDimensions.FontSizeMd), null, ThemeScales.FontSize);

        // ControlPadding thicknesses are symmetric (Left==Right, Top==Bottom), so one base per axis.
        // The text input paddings are derived from these at half the horizontal, not edited on their own.
        Thickness padding = ReadThickness(JDimensions.ActionPaddingMd);
        _paddingScale = Scale("Control padding", 0, 40, padding.Left, padding.Top, ThemeScales.Padding);
    }

    /// <summary>
    /// Builds a preset per concrete theme registered with <see cref="ThemeManager"/> (<c>System</c> is
    /// skipped — it is a resolver, not a palette). Each preset's colours are read straight from the
    /// theme's dictionary so the list never drifts from what the app can actually apply.
    /// </summary>
    private IReadOnlyList<ThemePreset> BuildPresets()
    {
        var presets = new List<ThemePreset>();
        foreach (string name in ThemeManager.Instance.Themes)
        {
            ResourceDictionary? dictionary = ThemeManager.Instance.GetDictionary(name);
            if (dictionary is null)
                continue;

            var colors = new Dictionary<string, Color>();
            foreach (var entry in _allColors)
            {
                if (dictionary[entry.Key] is Color color)
                    colors[entry.ResourceName] = color;
            }

            // Only offer themes that define the whole editable palette — ThemePreset needs every key.
            if (colors.Count == _allColors.Count)
                presets.Add(new ThemePreset(name, colors));
        }
        return presets;
    }

    #endregion

    #region Live application

    /// <summary>Applies a preset palette: seeds the editors and pushes every colour to the live resources.</summary>
    private void ApplyPreset(ThemePreset preset)
    {
        _suppress = true;
        try
        {
            foreach (var entry in _allColors)
            {
                if (!preset.Colors.TryGetValue(entry.ResourceName, out Color color))
                    continue;
                entry.SetColorSilently(color);
            }
            // A preset has no category colours : the complete mode starts them again from the preset, keeping its shapes.
            if (IsComplete)
                SeedCategoryColors();
        }
        finally
        {
            _suppress = false;
        }

        ApplyColors();
    }

    private void OnColorChanged(ThemeColorEntry entry)
    {
        if (_suppress)
            return;
        // A manual edit no longer matches any preset — drop the highlight without re-applying.
        SetProperty(ref _selectedPreset, null, nameof(SelectedPreset));
        ApplyColors();
    }

    private void OnCategoryColorChanged(ThemeColorEntry entry)
    {
        if (_suppress)
            return;
        SetProperty(ref _selectedPreset, null, nameof(SelectedPreset));
        ApplyColors();
    }

    private void OnCategoryDimensionChanged(ThemeDimensionEntry entry)
    {
        if (_suppress)
            return;
        ApplyShape();
    }

    /// <summary>
    /// Starts the categories of the complete mode from the global colours and shape, as the simple mode has them : an
    /// action filled with the page and padded by the control padding, an input at half its horizontal.
    /// </summary>
    private void SeedCategories()
    {
        foreach ((ThemeCategoryEditor category, ThemeCategoryValues values) in SimpleCategories())
            category.SetSilently(values);
    }

    private void SeedCategoryColors()
    {
        foreach ((ThemeCategoryEditor category, ThemeCategoryValues values) in SimpleCategories())
            category.SetColorsSilently(values);
    }

    /// <summary>The categories as the simple mode makes them out of the global colours and shape.</summary>
    private IEnumerable<(ThemeCategoryEditor Category, ThemeCategoryValues Values)> SimpleCategories()
    {
        double radius = DimValue("Radius");
        double thickness = DimValue("Thickness");
        double spacing = DimValue("Spacing");
        double paddingHorizontal = _paddingScale.Base;
        double paddingVertical = _paddingScale.VerticalBase;
        Color surface2 = GlobalColor("Surface2Color");
        Color stroke = GlobalColor("StrokeColor");
        yield return (_display, new ThemeCategoryValues(GlobalColor("Surface1Color"), surface2, stroke, radius, thickness, spacing, spacing));
        yield return (_input, new ThemeCategoryValues(
            GlobalColor("Surface1Color"), surface2, stroke, radius, thickness,
            paddingHorizontal * ThemeScales.InputPaddingHorizontalRatio, paddingVertical));
        yield return (_action, new ThemeCategoryValues(GlobalColor("BaseColor"), surface2, stroke, radius, thickness, paddingHorizontal, paddingVertical));
    }

    /// <summary>The colours and shape of every category, its own in the complete mode, the global ones otherwise.</summary>
    private IEnumerable<(ThemeCategoryEditor Category, ThemeCategoryValues Values)> CurrentCategories()
        => IsComplete ? Categories.Select(category => (category, category.Values)) : SimpleCategories();

    private Color GlobalColor(string resourceName) => _allColors.First(entry => entry.ResourceName == resourceName).Color;

    /// <summary>
    /// Pushes every colour and the brushes made of them to the live resources. A brush resolves its DynamicResource
    /// colour once, when first used, so overriding a colour alone leaves it as it was : the brushes are overridden
    /// too, the derived ones (hover, strong border) mixed here the way Brushes.xaml mixes them.
    /// </summary>
    private void ApplyColors()
    {
        var res = Application.Current.Resources;
        foreach (var entry in _allColors)
        {
            res[entry.Key] = entry.Color;
            res[BrushKey(entry.ResourceName)] = new SolidColorBrush(entry.Color);
        }

        foreach ((ThemeCategoryEditor category, ThemeCategoryValues values) in CurrentCategories())
        {
            // The category colours exist only in the complete mode : in the simple one they fall back to the global ones.
            foreach ((ThemeColorEntry entry, Color color) in new[] { (category.Surface1, values.Surface1), (category.Surface2, values.Surface2), (category.Border, values.Border) })
            {
                if (IsComplete)
                    res[entry.Key] = color;
                else
                    res.Remove(entry.Key);
                res[BrushKey(entry.ResourceName)] = new SolidColorBrush(color);
            }
        }

        Color foreground = GlobalColor("ForegroundColor");
        ThemeCategoryValues action = CurrentCategories().First(c => c.Category == _action).Values;
        ThemeCategoryValues input = CurrentCategories().First(c => c.Category == _input).Values;
        res[JBrushes.ActionHoverBrush] = new SolidColorBrush(ColorShade.Mix(action.Surface1, foreground, 0.1));
        res[JBrushes.ActionBorderStrongBrush] = new SolidColorBrush(ColorShade.Mix(action.Border, foreground, 0.4));
        res[JBrushes.InputBorderStrongBrush] = new SolidColorBrush(ColorShade.Mix(input.Border, foreground, 0.4));
    }

    /// <summary>
    /// Pushes the shape to the live resources : the global radius, thickness and spacing, and the radius, border and
    /// padding of each category.
    /// </summary>
    private void ApplyShape()
    {
        var res = Application.Current.Resources;
        double radius = DimValue("Radius");
        double thickness = DimValue("Thickness");
        double spacing = DimValue("Spacing");

        // These base doubles feed a derived Thickness/CornerRadius built with StaticResource,
        // so the derived key must be overridden explicitly for the live preview to move.
        res[JDimensions.Radius] = radius;
        res[JDimensions.CornerRadius] = new CornerRadius(radius);
        res[JDimensions.Thickness] = thickness;
        res[JDimensions.BorderThickness] = new Thickness(thickness);
        res[JDimensions.Spacing] = spacing;
        res[JDimensions.SpacingThickness] = new Thickness(spacing);

        foreach ((ThemeCategoryEditor category, ThemeCategoryValues values) in CurrentCategories())
        {
            (ComponentResourceKey radiusKey, ComponentResourceKey cornerRadiusKey, ComponentResourceKey thicknessKey) = CategoryShapeKeys(category.Name);
            res[radiusKey] = values.Radius;
            res[cornerRadiusKey] = new CornerRadius(values.Radius);
            res[thicknessKey] = new Thickness(values.Thickness);
        }
        ThemeCategoryValues display = CurrentCategories().First(c => c.Category == _display).Values;
        res[JDimensions.DisplayPadding] = new Thickness(display.PaddingHorizontal, display.PaddingVertical, display.PaddingHorizontal, display.PaddingVertical);
        ApplyPaddings();
    }

    /// <summary>
    /// The action and input paddings of every size : from the control padding in the simple mode (an input at half
    /// its horizontal), from each category's own in the complete one.
    /// </summary>
    private void ApplyPaddings()
    {
        var res = Application.Current.Resources;
        ThemeValues values = CurrentValues();
        foreach (ThemeScaleSize size in ThemeScales.Padding)
        {
            res[PaddingKey(size.ResourceName)] = ThemeScales.ActionPadding(size.Ratio, values);
            res[InputPaddingKey(size.ResourceName)] = ThemeScales.InputPadding(size.Ratio, values);
        }
    }

    private void OnDimensionChanged(ThemeDimensionEntry entry)
    {
        if (_suppress)
            return;
        ApplyDimension(entry);
    }

    private void OnScaleChanged(ThemeScaleEntry entry)
    {
        if (_suppress)
            return;
        ApplyScale(entry);
    }

    /// <summary>Pushes every derived step of a scale to its live resource.</summary>
    private void ApplyScale(ThemeScaleEntry entry)
    {
        var res = Application.Current.Resources;
        // The control padding feeds the action and the input paddings, through the categories in the complete mode.
        if (entry == _paddingScale)
            ApplyPaddings();
        else
            foreach (var step in entry.Steps)
                res[DimensionKey(step.ResourceName)] = step.Value;

        // The close-button height is not edited on its own: it follows the control height's base value.
        if (entry == _heightScale)
            res[JDimensions.HeightEmbedded] = ThemeScales.Derive(entry.Base, ThemeScales.EmbeddedHeightRatio);
    }

    /// <summary>The input padding key a control padding step feeds (e.g. <c>ActionPaddingSm</c> → <c>InputPaddingSm</c>).</summary>
    private static ComponentResourceKey InputPaddingKey(string controlPaddingName)
        => PaddingKey(ThemeScales.InputPaddingName(controlPaddingName));

    private void ApplyDimension(ThemeDimensionEntry entry)
    {
        var res = Application.Current.Resources;
        double v = entry.Value;
        switch (entry.ResourceName)
        {
            // The shape feeds the categories too, in the simple mode.
            case "Radius" or "Thickness" or "Spacing":
                ApplyShape();
                break;
            default:
                res[DimensionKey(entry.ResourceName)] = v;
                break;
        }
    }

    #endregion

    #region Commands

    private void Reset()
    {
        _suppress = true;
        try
        {
            var res = Application.Current.Resources;

            // Drop every override so lookups fall back to the merged theme dictionary…
            foreach (var color in _allColors.Concat(Categories.SelectMany(category => category.Colors)))
            {
                res.Remove(color.Key);
                res.Remove(BrushKey(color.ResourceName));
            }
            res.Remove(JBrushes.ActionHoverBrush);
            res.Remove(JBrushes.ActionBorderStrongBrush);
            res.Remove(JBrushes.InputBorderStrongBrush);
            foreach (var key in AllDimensionKeys())
                res.Remove(key);
            foreach (var step in _allScales.SelectMany(scale => scale.Steps))
                res.Remove(ScaleKey(step.ResourceName));
            // Input paddings are derived from the control paddings, so drop their overrides too.
            foreach (var step in _paddingScale.Steps)
                res.Remove(InputPaddingKey(step.ResourceName));

            // …then re-seed the editors from those restored values.
            foreach (var color in _allColors)
                color.SetColorSilently(ReadColor(color.Key));
            foreach (var dim in _allDimensions)
                dim.SetValueSilently(ReadDouble(DimensionKey(dim.ResourceName)));
            foreach (var scale in _allScales)
            {
                // Re-seed from the restored md step, the one the base value is read from.
                string mdName = scale.Steps.First(step => step.Ratio == 1).ResourceName;
                if (scale.HasVertical)
                {
                    Thickness t = ReadThickness(PaddingKey(mdName));
                    scale.SetBaseSilently(t.Left, t.Top);
                }
                else
                {
                    scale.SetBaseSilently(ReadDouble(DimensionKey(mdName)));
                }
            }

            // Back to the theme as it is : the simple mode, the categories following it.
            IsComplete = false;
            SeedCategories();
        }
        finally
        {
            _suppress = false;
        }

        SelectedPreset = null;
    }

    private void Copy()
    {
        try
        {
            Clipboard.SetText(ThemeDictionaryWriter.Write(CurrentValues()));
        }
        catch
        {
            // Clipboard can transiently fail if another process holds it; ignore.
        }
    }

    private void Save()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save theme dictionary",
            Filter = "XAML resource dictionary (*.xaml)|*.xaml|All files (*.*)|*.*",
            FileName = "Theme.xaml",
            DefaultExt = ".xaml",
            AddExtension = true,
        };

        if (dialog.ShowDialog() == true)
            File.WriteAllText(dialog.FileName, ThemeDictionaryWriter.Write(CurrentValues()));
    }

    #endregion

    #region Resource helpers

    private static Color ReadColor(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is Color color ? color : System.Windows.Media.Colors.Magenta;

    private static double ReadDouble(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is double value ? value : 0d;

    private static Thickness ReadThickness(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is Thickness value ? value : new Thickness(0);

    /// <summary>The brush key derived from a colour's accessor name (e.g. <c>PrimaryColor</c> → <c>PrimaryBrush</c>).</summary>
    private static ComponentResourceKey BrushKey(string colorName)
        => new(typeof(JBrushes), colorName.Replace("Color", "Brush"));

    private static ComponentResourceKey DimensionKey(string name) => name switch
    {
        "Radius" => JDimensions.Radius,
        "Thickness" => JDimensions.Thickness,
        "Spacing" => JDimensions.Spacing,
        "HeightXs" => JDimensions.HeightXs,
        "HeightSm" => JDimensions.HeightSm,
        "HeightMd" => JDimensions.HeightMd,
        "HeightLg" => JDimensions.HeightLg,
        "HeightEmbedded" => JDimensions.HeightEmbedded,
        "FontSizeXs" => JDimensions.FontSizeXs,
        "FontSizeSm" => JDimensions.FontSizeSm,
        "FontSizeMd" => JDimensions.FontSizeMd,
        "FontSizeLg" => JDimensions.FontSizeLg,
        "FontSizeXl" => JDimensions.FontSizeXl,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown dimension"),
    };

    private static ComponentResourceKey PaddingKey(string name) => name switch
    {
        "ActionPaddingXs" => JDimensions.ActionPaddingXs,
        "ActionPaddingSm" => JDimensions.ActionPaddingSm,
        "ActionPaddingMd" => JDimensions.ActionPaddingMd,
        "ActionPaddingLg" => JDimensions.ActionPaddingLg,
        "InputPaddingXs" => JDimensions.InputPaddingXs,
        "InputPaddingSm" => JDimensions.InputPaddingSm,
        "InputPaddingMd" => JDimensions.InputPaddingMd,
        "InputPaddingLg" => JDimensions.InputPaddingLg,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown padding"),
    };

    /// <summary>The key of a scale step, whichever kind of resource it feeds.</summary>
    private static ComponentResourceKey ScaleKey(string name)
        => name.StartsWith("ActionPadding", StringComparison.Ordinal) ? PaddingKey(name) : DimensionKey(name);

    private static (ComponentResourceKey Radius, ComponentResourceKey CornerRadius, ComponentResourceKey BorderThickness) CategoryShapeKeys(string category) => category switch
    {
        "Display" => (JDimensions.DisplayRadius, JDimensions.DisplayCornerRadius, JDimensions.DisplayBorderThickness),
        "Input" => (JDimensions.InputRadius, JDimensions.InputCornerRadius, JDimensions.InputBorderThickness),
        "Action" => (JDimensions.ActionRadius, JDimensions.ActionCornerRadius, JDimensions.ActionBorderThickness),
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown category"),
    };

    private static readonly (ComponentResourceKey Radius, ComponentResourceKey CornerRadius)[] CategoryRadiusKeys =
    [
        (JDimensions.DisplayRadius, JDimensions.DisplayCornerRadius),
        (JDimensions.InputRadius, JDimensions.InputCornerRadius),
        (JDimensions.ActionRadius, JDimensions.ActionCornerRadius),
    ];

    private static readonly ComponentResourceKey[] CategoryBorderThicknessKeys =
        [JDimensions.DisplayBorderThickness, JDimensions.InputBorderThickness, JDimensions.ActionBorderThickness];

    private IEnumerable<ComponentResourceKey> AllDimensionKeys()
    {
        yield return JDimensions.Radius;
        yield return JDimensions.CornerRadius;
        yield return JDimensions.Thickness;
        yield return JDimensions.BorderThickness;
        yield return JDimensions.Spacing;
        yield return JDimensions.SpacingThickness;
        yield return JDimensions.HeightEmbedded;
        foreach ((ComponentResourceKey radius, ComponentResourceKey cornerRadius) in CategoryRadiusKeys)
        {
            yield return radius;
            yield return cornerRadius;
        }
        foreach (ComponentResourceKey borderThickness in CategoryBorderThicknessKeys)
            yield return borderThickness;
        yield return JDimensions.DisplayPadding;
        foreach (var dim in _allDimensions)
        {
            if (dim.ResourceName is "Radius" or "Thickness" or "Spacing")
                continue;
            yield return DimensionKey(dim.ResourceName);
        }
    }

    #endregion

    #region XAML generation

    private double DimValue(string name) => _allDimensions.First(d => d.ResourceName == name).Value;

    private ThemeValues CurrentValues() => new(
        [.. _allColors.Select(entry => KeyValuePair.Create(entry.ResourceName, entry.Color))],
        Thickness: DimValue("Thickness"),
        Radius: DimValue("Radius"),
        Spacing: DimValue("Spacing"),
        HeightMd: _heightScale.Base,
        FontSizeMd: _fontSizeScale.Base,
        PaddingMdHorizontal: _paddingScale.Base,
        PaddingMdVertical: _paddingScale.VerticalBase,
        Categories: IsComplete ? new ThemeCategories(_display.Values, _input.Values, _action.Values) : null);

    #endregion
}
