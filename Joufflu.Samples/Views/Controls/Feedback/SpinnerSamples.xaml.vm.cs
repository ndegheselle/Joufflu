using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Controls.Feedback;

public class SpinnerSamplesViewModel : ObservableObject
{
    public string Code =>
        "<feedback:Spinner />\n" +
        "<feedback:Spinner joufflu:Sizing.Size=\"lg\" />";
}
