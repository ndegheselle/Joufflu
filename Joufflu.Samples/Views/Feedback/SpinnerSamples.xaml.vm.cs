using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Feedback;

public class SpinnerSamplesViewModel : ObservableObject
{
    public string Code =>
        "<feedback:Spinner toolkit:Sizing.Size=\"xs\" />\n" +
        "<feedback:Spinner toolkit:Sizing.Size=\"sm\" />\n" +
        "<feedback:Spinner />\n" +
        "<feedback:Spinner toolkit:Sizing.Size=\"lg\" />\n" +
        "<feedback:Spinner toolkit:Sizing.Size=\"lg\"\n" +
        "                  Foreground=\"{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}\" />";
}
