using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.Inputs.Controls.Format
{
    [TemplatePart(Name = "PART_ClearButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_UpButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_DownButton", Type = typeof(Button))]
    public class FormatTextBox : TextBox, INotifyPropertyChanged
    {
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
            if (!_isParsed)
                ParseGroups(Format, GlobalFormat);
            ValuesChanged?.Invoke(this, Values);
            FormatText();
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

        public int IncrementValue { get; set; } = 1;

        public static readonly DependencyProperty GlobalFormatProperty = DependencyProperty.Register(
            nameof(GlobalFormat),
            typeof(string),
            typeof(FormatTextBox),
            new FrameworkPropertyMetadata(null, (o, e) => ((FormatTextBox)o).InvalidateFormat()));

        public string? GlobalFormat
        {
            get => (string?)GetValue(GlobalFormatProperty);
            set => SetValue(GlobalFormatProperty, value);
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(FormatTextBox),
            new FrameworkPropertyMetadata("", (o, e) => ((FormatTextBox)o).InvalidateFormat()));

        public string Format
        {
            get => (string)GetValue(FormatProperty);
            set => SetValue(FormatProperty, value);
        }
        #endregion

        private int _selectedGroupIndex = -1;

        public int SelectedGroupIndex
        {
            get { return _selectedGroupIndex; }
            set
            {
                _selectedGroupIndex = value;
                SelectedGroup = (value >= 0 && value < _groups.Count) ? _groups[value] : null;
            }
        }

        public BaseGroup? SelectedGroup { get; set; }

        private readonly List<BaseGroup> _groups = new List<BaseGroup>();
        public IReadOnlyList<BaseGroup> Groups => _groups;

        // Ordered format parts: BaseGroup for a group, string for literal text between groups.
        private List<object> _parts = new List<object>();
        private bool _isSelectionChanging = false;
        private bool _isParsed = false;

        // Matches the content inside curly braces, ignoring escaped ones
        private static readonly Regex _formatRegex =
            new Regex(@"(?<!\\)\{(.*?)(?<!\\)\}|[^{}]+", RegexOptions.Compiled);

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
            this.Loaded += OnLoaded;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Parse only once: Loaded fires again whenever the control re-enters the
            // visual tree (tab switches, virtualization) and re-parsing would reset groups.
            if (!_isParsed)
                ParseGroups(Format, GlobalFormat);
            FormatText();
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

            // Get the new text based on the input and the current selection

            bool validInput = SelectedGroup?.OnInput(e.Text) ?? false;
            if (validInput)
            {
                UpdateCurrentValue();
                SelectedGroup?.OnAfterInput();
            }

            e.Handled = true;
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (_isSelectionChanging)
                return;

            base.OnSelectionChanged(e);

            int currentRegexGroupIndex = -1;
            for (int i = 0; i < Groups.Count; i++)
            {
                if (SelectionStart >= Groups[i].Index && SelectionStart <= Groups[i].Index + Groups[i].RenderedLength)
                {
                    currentRegexGroupIndex = i;
                    break;
                }
            }
            // Index of the group minus the first group (the global match)
            SelectedGroupIndex = currentRegexGroupIndex;

            if (SelectedGroupIndex < 0 && AllowSelectionOutsideGroups == false)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }

            _isSelectionChanging = true;
            SelectedGroup?.OnSelection();
            _isSelectionChanging = false;
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
                ChangeSelectedGroup(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1);
                e.Handled = true;
            }
            // If arrow keys change group
            else if (e.Key == Key.Left)
            {
                // Numeric group with global selection allow to go to the previous group
                IBaseNumericGroup? numericGroup = SelectedGroup as IBaseNumericGroup;
                if (numericGroup?.NoGlobalSelection == true)
                {
                    ChangeSelectedGroup(-1);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Right)
            {
                // Numeric group with global selection allow to go to the next group
                IBaseNumericGroup? numericGroup = SelectedGroup as IBaseNumericGroup;
                if (numericGroup?.NoGlobalSelection == true)
                {
                    ChangeSelectedGroup(+1);
                    e.Handled = true;
                }
            }
            // Up/Down arrows increment or decrement the selected numeric group
            else if (e.Key == Key.Up)
            {
                if (SelectedGroup is IBaseNumericGroup)
                {
                    Spin(1);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (SelectedGroup is IBaseNumericGroup)
                {
                    Spin(-1);
                    e.Handled = true;
                }
            }
            // Delete and Backspace clear the selected group. The text is fully driven
            // by the groups, so the key is always handled to prevent raw text editing.
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                if (SelectedGroup != null)
                {
                    SelectedGroup.OnDelete();
                    UpdateCurrentValue();
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            // Only spin when focused so we don't hijack scrolling of a parent container
            if (IsKeyboardFocusWithin && SelectedGroup is IBaseNumericGroup)
            {
                Spin(e.Delta > 0 ? 1 : -1);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Increment (direction &gt; 0) or decrement the selected numeric group.
        /// </summary>
        private void Spin(int direction)
        {
            if (SelectedGroup == null)
                ChangeSelectedGroup(1);

            if (SelectedGroup is IBaseNumericGroup numericGroup)
            {
                if (direction >= 0)
                    numericGroup.Increment();
                else
                    numericGroup.Decrement();

                UpdateCurrentValue();
                SelectedGroup?.OnAfterInput();
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e) => Spin(1);

        private void DownButton_Click(object sender, RoutedEventArgs e) => Spin(-1);

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var group in Groups)
                group.OnDelete();
            UpdateValues();
        }
        #endregion

        #region Methods
        public void ChangeSelectedGroup(int delta)
        {
            int newindex = SelectedGroupIndex + delta;
            if (newindex < 0 || newindex >= Groups.Count)
                return;

            if (IsFocused == false)
                Focus();

            Select(Groups[newindex].Index, 0);
        }

        private void FormatText()
        {
            // Change text and prevent selection from changing
            _isSelectionChanging = true;
            int selectionStart = SelectionStart;
            int selectionLength = SelectionLength;

            // Build the text from the ordered parts, recording each group's actual
            // start index and rendered length. Groups can render fewer characters than
            // their max Length (e.g. an unpadded "0"), so positions must come from the
            // real text, not from the max width, otherwise selection drifts.
            StringBuilder builder = new StringBuilder();
            foreach (object part in _parts)
            {
                if (part is BaseGroup group)
                {
                    string rendered = group.ToString() ?? "";
                    group.Index = builder.Length;
                    group.RenderedLength = rendered.Length;
                    builder.Append(rendered);
                }
                else if (part is string literal)
                {
                    builder.Append(literal);
                }
            }
            this.Text = builder.ToString();

            Select(selectionStart, selectionLength);
            _isSelectionChanging = false;
        }

        private void UpdateCurrentValue()
        {
            if (SelectedGroup == null)
                return;

            if (Values == null)
            {
                UpdateValues();
            }
            else
            {
                object? oldValue = Values[SelectedGroupIndex];
                // Values are boxed (int/decimal), so compare by value, not reference.
                if (!Equals(oldValue, SelectedGroup.Value))
                    UpdateValues();
            }
        }

        private void UpdateValues()
        {
            // Trigger DP change
            Values = Groups.Select(x => x.Value).ToList();
        }
        #endregion

        #region Parsing
        /// <summary>
        /// Reset the parsed state after <see cref="Format"/> or <see cref="GlobalFormat"/> change.
        /// Re-parses immediately when the control is loaded, otherwise defers to <see cref="OnLoaded"/>.
        /// </summary>
        private void InvalidateFormat()
        {
            SelectedGroupIndex = -1;
            _isParsed = false;
            if (IsLoaded)
            {
                ParseGroups(Format, GlobalFormat);
                FormatText();
            }
        }

        public void ParseGroups(string format, string? globalFormat)
        {
            _groups.Clear();
            _parts = ParseFormatString(format, globalFormat);

            foreach (object part in _parts)
            {
                if (part is BaseGroup group)
                    _groups.Add(group);
            }

            _isParsed = true;

            if (Values == null || Values.Count != Groups.Count)
                return;

            // Update group values from parts
            for (int i = 0; i < Groups.Count; i++)
            {
                _groups[i].Value = Values[i];
            }
        }

        /// <summary>
        /// Parse the format string
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="globalFormat">Global format string</param>
        /// <returns></returns>
        private List<object> ParseFormatString(string format, string? globalFormat)
        {
            GroupsFactory groupsFactory = new GroupsFactory();
            List<object> groups = new List<object>();
            int index = 0;

            MatchCollection matches = _formatRegex.Matches(format);

            foreach (Match match in matches)
            {
                if (match.Value.StartsWith("{") && match.Value.EndsWith("}"))
                {
                    // Extract the content inside the curly braces
                    string groupContent = match.Groups[1].Value;
                    BaseGroup group = groupsFactory.CreateGroupFromParams(this, groupContent, globalFormat);

                    group.Index = index;
                    groups.Add(group);
                    index += group.Length;
                }
                else
                {
                    // CreateValue the literal text to the groups
                    groups.Add(match.Value);
                    index += match.Value.Length;
                }
            }

            return groups;
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

        protected virtual void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            if (EqualityComparer<T>.Default.Equals(Value, _previousValue))
                return;

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
            Value = newValue;
        }
    }
}