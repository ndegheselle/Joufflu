using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class AnimateSamplesViewModel : ObservableObject
{
    /// <summary>Drives the bound example so the hop can be started and stopped from the page.</summary>
    private bool _hasUnread = true;
    public bool HasUnread
    {
        get => _hasUnread;
        set => SetProperty(ref _hasUnread, value);
    }

    /// <summary>Drives the bound spin example so the turn can be started and stopped from the page.</summary>
    private bool _isRefreshing = true;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public string BounceCode =>
        "<!-- Bounce loops a vertical hop on any element, template untouched -->\n" +
        "<Button toolkit:Animate.Bounce=\"True\">Look at me</Button>\n\n" +
        "<!-- Tune the hop: height in DIPs, duration of a full cycle (hop + pause) -->\n" +
        "<fonts:FontIcon\n" +
        "    toolkit:Animate.Bounce=\"True\"\n" +
        "    toolkit:Animate.BounceHeight=\"10\"\n" +
        "    toolkit:Animate.BounceDuration=\"0:0:0.6\"\n" +
        "    Text=\"{x:Static fonts:LucideFontIcons.Bell}\" />\n\n" +
        "<!-- Hidden elements pause their own hop: no frames spent on a -->\n" +
        "<!-- bounce nobody can see, resumed when the element is back   -->\n" +
        "<Border Visibility=\"Collapsed\" toolkit:Animate.Bounce=\"True\" />";

    public string BindingCode =>
        "<!-- Bounce is a plain bool, so bind it to start and stop the loop -->\n" +
        "<feedback:Badge toolkit:Animate.Bounce=\"{Binding HasUnread}\">3</feedback:Badge>\n\n" +
        "<!-- A transform already on the element is kept and composed with -->\n" +
        "<!-- the bounce, then restored once Bounce goes back to False     -->\n" +
        "<Border toolkit:Animate.Bounce=\"{Binding HasUnread}\">\n" +
        "    <Border.RenderTransform>\n" +
        "        <RotateTransform Angle=\"8\" />\n" +
        "    </Border.RenderTransform>\n" +
        "</Border>";

    public string SpinCode =>
        "<!-- Spin turns any element around its centre, template untouched -->\n" +
        "<fonts:FontIcon toolkit:Animate.Spin=\"True\" Text=\"{x:Static fonts:LucideFontIcons.Loader}\" />\n\n" +
        "<!-- SpinDuration is the time of one full turn -->\n" +
        "<fonts:FontIcon\n" +
        "    toolkit:Animate.Spin=\"True\"\n" +
        "    toolkit:Animate.SpinDuration=\"0:0:3\"\n" +
        "    Text=\"{x:Static fonts:LucideFontIcons.Settings}\" />\n\n" +
        "<!-- A plain bool too: spin a refresh icon only while it works -->\n" +
        "<fonts:FontIcon\n" +
        "    toolkit:Animate.Spin=\"{Binding IsRefreshing}\"\n" +
        "    Text=\"{x:Static fonts:LucideFontIcons.RefreshCw}\" />";
}
