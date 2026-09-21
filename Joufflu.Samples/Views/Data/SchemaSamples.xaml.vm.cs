using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Data.Controls;
using Joufflu.Data.Model;
using NJsonSchema;

namespace Joufflu.Samples.Views.Data;

public partial class SchemaSamplesViewModel : ObservableObject
{
    /// <summary>The shape the display is shown against, the same one the editor fills in.</summary>
    public JsonSchema Schema { get; } = JsonSchema.FromType<Order>();
}
