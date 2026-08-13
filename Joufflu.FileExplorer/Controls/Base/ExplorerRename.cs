using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;
using Joufflu.Helpers;

namespace Joufflu.FileExplorer.Controls.Base
{
    /// <summary>
    /// Turns a <see cref="TextBox"/> into the editable name of the node being renamed : it takes the focus as it is
    /// displayed, with the name selected the way the Windows explorer does, Enter and the loss of the focus validate
    /// what has been typed and Escape gives it up.
    /// </summary>
    /// <remarks>
    /// An attached behaviour rather than a control of its own : the box is a plain <see cref="TextBox"/>, so it keeps
    /// the style the application gives to them.
    /// </remarks>
    public static class ExplorerRename
    {
        /// <summary>
        /// Command ending the edition, <see cref="IExplorerSource.RenameCommand"/> in practice : run with the typed
        /// name, or with null when the edition has been given up. Setting it is what wires the behaviour.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ExplorerRename),
            new PropertyMetadata(null, OnCommandChanged));

        public static ICommand? GetCommand(DependencyObject element) => (ICommand?)element.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject element, ICommand? value)
            => element.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox box)
                return;

            box.Loaded -= OnLoaded;
            Detach(box);

            if (e.NewValue == null)
                return;

            box.Loaded += OnLoaded;
            box.PreviewKeyDown += OnPreviewKeyDown;
            box.LostKeyboardFocus += OnLostKeyboardFocus;
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;

            // Focused once the row displaying the box is rendered : it is not part of the tree the keyboard reaches
            // while the item container is still being built.
            box.Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new Action(
                    () =>
                    {
                        box.Focus();
                        SelectBaseName(box);
                    }));
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var box = (TextBox)sender;

            switch (e.Key)
            {
                case Key.Enter:
                    End(box, box.Text);
                    break;
                case Key.Escape:
                    End(box, null);
                    break;
                default:
                    return;
            }

            // Handled so that the list doesn't act on the key as well.
            e.Handled = true;
        }

        private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var box = (TextBox)sender;
            End(box, box.Text);
        }

        /// <summary>
        /// Ends the edition, <paramref name="name"/> being null when it has been given up.
        /// </summary>
        private static void End(TextBox box, string? name)
        {
            ICommand? command = GetCommand(box);

            // Detached first : the command makes the box disappear, which moves the focus and would end the edition a
            // second time.
            Detach(box);
            RestoreFocus(box);

            if (command?.CanExecute(name) == true)
                command.Execute(name);
        }

        private static void Detach(TextBox box)
        {
            box.PreviewKeyDown -= OnPreviewKeyDown;
            box.LostKeyboardFocus -= OnLostKeyboardFocus;
        }

        /// <summary>
        /// Name without its extension, as the Windows explorer selects it : typing replaces the name only, and the
        /// extension is kept unless it is edited on purpose.
        /// </summary>
        private static void SelectBaseName(TextBox box)
        {
            // A directory has no extension, "v1.2" being a whole name.
            int length = box.DataContext is IExplorerDirectory
                ? box.Text.Length
                : box.Text.Length - Path.GetExtension(box.Text).Length;

            box.Select(0, Math.Max(length, 0));
        }

        /// <summary>
        /// Gives the focus back to the row of the box, which disappears with the edition : without it the focus goes
        /// back to the window and the list cannot be walked with the keyboard anymore.
        /// </summary>
        private static void RestoreFocus(TextBox box)
        {
            if (!box.IsKeyboardFocusWithin)
                return;

            for (DependencyObject? element = MoreVisualTreeHelper.GetParent(box); element != null;
                element = MoreVisualTreeHelper.GetParent(element))
            {
                if (element is UIElement { Focusable: true } focusable && focusable.Focus())
                    return;
            }
        }
    }
}
