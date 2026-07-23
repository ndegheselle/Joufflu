using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace Joufflu.Themes;

/// <summary>
/// Applies and tracks the active Joufflu colour theme application-wide.
/// <para>
/// A theme is a single <see cref="ResourceDictionary"/> kept at the front of
/// <see cref="Application"/>.<c>Resources.MergedDictionaries</c>. Switching swaps that one dictionary;
/// because every control references colours through <c>DynamicResource</c>, the whole UI re-themes live.
/// </para>
/// <para>
/// Themes are selected by name. Three are always available: <see cref="Light"/> and <see cref="Dark"/>
/// (Joufflu's built-in palettes) and <see cref="System"/>, which reads the Windows "apps" theme and keeps
/// following it (via <see cref="SystemEvents.UserPreferenceChanged"/>) — resolving to Light or Dark — until
/// another theme is chosen. Consumers register their own palettes with <see cref="Register(string, Uri, bool)"/>
/// (or <see cref="Register(string, ResourceDictionary, bool)"/>); registered themes become selectable through
/// <see cref="Theme"/> alongside the built-ins.
/// </para>
/// <para>
/// The manager does not persist the selection — an app that wants the theme restored on the next launch saves
/// <see cref="Theme"/> itself (it raises <see cref="ObservableObject.PropertyChanged"/> on every change) and
/// assigns it back before <see cref="Initialize"/>.
/// </para>
/// </summary>
public sealed class ThemeManager : ObservableObject
{
    /// <summary>Name of the built-in theme that follows the Windows apps theme (light/dark) live.</summary>
    public const string System = "System";

    /// <summary>Name of Joufflu's built-in light palette.</summary>
    public const string Light = "Light";

    /// <summary>Name of Joufflu's built-in dark palette.</summary>
    public const string Dark = "Dark";

    private const string LightSource = "pack://application:,,,/Joufflu;component/Themes/Light.xaml";
    private const string DarkSource = "pack://application:,,,/Joufflu;component/Themes/Dark.xaml";

    [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
    public static extern bool ShouldSystemUseDarkMode();

    public static ThemeManager Instance { get; } = new();

    /// <summary>A registered, ready-to-apply theme: a dictionary source and whether it reads as dark.</summary>
    private sealed class Registration
    {
        private readonly Uri? _source;
        private readonly ResourceDictionary? _dictionary;

        public bool IsDark { get; }

        public Registration(Uri source, bool isDark)
        {
            _source = source;
            IsDark = isDark;
        }

        public Registration(ResourceDictionary dictionary, bool isDark)
        {
            _dictionary = dictionary;
            IsDark = isDark;
        }

        /// <summary>
        /// The dictionary to merge. A source-backed theme is rebuilt each time (cheap, avoids sharing a
        /// single instance across swaps); an in-memory theme reuses the instance the consumer supplied.
        /// </summary>
        public ResourceDictionary CreateDictionary()
            => _dictionary ?? new ResourceDictionary { Source = _source };
    }

    // Concrete themes keyed by name (Light, Dark, and any registered by the consumer). "System" is not
    // stored here — it is a resolver that picks Light or Dark from the OS at apply-time.
    private readonly Dictionary<string, Registration> _themes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ObservableCollection<string> _names = new();

    private ResourceDictionary? _themeDictionary;
    private bool _initialized;

    private ThemeManager()
    {
        // Built-ins are always present so "System" can always resolve to one of them.
        _themes[Light] = new Registration(new Uri(LightSource), isDark: false);
        _themes[Dark] = new Registration(new Uri(DarkSource), isDark: true);
        _names.Add(System);
        _names.Add(Light);
        _names.Add(Dark);
        Themes = new ReadOnlyObservableCollection<string>(_names);
    }

    /// <summary>The selectable theme names, in registration order (System, Light, Dark, then any custom ones).</summary>
    public ReadOnlyObservableCollection<string> Themes { get; }

    private string _theme = System;
    /// <summary>
    /// The requested theme, by name. Assigning applies it immediately (once <see cref="Initialize"/> has run).
    /// Assigning a name that is not registered falls back to the system theme until a theme with that name is
    /// registered.
    /// </summary>
    public string Theme
    {
        get => _theme;
        set
        {
            if (!SetProperty(ref _theme, value))
                return;

            if (_initialized)
                Apply();
        }
    }

    /// <summary>The theme actually on screen — <see cref="Theme"/> with <c>System</c> resolved to Light or Dark.</summary>
    public bool IsDark { get; private set; }

    /// <summary>
    /// Registers (or replaces) a custom theme loaded from a resource-dictionary <paramref name="source"/>,
    /// making it selectable by <paramref name="name"/> through <see cref="Theme"/>. Call before
    /// <see cref="Initialize"/> so a persisted custom selection can be restored; registering the theme that is
    /// currently selected re-applies it live.
    /// </summary>
    /// <param name="name">The selectable name. Reusing an existing name (including a built-in) replaces it.</param>
    /// <param name="source">A pack/absolute <see cref="Uri"/> to the theme <see cref="ResourceDictionary"/>.</param>
    /// <param name="isDark">Whether the theme reads as dark; surfaced through <see cref="IsDark"/> while selected.</param>
    public void Register(string name, Uri source, bool isDark = false)
        => Register(name, new Registration(source, isDark));

    /// <summary>
    /// Registers (or replaces) a custom theme from an in-memory <paramref name="dictionary"/>, making it
    /// selectable by <paramref name="name"/> through <see cref="Theme"/>. Call before <see cref="Initialize"/>
    /// so a persisted custom selection can be restored; registering the theme that is currently selected
    /// re-applies it live.
    /// </summary>
    /// <param name="name">The selectable name. Reusing an existing name (including a built-in) replaces it.</param>
    /// <param name="dictionary">The theme <see cref="ResourceDictionary"/> to merge when selected.</param>
    /// <param name="isDark">Whether the theme reads as dark; surfaced through <see cref="IsDark"/> while selected.</param>
    public void Register(string name, ResourceDictionary dictionary, bool isDark = false)
        => Register(name, new Registration(dictionary, isDark));

    private void Register(string name, Registration registration)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Theme name must not be empty.", nameof(name));
        if (string.Equals(name, System, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"'{System}' is reserved for the OS-following theme.", nameof(name));

        bool isNew = !_themes.ContainsKey(name);
        _themes[name] = registration;
        if (isNew)
            _names.Add(name);

        // If this is the theme on screen (or the persisted-but-not-yet-registered selection), show it now.
        if (_initialized && string.Equals(name, _theme, StringComparison.OrdinalIgnoreCase))
            Apply();
    }

    /// <summary>
    /// Removes a custom theme. The built-in <see cref="System"/>, <see cref="Light"/> and <see cref="Dark"/>
    /// themes cannot be removed. Removing the selected theme falls back to the system theme.
    /// </summary>
    /// <returns><c>true</c> if a theme was removed.</returns>
    public bool Unregister(string name)
    {
        if (string.Equals(name, System, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, Light, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, Dark, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!_themes.Remove(name))
            return false;

        // Drop the name using the stored casing so the observable collection stays in sync.
        for (int i = 0; i < _names.Count; i++)
        {
            if (string.Equals(_names[i], name, StringComparison.OrdinalIgnoreCase))
            {
                _names.RemoveAt(i);
                break;
            }
        }

        if (string.Equals(name, _theme, StringComparison.OrdinalIgnoreCase))
            Theme = System;

        return true;
    }

    /// <summary>
    /// Builds the resource dictionary for a concrete registered theme (its colours, brushes, …) so its
    /// palette can be inspected without selecting it. Returns <c>null</c> for <see cref="System"/> (a
    /// resolver, not a concrete theme) or an unknown name.
    /// </summary>
    public ResourceDictionary? GetDictionary(string name)
    {
        if (string.Equals(name, System, StringComparison.OrdinalIgnoreCase))
            return null;

        return _themes.TryGetValue(name, out Registration? registration)
            ? registration.CreateDictionary()
            : null;
    }

    /// <summary>
    /// Inserts the theme dictionary for the current <see cref="Theme"/> and starts following the OS theme.
    /// Call once, before the first window is shown (typically in <c>App.OnStartup</c>). Register any custom
    /// themes and assign a restored <see cref="Theme"/> first so the right theme is shown from the start.
    /// </summary>
    public void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;

        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        Apply();
    }

    /// <summary>Resolves the requested theme against the OS and swaps in the matching theme dictionary.</summary>
    private void Apply()
    {
        Registration registration = Resolve(_theme);

        var next = registration.CreateDictionary();

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

        IsDark = registration.IsDark;
        OnPropertyChanged(nameof(IsDark));
    }

    /// <summary>
    /// Maps a selected name to the concrete theme to show: <see cref="System"/> (or an unknown/not-yet-registered
    /// name) resolves to Light or Dark from the OS; any registered name resolves to itself.
    /// </summary>
    private Registration Resolve(string name)
    {
        if (!string.Equals(name, System, StringComparison.OrdinalIgnoreCase)
            && _themes.TryGetValue(name, out Registration? registration))
            return registration;

        return ShouldSystemUseDarkMode() ? _themes[Dark] : _themes[Light];
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        // Only the "General" category carries the light/dark switch, and only System mode follows it.
        if (e.Category != UserPreferenceCategory.General || !string.Equals(_theme, System, StringComparison.OrdinalIgnoreCase))
            return;

        // The event arrives off the UI thread; touch resources on the dispatcher.
        Application.Current?.Dispatcher.Invoke(Apply);
    }
}
