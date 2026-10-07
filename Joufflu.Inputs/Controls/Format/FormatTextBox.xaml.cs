using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.Inputs.Controls.Format
{
    /// <summary>
    /// A text box typed into group by group, as its <see cref="Format"/> says. What it shows and
    /// how it answers the keyboard is the <see cref="FormatEditor"/>'s: the box forwards its input
    /// there and shows the outcome.
    /// </summary>
    [TemplatePart(Name = "PART_ClearButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_UpButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_DownButton", Type = typeof(Button))]
    public class FormatTextBox : TextBox, INotifyPropertyChanged
    {
        static FormatTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FormatTextBox), new FrameworkPropertyMetadata(typeof(FormatTextBox)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public event EventHandler<List<object?>>? ValuesChanged;

        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
            nameof(Values),
            typeof(List<object?>),
            typeof(FormatTextBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((FormatTextBox)o).OnValuesChanged()));

        #region Dependency Properties

        public List<object?> Values
        {
            get { return (List<object?>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        protected virtual void OnValuesChanged()
        {
            LoadValuesIntoEditor();
            ValuesChanged?.Invoke(this, Values);
        }

        #endregion

        #region Properties

        #region Options
        public bool AllowSelectionOutsideGroups { get; set; } = false;

        private bool _showDeleteButton = true;
        public bool ShowDeleteButton
        {
            get => _showDeleteButton;
            set { _showDeleteButton = value; OnPropertyChanged(); }
        }

        private bool _showIncrementsButtons = true;
        public bool ShowIncrementsButtons
        {
            get => _showIncrementsButtons;
            set { _showIncrementsButtons = value; OnPropertyChanged(); }
        }

        public static readonly DependencyProperty GlobalFormatProperty = DependencyProperty.Register(
            nameof(GlobalFormat),
            typeof(string),
            typeof(FormatTextBox),
            new FrameworkPropertyMetadata(null, (o, e) => ((FormatTextBox)o).OnFormatChanged()));

        public string? GlobalFormat
        {
            get => (string?)GetValue(GlobalFormatProperty);
            set => SetValue(GlobalFormatProperty, value);
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(FormatTextBox),
            new FrameworkPropertyMetadata("", (o, e) => ((FormatTextBox)o).OnFormatChanged()));

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }
        #endregion

        // Empty until the control is initialized, see OnFormatChanged.
        private FormatEditor _editor = new FormatEditor("", null);

        /// <summary>
        /// Set while the box writes the editor's text and selection back to itself, which is no
        /// selection of the user's.
        /// </summary>
        private bool _isShowingEditor;

        /// <summary>
        /// Set while the box hands the editor's values out, which the editor already holds.
        /// </summary>
        private bool _isPushingValues;

        // UI Parts
        private Button? _clearButton;

        private Button? ClearButton
        {
            get { return _clearButton; }
            set
            {
                _clearButton = value;

                if (_clearButton != null)
                    _clearButton.Click += ClearButton_Click;
            }
        }

        private Button? _upButton;

        private Button? UpButton
        {
            get { return _upButton; }
            set
            {
                _upButton = value;
                if (_upButton != null)
                    _upButton.Click += UpButton_Click;
            }
        }

        private Button? _downButton;

        private Button? DownButton
        {
            get { return _downButton; }
            set
            {
                _downButton = value;

                if (_downButton != null)
                    _downButton.Click += DownButton_Click;
            }
        }
        #endregion

        public FormatTextBox()
        {
            IsUndoEnabled = false;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            CreateEditor();
        }

        public override void OnApplyTemplate()
        {
            ClearButton = (Button)GetTemplateChild("PART_ClearButton");
            UpButton = (Button)GetTemplateChild("PART_UpButton");
            DownButton = (Button)GetTemplateChild("PART_DownButton");

            base.OnApplyTemplate();
        }

        #region UI Events
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            e.Handled = true;
            _editor.Type(e.Text);
            ShowUserEdit();
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (_isShowingEditor)
                return;

            base.OnSelectionChanged(e);

            _editor.Select(SelectionStart, SelectionLength);
            if (_editor.SelectedGroupIndex < 0 && AllowSelectionOutsideGroups == false)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }

            // The editor may have widened the selection to the whole group.
            ShowEditor();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // If escape unfocus the textbox
            if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }
            // If tab select next group
            else if (e.Key == Key.Tab)
            {
                _editor.MoveToGroup(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1);
                ShowEditor();
                e.Handled = true;
            }
            // Left and right walk the text, by group or by character depending on the group
            else if (e.Key == Key.Left)
            {
                e.Handled = _editor.MoveCaret(-1);
                ShowEditor();
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = _editor.MoveCaret(+1);
                ShowEditor();
            }
            // Up/Down arrows increment or decrement the selected group
            else if (e.Key == Key.Up)
            {
                if (_editor.SelectedGroup != null)
                {
                    _editor.Spin(1);
                    ShowUserEdit();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (_editor.SelectedGroup != null)
                {
                    _editor.Spin(-1);
                    ShowUserEdit();
                    e.Handled = true;
                }
            }
            // Delete and Backspace clear the selected group. The text is fully driven
            // by the groups, so the key is always handled to prevent raw text editing.
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                _editor.Delete(backwards: e.Key == Key.Back);
                ShowUserEdit();
                e.Handled = true;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            // Only spin when focused so we don't hijack scrolling of a parent container
            if (IsKeyboardFocusWithin && _editor.SelectedGroup != null)
            {
                _editor.Spin(e.Delta > 0 ? 1 : -1);
                ShowUserEdit();
                e.Handled = true;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e) => SpinFromButton(1);

        private void DownButton_Click(object sender, RoutedEventArgs e) => SpinFromButton(-1);

        // The box takes the focus, so that the group being spun shows as selected.
        private void SpinFromButton(int direction)
        {
            Focus();
            _editor.Spin(direction);
            ShowUserEdit();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _editor.Clear();
            ShowUserEdit();
        }
        #endregion

        #region Methods
        /// <summary>
        /// A new editor for the new format. Before the control is initialized XAML may still be
        /// setting the other format property, so the editor waits for both, in OnInitialized.
        /// </summary>
        private void OnFormatChanged()
        {
            if (!IsInitialized)
                return;
            CreateEditor();
        }

        private void CreateEditor()
        {
            _editor = new FormatEditor(Format, GlobalFormat);
            _editor.Load(Values);
            ShowEditor();
        }

        /// <summary>
        /// Show values set from outside. Those the box hands out itself are already shown.
        /// </summary>
        private void LoadValuesIntoEditor()
        {
            if (_isPushingValues)
                return;

            _editor.Load(Values);
            ShowEditor();
        }

        /// <summary>
        /// Show what the user's edit made of the editor, and hand its values out.
        /// </summary>
        private void ShowUserEdit()
        {
            ShowEditor();
            PushValues();
        }

        /// <summary>
        /// Write the editor's text and selection to the box: the only place they are written.
        /// </summary>
        private void ShowEditor()
        {
            _isShowingEditor = true;
            try
            {
                if (Text != _editor.Text)
                    Text = _editor.Text;
                Select(_editor.SelectionStart, _editor.SelectionLength);
            }
            finally
            {
                _isShowingEditor = false;
            }
        }

        private void PushValues()
        {
            List<object?> values = _editor.GetValues();
            // Values are boxed (long/decimal), so compare by value, not reference.
            if (Values != null && Values.SequenceEqual(values))
                return;

            _isPushingValues = true;
            try
            {
                Values = values;
            }
            finally
            {
                _isPushingValues = false;
            }
        }
        #endregion
    }


    public abstract class SingleValueFormatTextBox<T> : FormatTextBox
    {
        public event EventHandler<T?>? ValueChanged;

        private T? _previousValue = default;

        public virtual T? Value { get; set; } = default;

        public virtual List<object?> ConvertTo() { return new List<object?>() { Value }; }

        public virtual T? ConvertFrom()
        {
            if (Values.Any(x => x == null))
                return default;
            return (T?)Values.FirstOrDefault();
        }

        /// <summary>
        /// Prevent recursive updates.
        /// </summary>
        private bool _isValueFromGroups;

        protected virtual void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            if (EqualityComparer<T>.Default.Equals(Value, _previousValue))
                return;

            if (_isValueFromGroups == false)
                Values = ConvertTo();

            _previousValue = Value;
            ValueChanged?.Invoke(this, Value);
        }

        protected override void OnValuesChanged()
        {
            base.OnValuesChanged();

            var newValue = ConvertFrom();

            if (EqualityComparer<T>.Default.Equals(Value, newValue))
                return;

            _isValueFromGroups = true;
            try
            {
                Value = newValue;
            }
            finally
            {
                _isValueFromGroups = false;
            }
        }
    }
}