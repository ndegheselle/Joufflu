using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Samples.Views.Toolkit;

/// <summary>One star of a <see cref="RatingInput"/>.</summary>
public partial class RatingStar : ObservableObject
{
    public int Index { get; }

    [ObservableProperty]
    private bool _isFilled;

    public RatingStar(int index) => Index = index;
}

/// <summary>
/// A star rating, built from a bare <see cref="Control"/> so that it has to follow the Joufflu
/// input rules by itself: see <c>RatingInput.xaml</c> for its styles.
/// </summary>
public class RatingInput : Control
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(int),
        typeof(RatingInput),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((RatingInput)d).UpdateStars()));

    /// <summary>The number of stars given, 0 when not rated.</summary>
    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(int),
        typeof(RatingInput),
        new PropertyMetadata(5, (d, _) => ((RatingInput)d).BuildStars()));

    /// <summary>The number of stars shown.</summary>
    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public ObservableCollection<RatingStar> Stars { get; } = [];

    /// <summary>Rates with the star index passed as parameter.</summary>
    public ICommand RateCommand { get; }

    public ICommand ClearCommand { get; }

    public RatingInput()
    {
        RateCommand = new RelayCommand<int>(index => Value = index);
        ClearCommand = new RelayCommand(() => Value = 0);
        BuildStars();
    }

    private void BuildStars()
    {
        Stars.Clear();
        for (int index = 1; index <= Maximum; index++)
            Stars.Add(new RatingStar(index));
        UpdateStars();
    }

    private void UpdateStars()
    {
        foreach (RatingStar star in Stars)
            star.IsFilled = star.Index <= Value;
    }
}
