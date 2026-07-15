namespace Joufflu.Inputs.Controls.Format
{
    public class GroupsFactory
    {
        Dictionary<string, Func<FormatTextBox, IEnumerable<string>, BaseGroup>> _types =
            new Dictionary<string, Func<FormatTextBox, IEnumerable<string>, BaseGroup>>()
        {
            { "numeric", (parent, options) => new NumericGroup(parent, options) },
            { "decimal", (parent, options) => new DecimalGroup(parent, options) },
        };

        /// <summary>
        /// CreateValue a group from the given params
        /// </summary>
        /// <param name="parent">Parent UI element</param>
        /// <param name="stringParams">String that describe the parameters (separated by |)</param>
        /// <param name="globalStringParams">Global string that describe the parameters (separated by |)</param>
        /// <returns>A base group based on the parameters</returns>
        /// <exception cref="ArgumentException">If the string parameters are empty</exception>
        public BaseGroup CreateGroupFromParams(FormatTextBox parent, string stringParams, string? globalStringParams)
        {
            IEnumerable<string> splitParams = stringParams.Split("|");
            if (splitParams.Count() <= 0)
                throw new ArgumentException("Options can not be empty.");

            // CreateValue global options at the beginning
            if (globalStringParams != null)
                splitParams = globalStringParams.Split("|").Concat(splitParams);

            // For each _types, check if options contains it
            // If yes, remove it from options and create the type
            // If no, create the default type
            foreach (var type in _types)
            {
                if (splitParams.Contains(type.Key))
                {
                    splitParams = splitParams.Where(x => x != type.Key);
                    return type.Value.Invoke(parent, splitParams);
                }
            }

            throw new ArgumentException("Unknow type key.");
        }
    }

    public abstract class BaseGroup
    {
        #region Options
        public int Length { get; set; } = 0;

        public string? StringFormat { get; set; } = null;

        public bool IsNullable { get; set; } = false;

        public char NullableChar { get; set; }
        #endregion

        public int Index { get; set; } = -1;

        // Number of characters this group actually renders (may be less than Length when
        // the value is unpadded). Set by the parent when it formats the text.
        public int RenderedLength { get; set; }

        public object? Value { get; set; }

        protected readonly FormatTextBox _parent;

        public BaseGroup(FormatTextBox parent, IEnumerable<string> stringParams)
        {
            _parent = parent;
            Dictionary<string, string?> options = ParseOptions(stringParams);
            // ApplyOptions removes each key it recognizes; anything left is a typo.
            ApplyOptions(options);
            if (options.Count > 0)
                throw new ArgumentException("Unknown option(s): " + string.Join(", ", options.Keys));
        }

        /// <summary>
        /// Split the "key:value" option strings into a lookup.
        /// Flag options (no value) are stored with a null value.
        /// </summary>
        protected static Dictionary<string, string?> ParseOptions(IEnumerable<string> stringParams)
        {
            Dictionary<string, string?> options = new Dictionary<string, string?>();
            foreach (var param in stringParams)
            {
                string[] splitParam = param.Split(":", 2);
                options[splitParam[0]] = splitParam.Length > 1 ? splitParam[1] : null;
            }
            return options;
        }

        /// <summary>
        /// Read <paramref name="key"/> from <paramref name="options"/> and remove it so
        /// unrecognized keys can be detected afterwards. Returns true when the key was present.
        /// </summary>
        protected static bool TryConsume(IDictionary<string, string?> options, string key, out string? value)
        {
            if (options.TryGetValue(key, out value))
            {
                options.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Apply the parsed options to this group, removing each recognized key.
        /// Override to read additional options, calling the base implementation first.
        /// </summary>
        protected virtual void ApplyOptions(IDictionary<string, string?> options)
        {
            if (TryConsume(options, "length", out var length) && length != null)
                Length = int.Parse(length);
            if (TryConsume(options, "format", out var format) && format != null)
                StringFormat = format;
            if (TryConsume(options, "nullable", out _))
                IsNullable = true;
            if (TryConsume(options, "nullableChar", out var nullableChar) && nullableChar != null)
                NullableChar = nullableChar[0];
        }

        /// <summary>
        /// What to do with the string input of the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="input"></param>
        /// <returns>Got a valid value</returns>
        public abstract bool OnInput(string input);

        public abstract void OnAfterInput();

        // What happen when the user click inside the group
        public abstract void OnSelection();

        public abstract void OnDelete();
    }

    public interface IBaseNumericGroup
    {
        public bool NoGlobalSelection { get; set; }

        public void Increment();

        public void Decrement();
    }

    public abstract class BaseNumericGroup<T> : BaseGroup, IBaseNumericGroup where T : struct
    {
        #region Options
        public bool NoGlobalSelection { get; set; } = false;

        public T? Min { get; set; }

        public T? Max { get; set; }

        public T? IncrementDelta { get; set; }

        public bool IsPadded { get; set; }
        #endregion

        public new T? Value
        {
            get { return (T?)base.Value; }
            set
            {
                // null is a valid value (cleared/nullable group) and must not be clamped.
                if (value == null)
                {
                    base.Value = null;
                    return;
                }
                if (Comparer<T?>.Default.Compare(value, Max) > 0)
                {
                    base.Value = Max;
                    return;
                }
                else if (Comparer<T?>.Default.Compare(value, Min) < 0)
                {
                    base.Value = Min;
                    return;
                }
                base.Value = value;
            }
        }

        protected override void ApplyOptions(IDictionary<string, string?> options)
        {
            base.ApplyOptions(options);

            if (TryConsume(options, "noGlobalSelection", out _))
                NoGlobalSelection = true;
            if (TryConsume(options, "padded", out _))
                IsPadded = true;
            if (TryConsume(options, "min", out var min) && min != null && TryParse(min, out T minValue))
                Min = minValue;
            if (TryConsume(options, "max", out var max) && max != null && TryParse(max, out T maxValue))
                Max = maxValue;
            if (TryConsume(options, "incrementDelta", out var delta) && delta != null && TryParse(delta, out T deltaValue))
                IncrementDelta = deltaValue;
        }

        public BaseNumericGroup(FormatTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            if (NullableChar == '\0')
                NullableChar = '-';
        }

        // Sensible field width when neither "length" nor an explicit "max" is given,
        // so an unqualified group does not size itself to the type's maximum (e.g. 10 digits).
        protected const int DefaultLength = 4;

        // Should be called after the constructor
        public void Init()
        {
            if (!IsNullable)
                Value = default(T);
            if (Length == 0)
                Length = DefaultLength;
        }

        public override bool OnInput(string input)
        {
            string newText;
            if (NoGlobalSelection)
            {
                // We replace the selected text by the input
                string oldText = _parent.Text;
                int carretOffset = 0;
                if (Index + Length < oldText.Length)
                {
                    oldText = oldText.Substring(Index, Length);
                    carretOffset = Index;
                }

                oldText = oldText.Remove(_parent.CaretIndex - carretOffset, _parent.SelectionLength);
                newText = oldText.Insert(_parent.CaretIndex - carretOffset, input);

                _parent.CaretIndex += 1;
                _parent.SelectionLength = 0;
            }
            else
            {
                newText = Value + input;
            }

            // If the number is too big we loop back to only the new number
            if (newText.Length > Length)
            {
                newText = input;
            }

            bool isValid = TryParse(newText, out T newValue);
            if (!isValid)
                return false;

            Value = newValue;
            return true;
        }

        public override void OnAfterInput()
        {
            if (Value == null)
                return;

            // Once the field is full, another digit can no longer fit, so move on
            // to the next group; otherwise keep the current group selected.
            if (IsFutureValueInvalid())
                _parent.ChangeSelectedGroup(1);
            else
                OnSelection();
        }

        public override void OnSelection()
        {
            if (NoGlobalSelection)
                return;

            // For numeric groups, we select the whole number (its rendered width, which
            // may be shorter than the max Length when the value is not padded).
            _parent.Select(Index, RenderedLength);
        }

        public override void OnDelete()
        {
            if (IsNullable)
                Value = null;
            else
                Value = default(T);
        }

        public override string? ToString()
        {
            if (Value == null)
                return new string(NullableChar, Length);

            string? format = Value.ToString();
            if (StringFormat != null)
                format = string.Format("{0" + StringFormat + "}", Value);
            if (IsPadded)
                format = format?.PadLeft(Length, '0');

            return format;
        }

        protected abstract bool TryParse(string newText, out T value);

        protected abstract bool IsFutureValueInvalid();

        public abstract void Increment();

        public abstract void Decrement();
    }

    public class NumericGroup : BaseNumericGroup<int>
    {
        public NumericGroup(FormatTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            if (IncrementDelta == null)
                IncrementDelta = 1;

            // Derive the field width from an explicit max before defaulting the bound.
            if (Length == 0 && Max != null)
                Length = Max.ToString()!.Length;

            if (Min == null)
                Min = int.MinValue;
            if (Max == null)
                Max = int.MaxValue;

            Init();
        }

        protected override bool TryParse(string newText, out int value) { return int.TryParse(newText, out value); }

        protected override bool IsFutureValueInvalid()
        {
            if (Value == null)
                return false;
            // Advance only when the field is full: the value already uses every
            // character, so a further digit could not be appended. We do NOT advance
            // early just because the next digit might exceed Max (an over-large value
            // is clamped by the Value setter instead).
            return Value.Value.ToString().Length >= Length;
        }

        public override void Increment()
        {
            if (Value is null)
                Value = Max;
            Value += IncrementDelta;
        }
        public override void Decrement()
        {
            if (Value is null)
                Value = Min;
            Value -= IncrementDelta;
        }
    }

    public class DecimalGroup : BaseNumericGroup<decimal>
    {
        public DecimalGroup(FormatTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            if (IncrementDelta == null)
                IncrementDelta = 0.1m;

            // Derive the field width from an explicit max before defaulting the bound.
            if (Length == 0 && Max != null)
                Length = Max.ToString()!.Length;

            if (Min == null)
                Min = decimal.MinValue;
            if (Max == null)
                Max = decimal.MaxValue;

            Init();
        }

        protected override bool TryParse(string newText, out decimal value)
        { return decimal.TryParse(newText, out value); }

        protected override bool IsFutureValueInvalid()
        {
            if (Value == null)
                return false;
            // Advance only when the field is full: the value already uses every
            // character, so a further digit could not be appended. We do NOT advance
            // early just because the next digit might exceed Max (an over-large value
            // is clamped by the Value setter instead).
            return Value.Value.ToString().Length >= Length;
        }

        public override void Increment()
        {
            if (Value is null)
                Value = Max;
            Value += IncrementDelta;
        }
        public override void Decrement()
        {
            if (Value is null)
                Value = Min;
            Value -= IncrementDelta;
        }
    }
}