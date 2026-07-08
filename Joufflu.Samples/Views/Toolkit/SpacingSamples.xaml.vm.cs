using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class SpacingSamplesViewModel : ObservableObject
{
    public string GapCode =>
        "<!-- Gap adds spacing between children of any panel -->\n" +
        "<!-- StackPanel: gap along the Orientation axis -->\n" +
        "<StackPanel Orientation=\"Horizontal\" joufflu:Spacing.Gap=\"8\">\n" +
        "    <Button>One</Button> <Button>Two</Button> <Button>Three</Button>\n" +
        "</StackPanel>\n\n" +
        "<!-- WrapPanel: gap on both axes, or per axis -->\n" +
        "<WrapPanel joufflu:Spacing.HorizontalGap=\"8\"\n" +
        "           joufflu:Spacing.VerticalGap=\"12\">\n" +
        "    ...\n" +
        "</WrapPanel>\n\n" +
        "<!-- Grid: gap sits between rows and columns -->\n" +
        "<Grid joufflu:Spacing.Gap=\"12\">\n" +
        "    ...\n" +
        "</Grid>";
}
