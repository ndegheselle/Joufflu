using System.Globalization;

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
        /// <summary>
        /// How many characters the group holds, 0 when nothing says: a group given neither a
        /// length nor a max is bounded by its type alone, and takes as much as that type does.
        /// </summary>
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
        /// <param name="input"></param>
        /// <returns>Got a valid value</returns>
        public abstract bool OnInput(string input);

        public abstract void OnAfterInput();

        // What happen when the user click inside the group
        public abstract void OnSelection();

        public abstract void OnDelete();

        /// <summary>
        /// Take out the one character the caret is at, [backwards] for the one before it rather
        /// than the one after. Tells whether the group took the key: a group with no caret of its
        /// own has no character in particular to take out, and is emptied instead.
        /// </summary>
        public virtual bool OnDeleteCharacter(bool backwards) => false;

        /// <summary>
        /// Take [value] as this group's own, coming from outside where nothing says which type a
        /// group counts in. Overridden by a group that counts in one of its own.
        /// </summary>
        public virtual void SetValueFrom(object? value) => Value = value;
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
                // Whatever was being typed is answered for by the value now being set.
                _typedText = null;

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

        /// <summary>
        /// Where the caret belongs once the text has been built again: right after what was just
        /// typed. Only a group that keeps the caret rather than selecting itself whole reads it.
        /// </summary>
        private int _caretAfterInput;

        /// <summary>
        /// What has been typed while a number cannot give it back as it stands: "3," on its way to
        /// "3,5", or "-" on its way to "-4". Null when the text is the value read out and nothing
        /// more, which is all it is once the number is whole.
        /// </summary>
        private string? _typedText;

        /// <summary>
        /// A number read back out of a box only comes out as the very type it went in as, and a
        /// host filling in the values has no reason to know which type the group counts in.
        /// Anything countable is taken and counted as the group's own.
        /// <para>
        /// Set past the clamping the group does of its own values: what a host hands over is taken
        /// as it stands, the way it always has been when the text is first parsed.
        /// </para>
        /// </summary>
        public override void SetValueFrom(object? value)
            // Past the setter, so nothing here clears what is being typed on its behalf.
            => base.Value = _typedText is not null ? base.Value : value is null
                ? null
                : Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);

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

        // Should be called after the constructor
        public void Init()
        {
            if (!IsNullable)
                Value = default(T);
        }

        public override bool OnInput(string input)
        {
            input = NormalizeInput(input);

            string newText;
            if (NoGlobalSelection && Value == null && _typedText == null)
            {
                // What a group holding nothing shows stands for nothing: it is not text to type
                // into, so what is typed starts the number afresh rather than landing among the
                // characters that say the group is empty.
                newText = input;
                _caretAfterInput = Index + input.Length;
            }
            else if (NoGlobalSelection)
            {
                // We replace the selected text by the input
                string oldText = _parent.Text;
                int carretOffset = 0;
                // An unbounded group has no slice of its own to cut out: it is the whole text.
                if (Length > 0 && Index + Length < oldText.Length)
                {
                    oldText = oldText.Substring(Index, Length);
                    carretOffset = Index;
                }

                oldText = oldText.Remove(_parent.CaretIndex - carretOffset, _parent.SelectionLength);
                newText = oldText.Insert(_parent.CaretIndex - carretOffset, input);

                // The caret cannot be moved from here: the box still holds the text as it was,
                // which is shorter than what is being typed into it, so the position would be
                // clamped back to the end of the old text and the next character would land in
                // the middle of the number. It waits for the text to be built again.
                _caretAfterInput = _parent.CaretIndex + input.Length;
            }
            else
            {
                newText = (_typedText ?? Value?.ToString()) + input;
            }

            // If the number is too big we loop back to only the new number. A group that holds
            // as much as its type does never fills up: what does not fit fails to parse instead.
            if (Length > 0 && newText.Length > Length)
            {
                newText = input;
                // Nothing of what was there is left, so the caret follows the one character that is.
                _caretAfterInput = Index + input.Length;
            }

            return ApplyText(newText);
        }

        public override bool OnDeleteCharacter(bool backwards)
        {
            // A group selected whole has no character in particular to take out.
            if (NoGlobalSelection == false)
                return false;

            string oldText = _parent.Text;
            int carretOffset = 0;
            if (Length > 0 && Index + Length < oldText.Length)
            {
                oldText = oldText.Substring(Index, Length);
                carretOffset = Index;
            }

            int caret = Math.Clamp(_parent.CaretIndex - carretOffset, 0, oldText.Length);
            int length = Math.Min(_parent.SelectionLength, oldText.Length - caret);

            if (length == 0)
            {
                // Nothing is selected, so the key points at the one character beside the caret,
                // where there is one to point at.
                if (backwards)
                {
                    if (caret <= 0)
                        return true;
                    caret -= 1;
                }
                else if (caret >= oldText.Length)
                {
                    return true;
                }

                length = 1;
            }

            _caretAfterInput = carretOffset + caret;
            return ApplyText(oldText.Remove(caret, length));
        }

        /// <summary>
        /// Take [newText] as what the group now reads: the number it amounts to once it amounts to
        /// one, and what was written while it does not yet. Tells whether the text was taken.
        /// </summary>
        private bool ApplyText(string newText)
        {
            // Nothing left is nothing held, which is what emptying the group means.
            if (newText.Length == 0)
            {
                OnDelete();
                return true;
            }

            if (TryParse(newText, out T newValue))
            {
                Value = newValue;

                // What was written is kept only where a number cannot give it back: a separator
                // with no fraction after it yet. A leading zero, say, is the group's own to render
                // as it always has. Never when the group clamped what was written either: what is
                // shown is then what is held.
                if (EqualityComparer<T?>.Default.Equals(Value, newValue)
                    && newText.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                    _typedText = newText;
                return true;
            }

            // Not a number yet, but on its way to one, which a digit would finish: a minus waiting
            // for its digits, a separator waiting for its fraction.
            if (TryParse(newText + "0", out _))
            {
                Value = null;
                _typedText = newText;
                return true;
            }

            return false;
        }

        /// <summary>
        /// What is typed, as this group reads it. Taken as it comes unless a group says otherwise.
        /// </summary>
        protected virtual string NormalizeInput(string input) => input;

        public override void OnAfterInput()
        {
            // Nothing typed, nothing to answer for. What is half typed counts as something: the
            // caret has to follow it even though no number holds it yet.
            if (Value == null && _typedText == null)
                return;

            // Once the field is full, another digit can no longer fit, so move on to the next
            // group. When there is none to move to - a lone group, or the last one — a group
            // keeping its own caret keeps it at the end of what was typed rather than letting it
            // drift back into the middle of the number.
            if (IsFutureValueInvalid() && (_parent.ChangeSelectedGroup(1) || NoGlobalSelection == false))
                return;

            if (NoGlobalSelection)
            {
                // Now that the text is the one that was typed, the caret can go where the typing
                // left it. It is only ever set here, so that clicking about the box stays the
                // user's own business.
                _parent.Select(Math.Min(_caretAfterInput, _parent.Text.Length), 0);
                return;
            }

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
            // What is being typed stands for itself until a number can give it back.
            if (_typedText != null)
                return _typedText;

            // A group with a width of its own shows what it is waiting for; one without has no
            // slots to show, so an empty number is an empty field, the way a number field reads.
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

        protected bool TryStartFromEmpty()
        {
            if (Value is not null)
                return false;

            Value = default(T);
            return true;
        }

        public abstract void Increment();

        public abstract void Decrement();
    }

    public class NumericGroup : BaseNumericGroup<long>
    {
        public NumericGroup(FormatTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            if (IncrementDelta == null)
                IncrementDelta = 1;

            // Derive the field width from an explicit max before defaulting the bound.
            if (Length == 0 && Max != null)
                Length = Max.ToString()!.Length;

            if (Min == null)
                Min = long.MinValue;
            if (Max == null)
                Max = long.MaxValue;

            Init();
        }

        protected override bool TryParse(string newText, out long value) { return long.TryParse(newText, out value); }

        protected override bool IsFutureValueInvalid()
        {
            if (Value == null || Length == 0)
                return false;
            // Advance only when the field is full: the value already uses every
            // character, so a further digit could not be appended. We do NOT advance
            // early just because the next digit might exceed Max (an over-large value
            // is clamped by the Value setter instead). A group with no length of its own
            // is never full, so it never hands over.
            return Value.Value.ToString().Length >= Length;
        }

        public override void Increment()
        {
            if (TryStartFromEmpty())
                return;
            Value += IncrementDelta;
        }
        public override void Decrement()
        {
            if (TryStartFromEmpty())
                return;
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

        // A fraction is written with whatever character the culture separates it by, and typed
        // with whichever of the two keys the keyboard offers — a numeric keypad's point included.
        protected override string NormalizeInput(string input)
            => input is "." or ","
                ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
                : input;

        protected override bool IsFutureValueInvalid()
        {
            if (Value == null || Length == 0)
                return false;
            // Advance only when the field is full: the value already uses every
            // character, so a further digit could not be appended. We do NOT advance
            // early just because the next digit might exceed Max (an over-large value
            // is clamped by the Value setter instead). A group with no length of its own
            // is never full, so it never hands over.
            return Value.Value.ToString().Length >= Length;
        }

        public override void Increment()
        {
            if (TryStartFromEmpty())
                return;
            Value += IncrementDelta;
        }
        public override void Decrement()
        {
            if (TryStartFromEmpty())
                return;
            Value -= IncrementDelta;
        }
    }
}