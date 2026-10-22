using Joufflu.Data.Model;

namespace Joufflu.Data.Tests.Model;

/// <summary>
/// <see cref="DataNode.Changed"/> tells a host the data of the tree changed, so it reports edits and nothing else.
/// </summary>
public class ChangedTests
{
    private static DataValue Text(string key) => new(EnumDataType.String, key, []);

    private static int CountChanges(DataNode root, Action change)
    {
        int count = 0;
        root.Changed += (_, _) => count++;
        change();
        return count;
    }

    [Test]
    public void An_edited_value_is_reported_up_to_the_root()
    {
        DataValue name = Text("name");
        var root = new DataObject(null) { Properties = [new DataObject("person") { Properties = [name] }] };

        int changes = CountChanges(root, () => name.Value = "Alice");

        Assert.That(changes, Is.EqualTo(1));
    }

    [Test]
    public void Renaming_a_key_into_a_duplicate_is_reported_once()
    {
        DataValue second = Text("b");
        var root = new DataObject(null) { Properties = [Text("a"), second] };

        int changes = CountChanges(root, () => second.Key = "a");

        Assert.That(changes, Is.EqualTo(1));
    }

    [Test]
    public void Expanding_a_node_is_not_reported()
    {
        var child = new DataObject("child");
        var root = new DataObject(null) { Properties = [child] };

        int changes = CountChanges(root, () => child.IsExpanded = false);

        Assert.That(changes, Is.Zero);
    }

    [Test]
    public void Forcing_a_value_is_reported()
    {
        DataValue amount = Text("amount");
        var root = new DataObject(null) { Properties = [amount] };

        int changes = CountChanges(root, () => amount.IsManual = true);

        Assert.That(changes, Is.EqualTo(1));
    }
}
