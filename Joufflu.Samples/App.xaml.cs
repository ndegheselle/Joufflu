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

            // Register a custom theme so it can be selected alongside the built-in System/Light/Dark.
            // Register before Initialize() so a persisted "Ocean" selection is restored on launch.
            ThemeManager.Instance.Register(
                "Ocean",
                new Uri("pack://application:,,,/Joufflu.Samples;component/Themes/Ocean.xaml"),
                isDark: true);

            // Restore the persisted theme (or follow the system) and insert its dictionary before any window is shown.
            ThemeManager.Instance.Initialize();

            appViewModel = new AppViewModel();

            MainWindow mainWindow = new MainWindow(appViewModel);
            mainWindow.Show();
        }

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