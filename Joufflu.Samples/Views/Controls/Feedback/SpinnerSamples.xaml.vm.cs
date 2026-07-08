using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Controls.Feedback;

public class SpinnerSamplesViewModel : ObservableObject
{
    public string Code =>
        "<controls:Spinner />\n" +
        "<controls:Spinner joufflu:ControlProperties.Size=\"lg\" />";
}
