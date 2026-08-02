using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.FileExplorer.Controls
{
    public class ExplorerList : ListView
    {
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.ChangedButton != MouseButton.Left) return;

            if (ContainerFromElement(this, (DependencyObject)e.OriginalSource)
                is not ListViewItem lvi) return;

            if (lvi.DataContext is not ExplorerFolder folder)
                return;

            folder.Open();
        }
    }
}
