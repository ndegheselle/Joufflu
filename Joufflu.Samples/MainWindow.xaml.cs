using Joufflu.Controls;
using Joufflu.Samples.ViewModels;

namespace Joufflu.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        public MainWindow(AppViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}