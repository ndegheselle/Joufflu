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
        private MainViewModel? mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            // Restore the persisted theme (or follow the system) and insert its dictionary before any window is shown.
            ThemeManager.Instance.Initialize();

            mainViewModel = new MainViewModel();

            MainWindow mainWindow = new MainWindow(mainViewModel);
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
                mainViewModel?.Toasts.Error("An unexpected error happend ...", "Ooops");
            }
            catch
            {}
        }
    }
}