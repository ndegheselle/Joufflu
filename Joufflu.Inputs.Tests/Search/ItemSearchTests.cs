using Joufflu.Inputs.Controls;

namespace Joufflu.Inputs.Tests.Search;

public class ItemSearchTests
{
    private sealed record Address(string City);
    private sealed record Customer(string Name, Address Address);

    private static readonly Customer Alice = new("Alice", new Address("Paris"));

    [Test]
    public void Reads_the_item_itself_without_a_display_member_path()
    {
        Assert.That(ItemSearch.TextOf("Paris", null), Is.EqualTo("Paris"));
    }

    [Test]
    public void Reads_the_property_named_by_the_display_member_path()
    {
        Assert.That(ItemSearch.TextOf(Alice, "Name"), Is.EqualTo("Alice"));
    }

    [Test]
    public void Follows_a_nested_display_member_path()
    {
        Assert.That(ItemSearch.TextOf(Alice, "Address.City"), Is.EqualTo("Paris"));
    }

    [Test]
    public void Keeps_an_item_containing_the_search_whatever_its_case()
    {
        Assert.That(ItemSearch.Passes("Marseille", "SEILLE", selectedItem: null, displayMemberPath: null), Is.True);
    }

    [Test]
    public void Drops_an_item_not_containing_the_search()
    {
        Assert.That(ItemSearch.Passes("Lyon", "par", selectedItem: null, displayMemberPath: null), Is.False);
    }

    [Test]
    public void Keeps_every_item_while_the_text_is_the_selected_item()
    {
        Assert.That(ItemSearch.Passes("Lyon", "Paris", selectedItem: "Paris", displayMemberPath: null), Is.True);
    }

    [Test]
    public void Finds_the_item_matching_the_text_exactly_rather_than_one_containing_it()
    {
        string[] items = ["Paris", "Par"];

        Assert.That(ItemSearch.IndexOfExactMatch(items, "Par", displayMemberPath: null), Is.EqualTo(1));
    }

    [Test]
    public void Finds_no_match_for_an_empty_text()
    {
        string[] items = ["", "Paris"];

        Assert.That(ItemSearch.IndexOfExactMatch(items, "", displayMemberPath: null), Is.EqualTo(-1));
    }
}
