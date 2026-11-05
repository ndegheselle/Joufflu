using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Samples.Views.Toolkit;

public partial class TooltipSamples : UserControl
{
    public TooltipSamples()
    {
        InitializeComponent();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        // The sample popup stays open on its own: it must not outlive the page.
        PopupAnchor.IsChecked = false;
    }
}
