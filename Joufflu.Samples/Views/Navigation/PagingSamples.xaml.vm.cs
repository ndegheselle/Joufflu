using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Navigation
{
    public class PagingSamplesViewModel : ObservableObject
    {
        public string Code =>
    "// The overlay content owns its buttons and closes itself\n" +
    "// via the service, e.g. overlays.CloseTop(true/false).\n" +
    "var content = new DeleteConfirmViewModel(overlays, \"Delete?\");\n" +
    "var options = new OverlayOptions { Title = \"Please confirm\" };\n" +
    "bool? result = await overlays.Show(content, options);";
    }
}
