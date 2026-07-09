using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class EditorInputsSamplesViewModel : ObservableObject
{
    private string _editableText = "Double-click to edit me";
    private string? _filePath;

    public string EditableText { get => _editableText; set => SetProperty(ref _editableText, value); }

    public string? FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }

    public string TextEditableCode =>
        "<inputs:TextEditable Text=\"{Binding EditableText, Mode=TwoWay}\" />";

    public string FilePickerCode =>
        "<inputs:FilePicker FilePath=\"{Binding FilePath, Mode=TwoWay}\" />";

    public string DropdownCode =>
        "<inputs:Dropdown Header=\"Options\">\n" +
        "    <StackPanel>\n" +
        "        <CheckBox Content=\"Enable feature\" />\n" +
        "    </StackPanel>\n" +
        "</inputs:Dropdown>";

    public string ShortcutCode =>
        "<inputs:ShortcutSelector />";
}
