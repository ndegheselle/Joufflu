using Joufflu.Data.Model;

namespace Joufflu.Data.Tests.Model;

public class KeysTests
{
    private static DataValue Text(string key) => new(EnumDataType.String, key, []);

    [Test]
    public void A_free_key_is_kept_as_it_is()
    {
        var obj = new DataObject(null) { Properties = [Text("name")] };

        Assert.That(obj.UniqueKey("key"), Is.EqualTo("key"));
    }

    [Test]
    public void A_taken_key_is_numbered_after_a_space()
    {
        var obj = new DataObject(null) { Properties = [Text("key"), Text("key 1")] };

        Assert.That(obj.UniqueKey("key"), Is.EqualTo("key 2"));
    }

    [Test]
    public void Two_properties_with_the_same_key_are_both_flagged()
    {
        var obj = new DataObject(null) { Properties = [Text("a"), Text("a")] };

        Assert.That(obj.Properties.Select(property => property.HasErrors), Is.EqualTo(new[] { true, true }));
    }

    [Test]
    public void Keys_differing_only_by_case_do_not_clash()
    {
        var obj = new DataObject(null) { Properties = [Text("a"), Text("A")] };

        Assert.That(obj.Properties.Any(property => property.HasErrors), Is.False);
    }

    [Test]
    public void Renaming_a_duplicate_clears_both_errors()
    {
        var obj = new DataObject(null) { Properties = [Text("a"), Text("a")] };

        obj.Properties[1].Key = "b";

        Assert.That(obj.Properties.Any(property => property.HasErrors), Is.False);
    }

    [Test]
    public void Removing_a_duplicate_clears_the_one_left()
    {
        DataValue kept = Text("a");
        var obj = new DataObject(null) { Properties = [kept, Text("a")] };

        obj.Remove(obj.Properties[1]);

        Assert.That(kept.HasErrors, Is.False);
    }

    [Test]
    public void Removing_an_array_item_keys_the_ones_after_it_again()
    {
        var array = new DataArray("items", null);
        DataValue first = Text("x"), second = Text("x"), third = Text("x");
        array.Add(first);
        array.Add(second);
        array.Add(third);

        array.Remove(first);

        Assert.That(array.Values, Is.EqualTo(new[] { second, third }));
        Assert.That(array.Values.Select(item => item.Key), Is.EqualTo(new[] { "[0]", "[1]" }));
    }
}
