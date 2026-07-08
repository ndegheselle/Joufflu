using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.Feedback;

public class ProgressBarSamplesViewModel : ObservableObject
{
    private double _progress = 65;

    public double Progress { get => _progress; set => SetProperty(ref _progress, value); }

    public string Code => "<ProgressBar Maximum=\"100\" Value=\"{Binding Progress}\" />";

    public string IndeterminateCode => "<ProgressBar IsIndeterminate=\"True\" />";
}
