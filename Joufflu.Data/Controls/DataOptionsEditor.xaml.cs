using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Model;

namespace Joufflu.Data.Controls;

/// <summary>
/// Edits the options of a choice: lists them, removes one, adds the one typed.
/// <para>Its DataContext is the <see cref="DataValue"/> whose options are edited.</para>
/// </summary>
public partial class DataOptionsEditor : UserControl
{
    public DataOptionsEditor()
    {
        InitializeComponent();
    }

    /// <summary>The typed option stays in the box when refused, so it can be corrected.</summary>
    [RelayCommand]
    private void Add()
    {
        if (DataContext is not DataValue value)
            return;
        if (!value.AddOption(NewOptionBox.Text))
            return;

        NewOptionBox.Clear();
    }

    [RelayCommand]
    private void Remove(DataChoiceOption option)
    {
        if (DataContext is not DataValue value)
            return;

        value.RemoveOption(option);
    }
}
