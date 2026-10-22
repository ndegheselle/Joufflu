using Joufflu.Controls;
using System.Windows;
using Joufflu.Samples.ViewModels;
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

        private void ThemeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.Theme = ThemeManager.Instance.IsDark ? ThemeManager.Light : ThemeManager.Dark;
        }
    }
}