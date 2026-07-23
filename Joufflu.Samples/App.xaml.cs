using Joufflu.Samples.ViewModels;
using Joufflu.Themes;
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

            // Restore the persisted theme (or follow the system) and insert its dictionary before any window is shown.
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