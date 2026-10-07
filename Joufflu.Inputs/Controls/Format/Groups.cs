using System.Globalization;
using System.Numerics;

namespace Joufflu.Inputs.Controls.Format
{
    internal static class GroupsFactory
    {
        /// <summary>
        /// Create a group from its parameters, "numeric|max:59|padded" for one.
        /// </summary>
        /// <param name="parent">Parent UI element</param>
        /// <param name="stringParams">The group's own parameters, separated by |</param>
        /// <param name="globalStringParams">Parameters shared by every group, separated by |</param>
        /// <exception cref="ArgumentException">If no type is given, or an option is unknown</exception>
        public static BaseGroup Create(FormatTextBox parent, string stringParams, string? globalStringParams)
        {
            // Global parameters go first so that the group's own override them.
            IEnumerable<string> splitParams = stringParams.Split("|");
            if (globalStringParams != null)
                splitParams = globalStringParams.Split("|").Concat(splitParams);

            if (splitParams.Contains("numeric"))
            {
                GroupOptions options = GroupOptions.Parse(splitParams.Where(x => x != "numeric"));
                return new NumberGroup<long>(parent, options, defaultIncrementDelta: 1);
            }
            if (splitParams.Contains("decimal"))
            {
                GroupOptions options = GroupOptions.Parse(splitParams.Where(x => x != "decimal"));
                return new NumberGroup<decimal>(parent, options, defaultIncrementDelta: 0.1m);
            }

            throw new ArgumentException("Unknown type key.");
        }
    }

    /// <summary>
    /// The options a group is written with: "key:value" pairs, and flags with no value. The bounds
    /// stay text, for the group to read in the type it counts in.
    /// </summary>
    internal record GroupOptions
    {
        private static readonly HashSet<string> _knownKeys = new HashSet<string>()
        {
            "length", "format", "nullable", "nullableChar", "noGlobalSelection", "padded", "min", "max", "incrementDelta",
        };

        /// <summary>
        /// How many characters the group holds, 0 when nothing says.
        /// </summary>
        public int Length { get; init; }

        public string? StringFormat { get; init; }

        public bool IsNullable { get; init; }

        public char NullableChar { get; init; } = '-';

        public bool NoGlobalSelection { get; init; }

        public bool IsPadded { get; init; }

        public string? Min { get; init; }

        public string? Max { get; init; }

        public string? IncrementDelta { get; init; }

        /// <exception cref="ArgumentException">If an option is unknown, most likely a typo</exception>
        public static GroupOptions Parse(IEnumerable<string> stringParams)
        {
            // A later option overrides an earlier one with the same key.
            Dictionary<string, string?> options = new Dictionary<string, string?>();
            foreach (string param in stringParams)
            {
                string[] keyValue = param.Split(":", 2);
                options[keyValue[0]] = keyValue.Length > 1 ? keyValue[1] : null;
            }

            string[] unknownKeys = options.Keys.Where(key => !_knownKeys.Contains(key)).ToArray();
            if (unknownKeys.Length > 0)
                throw new ArgumentException("Unknown option(s): " + string.Join(", ", unknownKeys));

            return new GroupOptions()
            {
                Length = options.GetValueOrDefault("length") is string length ? int.Parse(length) : 0,
                StringFormat = options.GetValueOrDefault("format"),
                IsNullable = options.ContainsKey("nullable"),
                NullableChar = options.GetValueOrDefault("nullableChar") is string nullableChar ? nullableChar[0] : '-',
                NoGlobalSelection = options.ContainsKey("noGlobalSelection"),
                IsPadded = options.ContainsKey("padded"),
                Min = options.GetValueOrDefault("min"),
                Max = options.GetValueOrDefault("max"),
                IncrementDelta = options.GetValueOrDefault("incrementDelta"),
            };
        }
    }

    public abstract class BaseGroup
    {
        /// <summary>
        /// How many characters the group holds, 0 when nothing says: a group given neither a
        /// length nor a max is bounded by its type alone, and takes as much as that type does.
        /// </summary>
        public int Length { get; protected set; } = 0;

        public int Index { get; set; } = -1;

        // Number of characters this group actually renders (may be less than Length when
        // the value is unpadded). Set by the parent when it formats the text.
        public int RenderedLength { get; set; }

        /// <summary>
        /// What the group holds, boxed in the type it counts in, null when it holds nothing.
        /// </summary>
        public abstract object? Value { get; }

        protected readonly FormatTextBox _parent;

        protected BaseGroup(FormatTextBox parent)
        {
            _parent = parent;
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
        /// group counts in.
        /// </summary>
        public abstract void Load(object? value);
    }

    public interface IBaseNumericGroup
    {
        public bool NoGlobalSelection { get; set; }

        public void Increment();

        public void Decrement();
    }

    /// <summary>
    /// A number, counted in long for a "numeric" group and in decimal for a "decimal" one.
    /// </summary>
    internal class NumberGroup<T> : BaseGroup, IBaseNumericGroup where T : struct, INumber<T>, IMinMaxValue<T>
    {
        #region Options
        public string? StringFormat { get; }

        public bool IsNullable { get; }

        public char NullableChar { get; }

        public bool NoGlobalSelection { get; set; }

        public bool IsPadded { get; }

        public T Min { get; }

        public T Max { get; }

        public T IncrementDelta { get; }
        #endregion

        /// <summary>
        /// Written through <see cref="SetNumber"/> when the user edits the group, and through
        /// <see cref="Load"/> when the value comes from outside.
        /// </summary>
        private T? _number;

        public override object? Value => _number;

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

        public NumberGroup(FormatTextBox parent, GroupOptions options, T defaultIncrementDelta) : base(parent)
        {
            StringFormat = options.StringFormat;
            IsNullable = options.IsNullable;
            NullableChar = options.NullableChar;
            NoGlobalSelection = options.NoGlobalSelection;
            IsPadded = options.IsPadded;

            T? max = ParseOption(options.Max);
            Min = ParseOption(options.Min) ?? T.MinValue;
            Max = max ?? T.MaxValue;
            IncrementDelta = ParseOption(options.IncrementDelta) ?? defaultIncrementDelta;

            // A group given no length is as wide as its max, when it is given one.
            Length = options.Length;
            if (Length == 0 && max is T explicitMax)
                Length = explicitMax.ToString()!.Length;

            // Last, so that the bounds it is clamped between are known.
            if (!IsNullable)
                SetNumber(T.Zero);
        }

        /// <summary>
        /// Hold [number], brought back between the bounds. Whatever was being typed is answered
        /// for by it.
        /// </summary>
        private void SetNumber(T? number)
        {
            _typedText = null;

            // null is a valid value (cleared/nullable group) and must not be clamped.
            if (number is not T given)
            {
                _number = null;
                return;
            }
            if (given > Max)
            {
                _number = Max;
                return;
            }
            if (given < Min)
            {
                _number = Min;
                return;
            }
            _number = given;
        }

        private static T? ParseOption(string? text)
        {
            if (text == null || !TryParse(text, out T value))
                return null;
            return value;
        }

        private static bool TryParse(string text, out T value)
            => T.TryParse(text, CultureInfo.CurrentCulture, out value);

        /// <summary>
        /// A number read back out of a box only comes out as the very type it went in as, and a
        /// host filling in the values has no reason to know which type the group counts in.
        /// Anything countable is taken and counted as the group's own.
        /// <para>
        /// Not clamped, unlike what the user edits: what a host hands over is taken as it stands.
        /// </para>
        /// </summary>
        public override void Load(object? value)
        {
            // What is being typed stands until the user finishes it.
            if (_typedText != null)
                return;

            if (value is null)
            {
                _number = null;
                return;
            }
            _number = (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public override bool OnInput(string input)
        {
            input = NormalizeInput(input);

            string newText;
            if (NoGlobalSelection && _number == null && _typedText == null)
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
                newText = (_typedText ?? _number?.ToString()) + input;
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
                SetNumber(newValue);

                // What was written is kept only where a number cannot give it back: a separator
                // with no fraction after it yet. A leading zero, say, is the group's own to render
                // as it always has. Never when the group clamped what was written either: what is
                // shown is then what is held.
                if (EqualityComparer<T?>.Default.Equals(_number, newValue)
                    && newText.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                    _typedText = newText;
                return true;
            }

            // Not a number yet, but on its way to one, which a digit would finish: a minus waiting
            // for its digits, a separator waiting for its fraction.
            if (TryParse(newText + "0", out _))
            {
                SetNumber(null);
                _typedText = newText;
                return true;
            }

            return false;
        }

        /// <summary>
        /// A fraction is written with whatever character the culture separates it by, and typed
        /// with whichever of the two keys the keyboard offers — a numeric keypad's point included.
        /// Harmless for a whole number: it refuses either separator all the same.
        /// </summary>
        private static string NormalizeInput(string input)
            => input is "." or ","
                ? CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator
                : input;

        public override void OnAfterInput()
        {
            // Nothing typed, nothing to answer for. What is half typed counts as something: the
            // caret has to follow it even though no number holds it yet.
            if (_number == null && _typedText == null)
                return;

            // Once the field is full, another digit can no longer fit, so move on to the next
            // group. When there is none to move to - a lone group, or the last one — a group
            // keeping its own caret keeps it at the end of what was typed rather than letting it
            // drift back into the middle of the number.
            if (IsFull() && (_parent.ChangeSelectedGroup(1) || NoGlobalSelection == false))
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

        /// <summary>
        /// Full once the value uses every character of the group, so a further digit could not be
        /// appended. Not before, just because the next digit might go past the max: an over-large
        /// value is clamped by <see cref="SetNumber"/> instead. A group with no length of its own
        /// is never full.
        /// </summary>
        private bool IsFull()
        {
            if (_number is not T number || Length == 0)
                return false;
            return number.ToString()!.Length >= Length;
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
                SetNumber(null);
            else
                SetNumber(T.Zero);
        }

        public override string? ToString()
        {
            // What is being typed stands for itself until a number can give it back.
            if (_typedText != null)
                return _typedText;

            // A group with a width of its own shows what it is waiting for; one without has no
            // slots to show, so an empty number is an empty field, the way a number field reads.
            if (_number is not T number)
                return new string(NullableChar, Length);

            string? format = number.ToString();
            if (StringFormat != null)
                format = string.Format("{0" + StringFormat + "}", number);
            if (IsPadded)
                format = format?.PadLeft(Length, '0');

            return format;
        }

        public void Increment()
        {
            // An empty group starts at zero rather than one step past it.
            if (_number is not T number)
            {
                SetNumber(T.Zero);
                return;
            }
            SetNumber(number + IncrementDelta);
        }

        public void Decrement()
        {
            // An empty group starts at zero rather than one step before it.
            if (_number is not T number)
            {
                SetNumber(T.Zero);
                return;
            }
            SetNumber(number - IncrementDelta);
        }
    }
}
