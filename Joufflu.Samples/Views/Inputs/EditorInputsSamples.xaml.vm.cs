using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class EditorInputsSamplesViewModel : ObservableObject
{
    private string _editableText = "Double-click to edit me";
    private string? _filePath;
    private Color _color = Color.FromRgb(0x4F, 0x46, 0xE5);

    public string EditableText { get => _editableText; set => SetProperty(ref _editableText, value); }

    public string? FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }

    public Color Color { get => _color; set => SetProperty(ref _color, value); }

    public string TextEditableCode =>
        "<inputs:TextEditable Text=\"{Binding EditableText, Mode=TwoWay}\" />";

    public string FilePickerCode =>
        "<inputs:FilePicker FilePath=\"{Binding FilePath, Mode=TwoWay}\" />";

    public string ColorPickerCode =>
        "<inputs:ColorPicker Color=\"{Binding Color, Mode=TwoWay}\" />";

    public string DropdownCode =>
        "<inputs:Dropdown Header=\"Bottom-right\" PopupPlacement=\"BottomRight\">\n" +
        "    <StackPanel>\n" +
        "        <CheckBox Content=\"Enable feature\" />\n" +
        "    </StackPanel>\n" +
        "</inputs:Dropdown>";
}
