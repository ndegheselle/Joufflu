using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Joufflu.Data.Controls;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Tests.Controls;

[Apartment(ApartmentState.STA)]
public class TempDataFormRender
{
    [Test]
    public async Task Render()
    {
        if (Application.Current == null)
        {
            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Joufflu;component/Resources.xaml") });
        }

        var schema = await JsonSchema.FromJsonAsync("""
        {
          "type": "object",
          "properties": {
            "Name": { "type": "string" },
            "Recipients": { "type": "array", "items": { "type": "string" } },
            "Address": { "type": "object", "properties": { "Street": { "type": "string" }, "City": { "type": "string" } } },
            "Lines": { "type": "array", "items": { "type": "object", "properties": { "Product": { "type": "string" }, "Quantity": { "type": "integer" } } } }
          }
        }
        """);
        var node = (DataObject)schema.ToDataNode();
        node.Load(JObject.Parse("""{ "Name": "n", "Recipients": ["alice@example.com", "bob@example.com"], "Address": { "Street": "s", "City": "c" }, "Lines": [{ "Product": "p", "Quantity": 2 }] }"""));

        using var errors = new BindingErrors();
        var form = new DataForm { Node = node };
        var window = new Window { Width = 420, Height = 900, Left = -10000, ShowActivated = false, ShowInTaskbar = false, Content = form, Background = System.Windows.Media.Brushes.White };
        window.Show();
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

        var bitmap = new RenderTargetBitmap((int)form.ActualWidth, (int)form.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(form);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (var file = File.Create(Environment.GetEnvironmentVariable("RENDER_OUT")!))
            encoder.Save(file);
        window.Close();
        TestContext.Out.WriteLine(string.Join("\n", errors.Messages));
    }
}
