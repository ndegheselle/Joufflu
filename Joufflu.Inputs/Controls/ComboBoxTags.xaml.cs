using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// The text a tag shows: the item's value at the <c>DisplayMemberPath</c> given as second value,
    /// or its <c>ToString()</c> when there is none.
    /// </summary>
    public class DisplayMemberPathConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
                return string.Empty;

            if (values[1] == DependencyProperty.UnsetValue)
                return values[0].ToString();

            return ItemSearch.TextOf(values[0], (string)values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

    /// <summary>
    /// A <see cref="ComboBoxSearch"/> selecting several items, shown as removable tags and kept in
    /// <see cref="SelectedItems"/>. Already selected items leave the list, so none is picked twice.
    /// </summary>
    public partial class ComboBoxTags : ComboBoxSearch
    {
        static ComboBoxTags()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxTags), new FrameworkPropertyMetadata(typeof(ComboBoxTags)));
        }

        #region Dependency Properties
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(ComboBoxTags),
            new FrameworkPropertyMetadata(null, (o, e) => ((ComboBoxTags)o).OnSelectedItemsChanged()));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty AllowAddProperty = DependencyProperty.Register(
            nameof(AllowAdd),
            typeof(bool),
            typeof(ComboBoxTags),
            new FrameworkPropertyMetadata(false));

        /// <summary>Whether <c>Enter</c> adds the typed text as a tag when it matches no item.</summary>
        public bool AllowAdd
        {
            get => (bool)GetValue(AllowAddProperty);
            set => SetValue(AllowAddProperty, value);
        }
        #endregion

        #region Properties
        // The control always edits this working collection (the tag list is bound to it). Edits are
        // mirrored back into the bound SelectedItems, so two-way binding works for any mutable IList,
        // not only ObservableCollection.
        private readonly ObservableCollection<object> _internalSelectedItems = new();
        public IList InternalSelectedItems => _internalSelectedItems;

        // Guards the internal -> source mirroring while we are loading source -> internal.
        private bool _syncingSelection;

        // Up and Down move the selection through the list, and an editable ComboBox offers no way
        // to keep that from committing it. Only a click or Enter should add a tag, so the selection
        // change an arrow key causes is skipped.
        private bool _ignoreNextSelection = false;
        private Popup? _popup;
        #endregion

        public ComboBoxTags()
        {
            _internalSelectedItems.CollectionChanged += OnInternalSelectedItemsChanged;

            // The tags wrap and grow the control, but an open popup does not follow its target's
            // size on its own: nudging the offset makes it place itself again.
            SizeChanged += (s, e) =>
            {
                if (_popup == null)
                    return;

                var offset = _popup.HorizontalOffset;
                _popup.HorizontalOffset = offset + 1;
                _popup.HorizontalOffset = offset;
            };
        }

        public override void OnApplyTemplate()
        {
            _popup = (Popup)GetTemplateChild("PART_Popup");
            base.OnApplyTemplate();
        }

        // Load the bound selection into the working collection. Fired when the SelectedItems
        // reference changes (e.g. a view model assigns its collection).
        void OnSelectedItemsChanged()
        {
            _syncingSelection = true;
            try
            {
                _internalSelectedItems.Clear();
                if (SelectedItems != null)
                {
                    foreach (var item in SelectedItems)
                        _internalSelectedItems.Add(item);
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        // Mirror edits made through the control back into the bound collection, whatever concrete
        // (mutable) IList it is, so the binding source stays in sync.
        private void OnInternalSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_syncingSelection || SelectedItems == null || SelectedItems.IsReadOnly || SelectedItems.IsFixedSize)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems!)
                        SelectedItems.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems!)
                        SelectedItems.Remove(item);
                    break;
                default:
                    // Replace / Move / Reset: rebuild the source from the working collection.
                    SelectedItems.Clear();
                    foreach (var item in _internalSelectedItems)
                        SelectedItems.Add(item);
                    break;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (_ignoreNextSelection)
            {
                _ignoreNextSelection = false;
                return;
            }

            AddSelectedItem();
        }

        #region UI events
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            // ComboBox cycles the selection on mouse wheel when the dropdown is closed,
            // which would auto-add a tag. Block it, but still allow scrolling the open list.
            if (IsDropDownOpen == false)
                e.Handled = true;

            base.OnPreviewMouseWheel(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
                _ignoreNextSelection = true;

            if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && InternalSelectedItems.Count > 0)
                InternalSelectedItems.RemoveAt(InternalSelectedItems.Count - 1);

            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Backspace is handled in OnPreviewKeyDown alone: handling it here too removed two tags per key.
            if (e.Key == Key.Enter)
            {
                if (SelectedItem != null)
                {
                    AddSelectedItem();
                }
                else if (AllowAdd && !string.IsNullOrEmpty(Text))
                {
                    InternalSelectedItems.Add(Text);
                    Text = string.Empty;
                }
            }

            base.OnKeyDown(e);
        }

        #endregion

        #region Methods
        protected override bool DoesItemPassFilter(object value)
        {
            // A selected item is a tag already: listing it again would let it be picked twice.
            if (InternalSelectedItems.Contains(value) == true)
                return false;

            return base.DoesItemPassFilter(value);
        }

        private void AddSelectedItem()
        {
            if (SelectedItem == null)
                return;
            InternalSelectedItems.Add(SelectedItem);
            Text = string.Empty;
        }

        [RelayCommand]
        private void RemoveSelected(object? parameter) => InternalSelectedItems.Remove(parameter);
        #endregion
    }
}
