using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

/// <summary>An <see cref="ObservableValidator"/> so the validation sample has errors to report.</summary>
public class TextEditableSamplesViewModel : ObservableValidator
{
    private string _editableText = "Double-click to edit me";
    private string _shortText = "Far too long a title";

    public TextEditableSamplesViewModel() => ValidateAllProperties();

    public string EditableText { get => _editableText; set => SetProperty(ref _editableText, value); }

    [Required(ErrorMessage = "A title is required.")]
    [MaxLength(10, ErrorMessage = "A title has at most 10 characters.")]
    public string ShortText { get => _shortText; set => SetProperty(ref _shortText, value, validate: true); }

    public string TextEditableCode =>
        "<inputs:TextEditable Text=\"{Binding EditableText, Mode=TwoWay}\" />";

    public string ValidationCode =>
        "<inputs:TextEditable Text=\"{Binding ShortText, Mode=TwoWay}\" />\n\n" +
        "<!-- Opt out of the template: only the input's own red text and border remain -->\n" +
        "<inputs:TextEditable Validation.ErrorTemplate=\"{x:Null}\" Text=\"{Binding ShortText, Mode=TwoWay}\" />";
}
