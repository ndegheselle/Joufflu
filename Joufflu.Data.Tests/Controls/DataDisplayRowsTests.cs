using Joufflu.Data.Controls;
using Joufflu.Data.Model;
using Newtonsoft.Json.Linq;
// System.Windows carries a DataObject of its own; the tree's node is the one meant here.
using DataObject = Joufflu.Data.Model.DataObject;

namespace Joufflu.Data.Tests.Controls;

/// <summary>The tree laid flat as <see cref="DataDisplay"/> shows it, following the changes of the tree.</summary>
public class DataDisplayRowsTests
{
    private readonly List<Action> _pending = [];

    private DataDisplayRows Show(DataNode node)
    {
        var rows = new DataDisplayRows(_pending.Add);
        rows.Show(node);
        return rows;
    }

    /// <summary>What the display does before its next render.</summary>
    private void ApplyPending()
    {
        foreach (Action apply in _pending.ToList())
            apply();
        _pending.Clear();
    }

    private static DataObject Parse(string json) => (DataObject)JToken.Parse(json).ToDataNode();

    private static string[] KeysOf(DataDisplayRows rows) => [.. rows.Select(row => row.Key ?? "")];

    [Test]
    public void An_object_shows_its_properties_each_followed_by_their_children()
    {
        DataDisplayRows rows = Show(Parse("""{ "a": 1, "b": { "c": 2 }, "d": [3] }"""));

        Assert.That(KeysOf(rows), Is.EqualTo(new[] { "a", "b", "c", "d", "[0]" }));
    }

    [Test]
    public void A_collapsed_node_hides_its_children()
    {
        DataObject root = Parse("""{ "b": { "c": 2 }, "e": 3 }""");
        DataDisplayRows rows = Show(root);

        root.Properties[0].IsExpanded = false;
        ApplyPending();

        Assert.That(KeysOf(rows), Is.EqualTo(new[] { "b", "e" }));
    }

    [Test]
    public void An_expanded_node_shows_its_children_again_as_they_were()
    {
        DataObject root = Parse("""{ "b": { "c": { "x": 1 }, "d": { "y": 2 } }, "e": 3 }""");
        var b = (DataObject)root.Properties[0];
        b.Properties[1].IsExpanded = false;
        b.IsExpanded = false;
        DataDisplayRows rows = Show(root);

        b.IsExpanded = true;
        ApplyPending();

        Assert.That(KeysOf(rows), Is.EqualTo(new[] { "b", "c", "x", "d", "e" }));
    }

    [Test]
    public void A_forced_node_hides_its_children()
    {
        DataObject root = Parse("""{ "b": { "c": 2 }, "e": 3 }""");
        DataDisplayRows rows = Show(root);

        root.Properties[0].IsManual = true;
        ApplyPending();

        Assert.That(KeysOf(rows), Is.EqualTo(new[] { "b", "e" }));
    }

    [Test]
    public void Children_added_to_a_shown_node_get_their_rows()
    {
        DataObject root = Parse("""{ "list": [1], "e": 3 }""");
        DataDisplayRows rows = Show(root);

        ((DataArray)root.Properties[0]).Add(new JValue(2).ToDataNode());
        root.Add(new JValue(4).ToDataNode("f"));
        ApplyPending();

        Assert.That(KeysOf(rows), Is.EqualTo(new[] { "list", "[0]", "[1]", "e", "f" }));
    }

    [Test]
    public void Changes_made_together_are_applied_once()
    {
        DataObject root = Parse("""{ "list": [] }""");
        var list = (DataArray)root.Properties[0];
        DataDisplayRows rows = Show(root);

        for (int i = 0; i < 100; i++)
            list.Add(new JValue(i).ToDataNode());

        Assert.That(_pending, Has.Count.EqualTo(1));
        ApplyPending();
        Assert.That(rows, Has.Count.EqualTo(101));
    }

    [Test]
    public void A_tree_no_longer_shown_is_no_longer_followed()
    {
        DataObject root = Parse("""{ "b": { "c": 2 } }""");
        DataDisplayRows rows = Show(root);

        rows.Show(null);
        root.Properties[0].IsExpanded = false;

        Assert.That(_pending, Is.Empty);
    }
}
