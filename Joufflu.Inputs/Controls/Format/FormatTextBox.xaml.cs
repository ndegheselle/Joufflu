using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Joufflu.Inputs.Controls.Format
{
    /// <summary>
    /// A text box typed into group by group, as its <see cref="Parts"/> say. What it shows and
    /// how it answers the keyboard is the <see cref="FormatEditor"/>'s: the box forwards its input
    /// there and shows the outcome.
    /// </summary>
    [ContentProperty(nameof(Parts))]
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

        /// <summary>
        /// What the box shows, in order: <see cref="IntegerGroup"/>s and <see cref="DecimalGroup"/>s
        /// the user types into, and <see cref="FormatLiteral"/> text between them. The content of
        /// the box in XAML.
        /// </summary>
        public ObservableCollection<FormatPart> Parts { get; } = new ObservableCollection<FormatPart>();
        #endregion

        // Empty until the control is initialized, see OnPartsChanged.
        private FormatEditor _editor = new FormatEditor([]);

        /// <summary>
        /// Set while the box writes the editor's text and selection back to itself, which is no
        /// selection of the user's.
        /// </summary>
        private bool _isShowingEditor;

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
            Parts.CollectionChanged += (sender, e) => OnPartsChanged();
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
            switch (e.Key)
            {
                case Key.Escape:
                    Keyboard.ClearFocus();
                    e.Handled = true;
                    break;
                case Key.Tab:
                    int delta = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1;
                    _editor.MoveToGroup(delta);
                    ShowEditor();
                    e.Handled = true;
                    break;
                // Left and right walk the text, by group or by character depending on the group
                case Key.Left:
                case Key.Right:
                    e.Handled = _editor.MoveCaret(e.Key == Key.Left ? -1 : +1);
                    ShowEditor();
                    break;
                case Key.Up:
                case Key.Down:
                    if (SpinSelectedGroup(e.Key == Key.Up ? 1 : -1))
                        e.Handled = true;
                    break;
                // The text is fully driven by the groups, so the key is always handled to prevent
                // raw text editing.
                case Key.Delete:
                case Key.Back:
                    _editor.Delete(backwards: e.Key == Key.Back);
                    ShowUserEdit();
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            // Only spin when focused so we don't hijack scrolling of a parent container
            if (!IsKeyboardFocusWithin)
                return;
            if (SpinSelectedGroup(e.Delta > 0 ? 1 : -1))
                e.Handled = true;
        }

        /// <summary>
        /// Increment (direction &gt; 0) or decrement the selected group. With none selected, the
        /// key or the wheel is left to the box.
        /// </summary>
        /// <returns>true if a group was spun</returns>
        private bool SpinSelectedGroup(int direction)
        {
            if (_editor.SelectedGroup == null)
                return false;

            _editor.Spin(direction);
            ShowUserEdit();
            return true;
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
        /// A new editor for the new parts. Before the control is initialized XAML is still adding
        /// them one by one, so the editor waits for all of them, in OnInitialized.
        /// </summary>
        private void OnPartsChanged()
        {
            if (!IsInitialized)
                return;
            CreateEditor();
        }

        private void CreateEditor()
        {
            _editor = new FormatEditor(Parts);
            _editor.Load(Values);
            ShowEditor();
        }

        /// <summary>
        /// Show values set from outside. Values the editor already holds, those the box hands out
        /// itself among them, are already shown.
        /// </summary>
        private void LoadValuesIntoEditor()
        {
            List<object?> editorValues = _editor.GetValues();
            if (Values != null && Values.SequenceEqual(editorValues))
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

            // Not SetValue, which would replace a binding the host set on Values.
            SetCurrentValue(ValuesProperty, values);
        }
        #endregion
    }


    /// <summary>
    /// A format text box standing for a single <see cref="Value"/>, made of its
    /// <see cref="FormatTextBox.Values"/> and read back from them.
    /// </summary>
    public abstract class SingleValueFormatTextBox<T> : FormatTextBox
    {
        public event EventHandler<T?>? ValueChanged;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(T),
            typeof(SingleValueFormatTextBox<T>),
            new FrameworkPropertyMetadata(
                default(T),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((SingleValueFormatTextBox<T>)o).OnValueChanged()));

        public T? Value
        {
            get => (T?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// The values of the groups making [value].
        /// </summary>
        protected virtual List<object?> ToValues(T? value) => new List<object?>() { value };

        /// <summary>
        /// The value the groups' [values] make, the default while any of them is missing.
        /// </summary>
        protected virtual T? FromValues(IReadOnlyList<object?> values)
        {
            if (values.Any(x => x == null))
                return default;
            return (T?)values.FirstOrDefault();
        }

        private void OnValueChanged()
        {
            // Values already making this value are left as they are: either the value was read
            // from them, or rewriting them would wipe what the user typed in some groups and not
            // others yet, which makes no value at all.
            T? valueFromValues = ReadValueFromValues();
            if (!EqualityComparer<T?>.Default.Equals(valueFromValues, Value))
                SetCurrentValue(ValuesProperty, ToValues(Value));

            ValueChanged?.Invoke(this, Value);
        }

        protected override void OnValuesChanged()
        {
            base.OnValuesChanged();

            T? valueFromValues = ReadValueFromValues();
            // Not SetValue, which would replace a binding the host set on Value.
            SetCurrentValue(ValueProperty, valueFromValues);
        }

        private T? ReadValueFromValues() => Values == null ? default : FromValues(Values);
    }
}