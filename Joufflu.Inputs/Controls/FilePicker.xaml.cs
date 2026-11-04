using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Inputs.Controls
{
    /// <summary>What the system dialog of a <see cref="FilePicker"/> offers.</summary>
    public class FilePickerOptions
    {
        /// <summary>The dialog's file types, as <c>Microsoft.Win32.OpenFileDialog.Filter</c> reads them: <c>"JSON files|*.json|All files|*.*"</c>.</summary>
        public string Filter { get; set; } = "";
        public string DefaultExtension { get; set; } = "*.*";
    }

    /// <summary>
    /// A file path, typed or chosen through the system open dialog, with a button to clear it.
    /// Two-way bindable through <see cref="FilePath"/>.
    /// </summary>
    public partial class FilePicker : Control
    {
        static FilePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilePicker), new FrameworkPropertyMetadata(typeof(FilePicker)));
        }

        #region Dependency Properties
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(FilePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? FilePath
        {
            get { return (string?)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options),
            typeof(FilePickerOptions),
            typeof(FilePicker),
            new PropertyMetadata(null));

        public FilePickerOptions? Options
        {
            get { return (FilePickerOptions?)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
        #endregion

        [RelayCommand]
        private void Clear() { FilePath = ""; }

        [RelayCommand]
        private void Select()
        {
            Options ??= new FilePickerOptions();
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = Options.DefaultExtension;
            dialog.Filter = Options.Filter;
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }
    }
}
