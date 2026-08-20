using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Navigation.Controls;

/// <summary>
/// Hosts a full page: the header sits in the title bar strip at the top, the content
/// scrolls below it.
/// </summary>
public class FullContainer : HeaderedContentControl
{
    static FullContainer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FullContainer),
            new FrameworkPropertyMetadata(typeof(FullContainer)));
    }
}
