using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

public class TypographySamplesViewModel : ObservableObject
{
    public string HeadingsCode =>
        "<TextBlock Style=\"{StaticResource H1}\" Text=\"Heading 1\" />\n" +
        "<TextBlock Style=\"{StaticResource H2}\" Text=\"Heading 2\" />\n" +
        "<TextBlock Style=\"{StaticResource H3}\" Text=\"Heading 3\" />\n" +
        "<TextBlock Style=\"{StaticResource H4}\" Text=\"Heading 4\" />\n" +
        "<TextBlock Style=\"{StaticResource H5}\" Text=\"Heading 5\" />\n" +
        "<TextBlock Style=\"{StaticResource H6}\" Text=\"Heading 6\" />";

    public string BodyCode =>
        "<TextBlock Style=\"{StaticResource Lead}\" Text=\"Lead paragraph\" />\n" +
        "<TextBlock Text=\"Default body text\" />\n" +
        "<TextBlock Style=\"{StaticResource Muted}\" Text=\"Muted text\" />\n" +
        "<TextBlock Style=\"{StaticResource Small}\" Text=\"Small text\" />";
}
