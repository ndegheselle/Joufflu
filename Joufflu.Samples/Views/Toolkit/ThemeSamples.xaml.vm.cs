using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class ThemeSamplesViewModel : ObservableObject
{
    public string SwitcherCode =>
        "<!-- Bind any control to ThemeManager.Instance.Mode. Here RadioButtons -->\n" +
        "<!-- using EnumMatchToBooleanConverter to map each mode to a choice.   -->\n" +
        "<RadioButton Content=\"System\"\n" +
        "    IsChecked=\"{Binding Mode, Source={x:Static themes:ThemeManager.Instance},\n" +
        "        Converter={StaticResource EnumMatch},\n" +
        "        ConverterParameter={x:Static themes:ThemeMode.System}, Mode=TwoWay}\" />\n" +
        "<RadioButton Content=\"Light\" IsChecked=\"{Binding Mode, ... ThemeMode.Light ...}\" />\n" +
        "<RadioButton Content=\"Dark\"  IsChecked=\"{Binding Mode, ... ThemeMode.Dark ...}\" />";

    public string ManagerCode =>
        "// 1. At startup (App.OnStartup), before showing any window:\n" +
        "ThemeManager.Instance.Initialize();\n\n" +
        "// 2. Switch theme from anywhere:\n" +
        "ThemeManager.Instance.Mode = ThemeMode.Dark;   // Light / Dark / System\n\n" +
        "// The choice is persisted and restored on the next launch.\n" +
        "// In System mode the app follows the Windows theme live.\n" +
        "bool isDark = ThemeManager.Instance.IsDark;";

    public string SetupCode =>
        "<!-- App.xaml: merge the control styles; the active theme -->\n" +
        "<!-- dictionary is inserted at runtime by Initialize().    -->\n" +
        "<Application.Resources>\n" +
        "    <ResourceDictionary>\n" +
        "        <ResourceDictionary.MergedDictionaries>\n" +
        "            <ResourceDictionary Source=\"pack://application:,,,/Joufflu;component/Resources.xaml\" />\n" +
        "        </ResourceDictionary.MergedDictionaries>\n" +
        "    </ResourceDictionary>\n" +
        "</Application.Resources>";
}
