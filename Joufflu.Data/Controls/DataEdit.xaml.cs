// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
namespace Joufflu.Data.Controls;

public partial class DataEdit : DataControlBase
{
    public DataEdit()
    {
        this.DataContext = this;
        InitializeComponent();
    }
}
