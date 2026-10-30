using Joufflu.Controls;
using System.Windows;
using Joufflu.Samples.ViewModels;
using Joufflu.Samples.Views.Themes;
using Joufflu.Themes;

namespace Joufflu.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        public MainWindow(ShellViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }

        // One customizer at a time : a second click brings the open one back to the front.
        private ThemeCustomizerWindow? _themeCustomizer;

        private void ThemeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.Theme = ThemeManager.Instance.IsDark ? ThemeManager.Light : ThemeManager.Dark;
        }

        private void ThemeCustomizer_Click(object sender, RoutedEventArgs e)
        {
            if (_themeCustomizer is not null)
            {
                _themeCustomizer.Activate();
                return;
            }

            _themeCustomizer = new ThemeCustomizerWindow { Owner = this };
            _themeCustomizer.Closed += (_, _) => _themeCustomizer = null;
            _themeCustomizer.Show();
        }
    }
}