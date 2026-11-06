using System.Windows;
using System.Windows.Threading;
using Joufflu.Data.Controls;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Tests.Controls;

/// <summary>
/// Fixtures run in a single-threaded apartment, the only one WPF elements can be created in.
/// </summary>
[Apartment(ApartmentState.STA)]
public class DataFillBindingTests
{
    private BindingErrors _errors = null!;
    private Window _window = null!;
    private DataFill _fill = null!;

    private static DataObject Parse(string json) => (DataObject)JToken.Parse(json).ToDataNode();

    /// <summary>
    /// The control's static resources are only found in the application's, as in an app merging
    /// Joufflu in its App.xaml. There is one application per process: the first fixture creates it.
    /// </summary>
    [OneTimeSetUp]
    public void StartApplication()
    {
        if (Application.Current != null)
            return;
        var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var joufflu = new ResourceDictionary { Source = new Uri("pack://application:,,,/Joufflu;component/Resources.xaml") };
        application.Resources.MergedDictionaries.Add(joufflu);
    }

    [SetUp]
    public void Show()
    {
        _errors = new BindingErrors();
        _fill = new DataFill
        {
            Node = Parse("""{ "a": "x", "b": 1, "c": true }"""),
            ManualValues = [new DataManualValue(EnumDataType.String, "TBD")]
        };
        _window = new Window
        {
            Width = 400,
            Height = 300,
            Left = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = _fill
        };
        _window.Show();
        Settle();
    }

    [TearDown]
    public void Close()
    {
        _window.Close();
        _errors.Dispose();
    }

    private static void Settle() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

    [Test]
    public void Showing_a_node_traces_no_binding_failure()
    {
        int Count<T>(DependencyObject d) { int n = d is T ? 1 : 0; for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(d); i++) n += Count<T>(System.Windows.Media.VisualTreeHelper.GetChild(d, i)); return n; }
        TestContext.Out.WriteLine($"DEBUG items={Count<System.Windows.Controls.TreeViewItem>(_window)} toggles={Count<System.Windows.Controls.Primitives.ToggleButton>(_window)}");
        Assert.That(_errors.Messages, Is.Empty);
    }

    [Test]
    public void Replacing_the_node_traces_no_binding_failure()
    {
        _fill.Node = Parse("""{ "d": "y" }""");
        Settle();

        Assert.That(_errors.Messages, Is.Empty);
    }

    [Test]
    public void Clearing_the_node_traces_no_binding_failure()
    {
        _fill.Node = null;
        Settle();

        Assert.That(_errors.Messages, Is.Empty);
    }

    [Test]
    public void Clearing_the_node_of_a_closed_form_traces_no_binding_failure()
    {
        _window.Content = null;
        _fill.Node = null;
        Settle();

        Assert.That(_errors.Messages, Is.Empty);
    }

    [Test]
    public void Closing_the_form_traces_no_binding_failure()
    {
        _window.Content = null;
        Settle();

        Assert.That(_errors.Messages, Is.Empty);
    }

    public record Form(DataObject Node, IReadOnlyList<DataManualValue> ManualValues);

    [Test]
    public void Closing_a_templated_form_traces_no_binding_failure()
    {
        var template = (DataTemplate)System.Windows.Markup.XamlReader.Parse("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                          xmlns:data="clr-namespace:Joufflu.Data.Controls;assembly=Joufflu.Data">
                <data:DataFill Node="{Binding Node, Mode=OneWay}" ManualValues="{Binding ManualValues}" />
            </DataTemplate>
            """);
        var forms = new System.Collections.ObjectModel.ObservableCollection<Form>
        {
            new(Parse("""{ "a": "x", "b": 1, "c": true }"""), [new DataManualValue(EnumDataType.String, "TBD")])
        };
        _window.Content = new System.Windows.Controls.ItemsControl { ItemTemplate = template, ItemsSource = forms };
        Settle();
        _errors.Clear();

        forms.RemoveAt(0);
        Settle();

        Assert.That(_errors.Messages, Is.Empty);
    }
}
