using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class TextEditableSamplesViewModel : ObservableObject
{
    private string _editableText = "Double-click to edit me";

    public string EditableText { get => _editableText; set => SetProperty(ref _editableText, value); }

    public string TextEditableCode =>
        "<inputs:TextEditable Text=\"{Binding EditableText, Mode=TwoWay}\" />";
}
