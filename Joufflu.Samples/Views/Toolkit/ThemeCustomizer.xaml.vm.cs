using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using JBrushes = Joufflu.Brushes;
using JColors = Joufflu.Colors;
using JDimensions = Joufflu.Dimensions;

namespace Joufflu.Samples.Views.Toolkit;

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
/// Drives the "Customize theme" page: edits live colour/dimension resources application-wide
/// (so the whole gallery becomes the preview) and emits a drop-in ResourceDictionary.
/// </summary>
public class ThemeCustomizerViewModel : ObservableObject
{
    // When true, entry changes are seeding/resetting and must not push back into resources.
    private bool _suppress;

    private readonly List<ThemeColorEntry> _allColors = new();
    private readonly List<ThemeDimensionEntry> _allDimensions = new();

    public ObservableCollection<ThemeColorGroup> ColorGroups { get; } = new();
    public ObservableCollection<ThemeDimensionGroup> DimensionGroups { get; } = new();

    /// <summary>Selectable preset palettes shown as cards at the top of the editor.</summary>
    public IReadOnlyList<ThemePreset> Presets { get; } = ThemePresets.All;

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

    private string _generatedXaml = "";
    public string GeneratedXaml
    {
        get => _generatedXaml;
        private set => SetProperty(ref _generatedXaml, value);
    }

    public ICommand ResetCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand SaveCommand { get; }

    public ThemeCustomizerViewModel()
    {
        BuildColorGroups();
        BuildDimensionGroups();

        ResetCommand = new RelayCommand(Reset);
        CopyCommand = new RelayCommand(Copy);
        SaveCommand = new RelayCommand(Save);

        RegenerateXaml();
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

        ColorGroups.Add(Group("Surface",
            ("Background", "BackgroundColor", JColors.BackgroundColor),
            ("Background 100 (elevated)", "Background100Color", JColors.Background100Color),
            ("Background 200 (selected)", "Background200Color", JColors.Background200Color),
            ("Border", "BorderColor", JColors.BorderColor),
            ("Border 100", "Border100Color", JColors.Border100Color)));

        ColorGroups.Add(Group("Text",
            ("Foreground", "ForegroundColor", JColors.ForegroundColor),
            ("Foreground 100", "Foreground100Color", JColors.Foreground100Color),
            ("Foreground 200", "Foreground200Color", JColors.Foreground200Color)));

        ColorGroups.Add(Group("Primary",
            ("Primary", "PrimaryColor", JColors.PrimaryColor),
            ("Primary 100 (hover)", "Primary100Color", JColors.Primary100Color),
            ("Primary content", "PrimaryContentColor", JColors.PrimaryContentColor)));

        ColorGroups.Add(Group("Secondary",
            ("Secondary", "SecondaryColor", JColors.SecondaryColor),
            ("Secondary 100 (hover)", "Secondary100Color", JColors.Secondary100Color),
            ("Secondary content", "SecondaryContentColor", JColors.SecondaryContentColor)));

        ColorGroups.Add(Group("Success",
            ("Success", "SuccessColor", JColors.SuccessColor),
            ("Success 100 (hover)", "Success100Color", JColors.Success100Color),
            ("Success content", "SuccessContentColor", JColors.SuccessContentColor)));

        ColorGroups.Add(Group("Info",
            ("Info", "InfoColor", JColors.InfoColor),
            ("Info 100 (hover)", "Info100Color", JColors.Info100Color),
            ("Info content", "InfoContentColor", JColors.InfoContentColor)));

        ColorGroups.Add(Group("Warning",
            ("Warning", "WarningColor", JColors.WarningColor),
            ("Warning 100 (hover)", "Warning100Color", JColors.Warning100Color),
            ("Warning content", "WarningContentColor", JColors.WarningContentColor)));

        ColorGroups.Add(Group("Danger",
            ("Danger", "DangerColor", JColors.DangerColor),
            ("Danger 100 (hover)", "Danger100Color", JColors.Danger100Color),
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

        DimensionGroups.Add(Group("Control height",
            Dim("Extra small", "ControlHeightXs", 16, 40),
            Dim("Small", "ControlHeightSm", 20, 44),
            Dim("Medium", "ControlHeightMd", 24, 52),
            Dim("Large", "ControlHeightLg", 28, 64)));

        DimensionGroups.Add(Group("Font size",
            Dim("Extra small", "ControlFontSizeXs", 8, 20),
            Dim("Small", "ControlFontSizeSm", 9, 22),
            Dim("Medium", "ControlFontSizeMd", 10, 24),
            Dim("Large", "ControlFontSizeLg", 11, 28)));
    }

    #endregion

    #region Live application

    /// <summary>Applies a preset palette: seeds the editors and pushes every colour to the live resources.</summary>
    private void ApplyPreset(ThemePreset preset)
    {
        _suppress = true;
        try
        {
            var res = Application.Current.Resources;
            foreach (var entry in _allColors)
            {
                if (!preset.Colors.TryGetValue(entry.ResourceName, out Color color))
                    continue;
                entry.SetColorSilently(color);
                res[entry.Key] = color;
                res[BrushKey(entry.ResourceName)] = new SolidColorBrush(color);
            }
        }
        finally
        {
            _suppress = false;
        }

        RegenerateXaml();
    }

    private void OnColorChanged(ThemeColorEntry entry)
    {
        if (_suppress)
            return;
        // A manual edit no longer matches any preset — drop the highlight without re-applying.
        SetProperty(ref _selectedPreset, null, nameof(SelectedPreset));
        var res = Application.Current.Resources;
        res[entry.Key] = entry.Color;
        // The semantic brushes bind their Color via DynamicResource, but each brush lives in the
        // same merged theme dictionary as its colour and resolves it there before reaching this
        // app-level override. Override the derived brush explicitly so the preview moves
        // (same reason ApplyDimension overrides the derived Thickness/CornerRadius keys).
        res[BrushKey(entry.ResourceName)] = new SolidColorBrush(entry.Color);
        RegenerateXaml();
    }

    private void OnDimensionChanged(ThemeDimensionEntry entry)
    {
        if (_suppress)
            return;
        ApplyDimension(entry);
        RegenerateXaml();
    }

    private void ApplyDimension(ThemeDimensionEntry entry)
    {
        var res = Application.Current.Resources;
        double v = entry.Value;
        switch (entry.ResourceName)
        {
            // These base doubles feed a derived Thickness/CornerRadius built with StaticResource,
            // so the derived key must be overridden explicitly for the live preview to move.
            case "Radius":
                res[JDimensions.Radius] = v;
                res[JDimensions.CornerRadius] = new CornerRadius(v);
                break;
            case "Thickness":
                res[JDimensions.Thickness] = v;
                res[JDimensions.BorderThickness] = new Thickness(v);
                break;
            case "Spacing":
                res[JDimensions.Spacing] = v;
                res[JDimensions.SpacingThickness] = new Thickness(v);
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
            foreach (var color in _allColors)
            {
                res.Remove(color.Key);
                res.Remove(BrushKey(color.ResourceName));
            }
            foreach (var key in AllDimensionKeys())
                res.Remove(key);

            // …then re-seed the editors from those restored values.
            foreach (var color in _allColors)
                color.SetColorSilently(ReadColor(color.Key));
            foreach (var dim in _allDimensions)
                dim.SetValueSilently(ReadDouble(DimensionKey(dim.ResourceName)));
        }
        finally
        {
            _suppress = false;
        }

        SelectedPreset = null;
        RegenerateXaml();
    }

    private void Copy()
    {
        try
        {
            Clipboard.SetText(GeneratedXaml);
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
            File.WriteAllText(dialog.FileName, GeneratedXaml);
    }

    #endregion

    #region Resource helpers

    private static Color ReadColor(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is Color color ? color : System.Windows.Media.Colors.Magenta;

    private static double ReadDouble(ComponentResourceKey key)
        => Application.Current.TryFindResource(key) is double value ? value : 0d;

    /// <summary>The brush key derived from a colour's accessor name (e.g. <c>PrimaryColor</c> → <c>PrimaryBrush</c>).</summary>
    private static ComponentResourceKey BrushKey(string colorName)
        => new(typeof(JBrushes), colorName.Replace("Color", "Brush"));

    private static ComponentResourceKey DimensionKey(string name) => name switch
    {
        "Radius" => JDimensions.Radius,
        "Thickness" => JDimensions.Thickness,
        "Spacing" => JDimensions.Spacing,
        "ControlHeightXs" => JDimensions.ControlHeightXs,
        "ControlHeightSm" => JDimensions.ControlHeightSm,
        "ControlHeightMd" => JDimensions.ControlHeightMd,
        "ControlHeightLg" => JDimensions.ControlHeightLg,
        "ControlFontSizeXs" => JDimensions.ControlFontSizeXs,
        "ControlFontSizeSm" => JDimensions.ControlFontSizeSm,
        "ControlFontSizeMd" => JDimensions.ControlFontSizeMd,
        "ControlFontSizeLg" => JDimensions.ControlFontSizeLg,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown dimension"),
    };

    private IEnumerable<ComponentResourceKey> AllDimensionKeys()
    {
        yield return JDimensions.Radius;
        yield return JDimensions.CornerRadius;
        yield return JDimensions.Thickness;
        yield return JDimensions.BorderThickness;
        yield return JDimensions.Spacing;
        yield return JDimensions.SpacingThickness;
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

    private void RegenerateXaml()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<ResourceDictionary");
        sb.AppendLine("    xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
        sb.AppendLine("    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
        sb.AppendLine("    xmlns:joufflu=\"clr-namespace:Joufflu;assembly=Joufflu\"");
        sb.AppendLine("    xmlns:system=\"clr-namespace:System;assembly=mscorlib\">");
        sb.AppendLine();

        // Colors
        sb.AppendLine("    <!--  Colors  -->");
        foreach (var entry in _allColors)
            sb.AppendLine($"    <Color x:Key=\"{{x:Static joufflu:Colors.{entry.ResourceName}}}\">{ToHex(entry.Color)}</Color>");
        sb.AppendLine();

        // Brushes (one per colour; brush name mirrors the colour name)
        sb.AppendLine("    <!--  Brushes  -->");
        foreach (var entry in _allColors)
        {
            string brushName = entry.ResourceName.Replace("Color", "Brush");
            sb.AppendLine(
                $"    <SolidColorBrush x:Key=\"{{x:Static joufflu:Brushes.{brushName}}}\" Color=\"{{DynamicResource {{x:Static joufflu:Colors.{entry.ResourceName}}}}}\" />");
        }
        sb.AppendLine();

        AppendDimensions(sb);

        sb.Append("</ResourceDictionary>");
        GeneratedXaml = sb.ToString();
    }

    private void AppendDimensions(StringBuilder sb)
    {
        double thickness = DimValue("Thickness");
        double radius = DimValue("Radius");
        double spacing = DimValue("Spacing");

        sb.AppendLine("    <!--  Dimensions  -->");
        sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.Thickness}}\">{Num(thickness)}</system:Double>");
        sb.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.BorderThickness}}\">{Num(thickness)}</Thickness>");
        sb.AppendLine();
        sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.Radius}}\">{Num(radius)}</system:Double>");
        sb.AppendLine($"    <CornerRadius x:Key=\"{{x:Static joufflu:Dimensions.CornerRadius}}\">{Num(radius)}</CornerRadius>");
        sb.AppendLine();
        sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.Spacing}}\">{Num(spacing)}</system:Double>");
        sb.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.SpacingThickness}}\">{Num(spacing)}</Thickness>");
        sb.AppendLine();

        sb.AppendLine("    <!--  Control heights  -->");
        foreach (var name in new[] { "ControlHeightXs", "ControlHeightSm", "ControlHeightMd", "ControlHeightLg" })
            sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.{name}}}\">{Num(DimValue(name))}</system:Double>");
        sb.AppendLine();

        sb.AppendLine("    <!--  Font sizes  -->");
        foreach (var name in new[] { "ControlFontSizeXs", "ControlFontSizeSm", "ControlFontSizeMd", "ControlFontSizeLg" })
            sb.AppendLine($"    <system:Double x:Key=\"{{x:Static joufflu:Dimensions.{name}}}\">{Num(DimValue(name))}</system:Double>");
        sb.AppendLine();

        // Paddings are not editable here, but emit their current values so the file is complete.
        sb.AppendLine("    <!--  Control paddings  -->");
        foreach (var name in new[] { "ControlPaddingXs", "ControlPaddingSm", "ControlPaddingMd", "ControlPaddingLg" })
            sb.AppendLine($"    <Thickness x:Key=\"{{x:Static joufflu:Dimensions.{name}}}\">{PaddingValue(name)}</Thickness>");
        sb.AppendLine();
    }

    private static string PaddingValue(string name)
    {
        ComponentResourceKey key = name switch
        {
            "ControlPaddingXs" => JDimensions.ControlPaddingXs,
            "ControlPaddingSm" => JDimensions.ControlPaddingSm,
            "ControlPaddingMd" => JDimensions.ControlPaddingMd,
            _ => JDimensions.ControlPaddingLg,
        };
        if (Application.Current.TryFindResource(key) is Thickness t)
            return $"{Num(t.Left)},{Num(t.Top)},{Num(t.Right)},{Num(t.Bottom)}";
        return "0";
    }

    private static string ToHex(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static string Num(double value)
        => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    #endregion
}
