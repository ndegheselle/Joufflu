using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Joufflu.Samples.Views.Natives.DataInput;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class TextBoxSamplesViewModel : ObservableValidator
{
    private string _text = "Hello";
    private string _username = "";

    public TextBoxSamplesViewModel() => ValidateProperty(Username, nameof(Username));

    public string Text { get => _text; set => SetProperty(ref _text, value); }

    [Required(ErrorMessage = "A username is required.")]
    [MinLength(3, ErrorMessage = "A username has at least 3 characters.")]
    [RegularExpression("^[a-z0-9]*$", ErrorMessage = "Only lowercase letters and digits.")]
    public string Username { get => _username; set => SetProperty(ref _username, value, validate: true); }

    public string TextCode => "<TextBox Text=\"{Binding Text}\" />";

    public string SizesCode =>
        "<TextBox toolkit:Sizing.Size=\"sm\" />\n" +
        "<TextBox />\n" +
        "<TextBox toolkit:Sizing.Size=\"lg\" />";

    public string ValidationCode =>
        "<!-- Any INotifyDataErrorInfo / IDataErrorInfo / ValidationRule error -->\n" +
        "<TextBox Text=\"{Binding Username, UpdateSourceTrigger=PropertyChanged}\" />\n\n" +
        "<!-- Restyle every input at once by redefining the key -->\n" +
        "<ControlTemplate x:Key=\"ValidationErrorTemplate\"> ... </ControlTemplate>";

    public string OtherCode =>
        "<PasswordBox />\n" +
        "<TextBox AcceptsReturn=\"True\" TextWrapping=\"Wrap\" />";
}
