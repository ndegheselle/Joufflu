using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Feedback;
using Joufflu.Feedback.Controls;

namespace Joufflu.Samples.Views.Feedback;

public class ToastSamplesViewModel : ObservableObject
{
    private readonly IToastService _toasts;
    private int _counter;
    private ToastPosition _position = ToastPosition.TopRight;

    /// <summary>Corner the shell's <c>ToastContainer</c> stacks its toasts in.</summary>
    public ToastPosition Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    public ICommand InfoCommand { get; }

    public ICommand SuccessCommand { get; }

    public ICommand WarningCommand { get; }

    public ICommand ErrorCommand { get; }

    public ICommand StickyCommand { get; }

    public ICommand StackCommand { get; }

    public ToastSamplesViewModel(IToastService toasts)
    {
        _toasts = toasts;

        InfoCommand = new RelayCommand(() => _toasts.Info("A neutral, informational message.", "Heads up"));
        SuccessCommand = new RelayCommand(() => _toasts.Success("Your changes were saved.", "Success"));
        WarningCommand = new RelayCommand(() => _toasts.Warning("Oops. Something want.", "Warning"));
        ErrorCommand = new RelayCommand(() => _toasts.Error("Something went wrong.", "Error"));
        StickyCommand = new RelayCommand(() => _toasts.Show(new ToastOptions
        {
            Type = ToastType.Info,
            Title = "Sticky",
            Message = "I stay until you close me.",
            Duration = TimeSpan.Zero
        }));
        StackCommand = new RelayCommand(() =>
        {
            for (int i = 0; i < 3; i++)
                _toasts.Info($"Stacked toast #{++_counter}");
        });
    }

    public string PositionCode =>
        "<feedback:ToastContainer Toasts=\"{Binding Toasts}\"\n" +
        "                         Position=\"BottomRight\">\n" +
        "    <!-- the whole app -->\n" +
        "</feedback:ToastContainer>";

    public string Code =>
        "// Inject IToastService\n" +
        "toasts.Info(\"A neutral message.\", \"Heads up\");\n" +
        "toasts.Success(\"Saved.\");\n" +
        "toasts.Warning(\"Careful.\");\n" +
        "toasts.Error(\"Failed.\");\n" +
        "toasts.Show(new ToastOptions { Message = \"Sticky\", Duration = TimeSpan.Zero });";
}
