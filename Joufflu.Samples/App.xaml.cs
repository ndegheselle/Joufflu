using Joufflu.Samples.ViewModels;
using Joufflu.Themes;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace Joufflu.Samples
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AppViewModel? appViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            // Register custom themes so they can be selected alongside the built-in System/Light/Dark
            // (and offered as presets by the theme customizer). Register before Initialize() so a
            // persisted custom selection is restored on launch.
            RegisterTheme("Ocean", isDark: true);
            RegisterTheme("Cupcake", isDark: false);
            RegisterTheme("Emerald", isDark: false);
            RegisterTheme("Corporate", isDark: false);
            RegisterTheme("Nord", isDark: false);
            RegisterTheme("Synthwave", isDark: true);
            RegisterTheme("Dracula", isDark: true);

            // The ThemeManager does not persist the selection itself — each app saves the parameters it cares
            // about however it likes. Here: restore the saved theme name, then keep it in sync on every change.
            string? savedTheme = LoadThemeSetting();
            if (!string.IsNullOrWhiteSpace(savedTheme))
                ThemeManager.Instance.Theme = savedTheme;
            ThemeManager.Instance.PropertyChanged += OnThemeManagerPropertyChanged;

            // Insert the current theme's dictionary before any window is shown.
            ThemeManager.Instance.Initialize();

            appViewModel = new AppViewModel();

            MainWindow mainWindow = new MainWindow(appViewModel);
            mainWindow.Show();
        }

        /// <summary>Registers a theme dictionary shipped under <c>Themes/&lt;name&gt;.xaml</c>.</summary>
        private static void RegisterTheme(string name, bool isDark) =>
            ThemeManager.Instance.Register(
                name,
                new Uri($"pack://application:,,,/Joufflu.Samples;component/Themes/{name}.xaml"),
                isDark);

        #region Theme persistence

        // Where this sample keeps its settings. A real app would fold the theme into its own settings store.
        private static string ThemeSettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "joufflu.samples",
            "settings.json");

        private sealed class ThemeSettings
        {
            public string? Theme { get; set; }
        }

        private void OnThemeManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeManager.Theme))
                SaveThemeSetting(ThemeManager.Instance.Theme);
        }

        private static string? LoadThemeSetting()
        {
            try
            {
                if (!File.Exists(ThemeSettingsPath))
                    return null;

                return JsonSerializer.Deserialize<ThemeSettings>(File.ReadAllText(ThemeSettingsPath))?.Theme;
            }
            catch
            {
                return null;
            }
        }

        private static void SaveThemeSetting(string theme)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ThemeSettingsPath)!);
                File.WriteAllText(ThemeSettingsPath, JsonSerializer.Serialize(new ThemeSettings { Theme = theme }));
            }
            catch
            {
                // Persistence is best-effort; a failure to save must not break theming.
            }
        }

        #endregion

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                HandleException(exception);
            }
        }

        private void HandleException(Exception exception)
        {
            try
            {
                appViewModel?.Toasts.Error("An unexpected error happend ...", "Ooops");
            }
            catch
            {}
        }
    }
}