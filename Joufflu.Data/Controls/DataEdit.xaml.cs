using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;
using NJsonSchema;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Controls;

/// Logique d'interaction pour DataEdit.xaml
/// </summary>
[ObservableObject]
public partial class DataEdit : UserControl
{
    [ObservableProperty]
    private DataObject _node = new DataObject("");

    public DataEdit()
    {
        this.DataContext = this;
        InitializeComponent();
    }
}
