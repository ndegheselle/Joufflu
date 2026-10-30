using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace Joufflu.Samples.Views.Toolkit;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class CustomControlSamplesViewModel : ObservableValidator
{
    private int _rating = 3;
    private int _requiredRating;

    /// <param name="openDesignTokens">Opens the Design tokens page, which lists the tokens of each category.</param>
    public CustomControlSamplesViewModel(Action openDesignTokens)
    {
        OpenDesignTokensCommand = new RelayCommand(openDesignTokens);
        ValidateAllProperties();
    }

    public IRelayCommand OpenDesignTokensCommand { get; }

    public int Rating { get => _rating; set => SetProperty(ref _rating, value); }

    [Range(1, int.MaxValue, ErrorMessage = "Give at least one star.")]
    public int RequiredRating { get => _requiredRating; set => SetProperty(ref _requiredRating, value, validate: true); }

    public string MinimumCode =>
        "<Style x:Key=\"RatingInputMinimal\" TargetType=\"{x:Type local:RatingInput}\">\n" +
        "    <Setter Property=\"Background\" Value=\"{DynamicResource {x:Static joufflu:Brushes.InputBaseBrush}}\" />\n" +
        "    <Setter Property=\"Padding\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.InputPaddingMd}}\" />\n" +
        "    <Setter Property=\"BorderBrush\" Value=\"{DynamicResource {x:Static joufflu:Brushes.InputBorderBrush}}\" />\n" +
        "    <Setter Property=\"BorderThickness\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.InputThickness}}\" />\n" +
        "    <Setter Property=\"Height\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.HeightMd}}\" />\n" +
        "    <Setter Property=\"Template\" Value=\"{StaticResource RatingInputTemplate}\" />\n" +
        "</Style>\n\n" +
        "<!-- In the template, the surface reads the control's properties; the radius is clamped to half the height -->\n" +
        "<Border Background=\"{TemplateBinding Background}\"\n" +
        "        BorderBrush=\"{TemplateBinding BorderBrush}\"\n" +
        "        BorderThickness=\"{TemplateBinding BorderThickness}\"\n" +
        "        Tag=\"{DynamicResource {x:Static joufflu:Dimensions.InputRadius}}\">\n" +
        "    <Border.CornerRadius>\n" +
        "        <MultiBinding Converter=\"{StaticResource MaxCornerRadiusConverter}\">\n" +
        "            <Binding Path=\"Tag\" RelativeSource=\"{RelativeSource Self}\" />\n" +
        "            <Binding Path=\"ActualWidth\" RelativeSource=\"{RelativeSource Self}\" />\n" +
        "            <Binding Path=\"ActualHeight\" RelativeSource=\"{RelativeSource Self}\" />\n" +
        "        </MultiBinding>\n" +
        "    </Border.CornerRadius>\n" +
        "</Border>";

    public string SizesCode =>
        "<Setter Property=\"FontSize\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.FontSizeMd}}\" />\n" +
        "<Setter Property=\"Padding\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.InputPaddingMd}}\" />\n\n" +
        "<Style.Triggers>\n" +
        "    <Trigger Property=\"IsKeyboardFocusWithin\" Value=\"True\">\n" +
        "        <Setter Property=\"BorderBrush\" Value=\"{DynamicResource {x:Static joufflu:Brushes.InputElevatedBrush}}\" />\n" +
        "        <Setter Property=\"Background\" Value=\"{DynamicResource {x:Static joufflu:Brushes.InputInteractionBrush}}\" />\n" +
        "    </Trigger>\n" +
        "    <Trigger Property=\"toolkit:Sizing.Size\" Value=\"xs\">\n" +
        "        <Setter Property=\"Height\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.HeightXs}}\" />\n" +
        "        <Setter Property=\"FontSize\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.FontSizeXs}}\" />\n" +
        "        <Setter Property=\"Padding\" Value=\"{DynamicResource {x:Static joufflu:Dimensions.InputPaddingXs}}\" />\n" +
        "    </Trigger>\n" +
        "    <!-- same for sm and lg -->\n" +
        "</Style.Triggers>\n\n" +
        "<!-- In the template, the height comes from the size: only the horizontal part of Padding, content centred -->\n" +
        "<Grid VerticalAlignment=\"Center\"\n" +
        "      Margin=\"{TemplateBinding Padding, Converter={StaticResource HorizontalThicknessConverter}}\"> ... </Grid>";

    public string ValidationCode =>
        "<Setter Property=\"Validation.ErrorTemplate\" Value=\"{DynamicResource ValidationErrorTemplate}\" />\n\n" +
        "<!-- In Style.Triggers, after focus so it wins over it -->\n" +
        "<Trigger Property=\"Validation.HasError\" Value=\"True\">\n" +
        "    <Setter Property=\"BorderBrush\" Value=\"{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}\" />\n" +
        "</Trigger>\n\n" +
        "<local:RatingInput Value=\"{Binding RequiredRating}\" />\n" +
        "<!-- Opt out of the badge: only the red border remains -->\n" +
        "<local:RatingInput Validation.ErrorTemplate=\"{x:Null}\" Value=\"{Binding RequiredRating}\" />";

    public string EmbeddedCode =>
        "<!-- A StaticResource only sees its own dictionary and what it merges -->\n" +
        "<ResourceDictionary.MergedDictionaries>\n" +
        "    <ResourceDictionary Source=\"pack://application:,,,/Joufflu;component/Styles/Natives/Actions/Button.named.xaml\" />\n" +
        "</ResourceDictionary.MergedDictionaries>\n\n" +
        "<Grid VerticalAlignment=\"Center\"\n" +
        "      Margin=\"{TemplateBinding Padding, Converter={StaticResource HorizontalThicknessConverter}}\">\n" +
        "    <Grid.ColumnDefinitions>\n" +
        "        <ColumnDefinition />\n" +
        "        <ColumnDefinition Width=\"Auto\" />\n" +
        "    </Grid.ColumnDefinitions>\n" +
        "    <!-- ... the stars, EmbeddedButton too ... -->\n" +
        "    <Button Grid.Column=\"1\"\n" +
        "            Command=\"{Binding ClearCommand, RelativeSource={RelativeSource TemplatedParent}}\"\n" +
        "            Style=\"{StaticResource EmbeddedButton}\">\n" +
        "        <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.x}\" />\n" +
        "    </Button>\n" +
        "</Grid>";
}
