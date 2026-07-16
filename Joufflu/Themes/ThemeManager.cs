using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace Joufflu.Themes;

/// <summary>The theme a consumer can ask for.</summary>
public enum ThemeMode
{
    /// <summary>Follow the Windows apps theme and track it while the app runs.</summary>
    System,
    Light,
    Dark,
}

/// <summary>
/// Applies and tracks the active Joufflu colour theme application-wide.
/// <para>
/// The theme is a single <see cref="ResourceDictionary"/> (Light or Dark) kept at the front of
/// <see cref="Application"/>.<c>Resources.MergedDictionaries</c>. Switching swaps that one dictionary;
/// because every control references colours through <c>DynamicResource</c>, the whole UI re-themes live.
/// </para>
/// <para>
/// In <see cref="ThemeMode.System"/> the manager reads the Windows "apps" theme and keeps following it
/// (via <see cref="SystemEvents.UserPreferenceChanged"/>) until another mode is chosen. The selected
/// <see cref="Mode"/> is persisted so it is restored on the next launch.
/// </para>
/// </summary>
public sealed class ThemeManager : ObservableObject
{
    private const string LightSource = "pack://application:,,,/Joufflu;component/Themes/Light.xaml";
    private const string DarkSource = "pack://application:,,,/Joufflu;component/Themes/Dark.xaml";

    [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
    public static extern bool ShouldSystemUseDarkMode();

    public static ThemeManager Instance { get; } = new();

    private ResourceDictionary? _themeDictionary;
    private bool _initialized;

    private ThemeManager() { }

    private ThemeMode _mode = ThemeMode.System;
    /// <summary>
    /// The requested theme. Assigning applies it immediately (once <see cref="Initialize"/> has run)
    /// and persists the choice.
    /// </summary>
    public ThemeMode Mode
    {
        get => _mode;
        set
        {
            if (!SetProperty(ref _mode, value))
                return;

            Save(value);
            if (_initialized)
                Apply();
        }
    }

    /// <summary>The theme actually on screen — <see cref="Mode"/> with <c>System</c> resolved to Light or Dark.</summary>
    public bool IsDark { get; private set; }

    /// <summary>
    /// Loads the persisted <see cref="Mode"/>, inserts the theme dictionary and starts following the OS
    /// theme. Call once, before the first window is shown (typically in <c>App.OnStartup</c>).
    /// </summary>
    public void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;

        _mode = Load();
        OnPropertyChanged(nameof(Mode));

        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        Apply();
    }

    /// <summary>Resolves the requested mode against the OS and swaps in the matching theme dictionary.</summary>
    private void Apply()
    {
        bool isDark = _mode switch
        {
            ThemeMode.Light => false,
            ThemeMode.Dark => true,
            _ => ShouldSystemUseDarkMode(),
        };

        var next = new ResourceDictionary { Source = new Uri(isDark ? DarkSource : LightSource) };

        var merged = Application.Current.Resources.MergedDictionaries;
        if (_themeDictionary is not null)
        {
            // Swap in place so the theme keeps its position ahead of the control styles.
            int index = merged.IndexOf(_themeDictionary);
            if (index >= 0)
                merged[index] = next;
            else
                merged.Insert(0, next);
        }
        else
        {
            merged.Insert(0, next);
        }

        _themeDictionary = next;

        IsDark = isDark;
        OnPropertyChanged(nameof(IsDark));
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        // Only the "General" category carries the light/dark switch, and only System mode follows it.
        if (e.Category != UserPreferenceCategory.General || _mode != ThemeMode.System)
            return;

        // The event arrives off the UI thread; touch resources on the dispatcher.
        Application.Current?.Dispatcher.Invoke(Apply);
    }

    #region Persistence

    private sealed class Settings
    {
        public ThemeMode ThemeMode { get; set; }
    }

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "joufflu",
        "settings.json");

    private static ThemeMode Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return ThemeMode.System;
            var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));
            return settings?.ThemeMode ?? ThemeMode.System;
        }
        catch
        {
            return ThemeMode.System;
        }
    }

    private static void Save(ThemeMode mode)
    {
        try
        {
            string path = SettingsPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new Settings { ThemeMode = mode }));
        }
        catch
        {
            // Persistence is best-effort; a failure to save must not break theming.
        }
    }

    #endregion
}
