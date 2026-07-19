using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// Text that can be edited, can be used outside a form to indicate clearly that a value can be edited
    /// </summary>
    [TemplatePart(Name = ElementTextBox, Type = typeof(FrameworkElement))]
    public partial class TextEditable : ContentControl, INotifyPropertyChanged
    {
        public struct TextEditedArgs
        {
            public string Text { get; set; }
            public string OldText { get; set; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<TextEditedArgs>? TextChanged;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextEditable),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            private set { _isEditing = value; NotifyPropertyChanged(); }
        }

        protected const string ElementTextBox = "PART_TextBox";
        protected TextBox? EditTextBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EditTextBox = Template.FindName(ElementTextBox, this) as TextBox;
        }

        [RelayCommand]
        private void Cancel()
        {
            IsEditing = false;
        }

        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [RelayCommand]
        private void Edit()
        {
            if (!IsEditing && EditTextBox != null)
            {
                EditTextBox.Text = Text;
                IsEditing = true;
            }
        }

        [RelayCommand]
        private void Validate()
        {
            if (IsEditing)
            {
                IsEditing = false;
                if (EditTextBox != null && Text != EditTextBox.Text)
                {
                    string oldText = Text;
                    Text = EditTextBox.Text;
                    TextChanged?.Invoke(this, new TextEditedArgs() { Text = Text, OldText = oldText });
                }
            }
        }
    }
}
