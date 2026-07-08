using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Navigation;
using Joufflu.Samples.ViewModels;

namespace Joufflu.Samples.Views.Controls.Navigation;

public class OverlaySamplesViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;
    private readonly IToastService _toasts;

    public IRelayCommand OpenSimpleCommand { get; }

    public IRelayCommand OpenConfirmCommand { get; }

    public IRelayCommand OpenFormCommand { get; }

    public IRelayCommand OpenStackedCommand { get; }

    public OverlaySamplesViewModel(IOverlayService overlays, IToastService toasts)
    {
        _overlays = overlays;
        _toasts = toasts;

        OpenSimpleCommand = new RelayCommand(OpenSimple);
        OpenConfirmCommand = new AsyncRelayCommand(OpenConfirmAsync);
        OpenFormCommand = new AsyncRelayCommand(OpenFormAsync);
        OpenStackedCommand = new RelayCommand(OpenStacked);
    }

    private void OpenSimple()
    {
        var content = new ConfirmViewModel("This is a simple overlay. Click the cross or the dimmed background to dismiss it.");
        _overlays.Show(content, new OverlayOptions { Title = "Simple overlay" });
    }

    private async Task OpenConfirmAsync()
    {
        var content = new ConfirmViewModel("Delete the selected item? This action cannot be undone.");
        var options = new OverlayOptions { Title = "Please confirm", CloseOnClickAway = false };
        options.Actions.Add(new OverlayAction { Label = "Cancel", Style = OverlayActionStyle.Secondary, Result = false });
        options.Actions.Add(new OverlayAction { Label = "Delete", Style = OverlayActionStyle.Danger, Result = true });

        bool? result = await _overlays.Show(content, options);
        if (result == true)
            _toasts.Success("Item deleted.", "Confirmed");
        else
            _toasts.Info("Cancelled.");
    }

    private async Task OpenFormAsync()
    {
        var form = new SampleFormViewModel();
        var options = new OverlayOptions { Title = "Edit profile", CloseOnClickAway = false };
        options.Actions.Add(new OverlayAction { Label = "Cancel", Style = OverlayActionStyle.Secondary, Result = false });
        options.Actions.Add(new OverlayAction { Label = "Save", Style = OverlayActionStyle.Primary, Result = true });

        bool? result = await _overlays.Show(form, options);
        if (result == true)
            _toasts.Success($"Saved name: {form.Name}", "Profile");
    }

    private void OpenStacked()
    {
        _overlays.Show(
            new ConfirmViewModel("First overlay. Open another one on top to see overlays stack."),
            new OverlayOptions { Title = "Overlay 1" });

        _overlays.Show(
            new ConfirmViewModel("Second overlay, stacked above the first. Close me to reveal it."),
            new OverlayOptions { Title = "Overlay 2" });
    }

    public string Code =>
        "// Inject the service, then await a result\n" +
        "var options = new OverlayOptions { Title = \"Please confirm\" };\n" +
        "options.Actions.Add(new OverlayAction { Label = \"Cancel\", Result = false });\n" +
        "options.Actions.Add(new OverlayAction { Label = \"Delete\",\n" +
        "    Style = OverlayActionStyle.Danger, Result = true });\n" +
        "bool? result = await overlays.Show(new ConfirmViewModel(...), options);";
}
