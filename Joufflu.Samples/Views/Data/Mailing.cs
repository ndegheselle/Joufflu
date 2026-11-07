using System.ComponentModel;

namespace Joufflu.Samples.Views.Data;

/// <summary>What the array sample fills in : a list of plain strings, each item an editor of its own.</summary>
public class Mailing
{
    [Description("Who the mail goes to, one address per item.")]
    public List<string> Recipients { get; set; } = [];
}
