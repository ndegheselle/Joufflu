using Joufflu.FileExplorer.Nodes;

namespace Joufflu.FileExplorer.Tests.Paths;

public class ExplorerPathsTests
{
    [Test]
    public void Paths_differing_only_by_case_are_the_same_node()
    {
        Assert.That(ExplorerPaths.AreEqual(@"C:\Data\Report.txt", @"c:\data\REPORT.TXT"), Is.True);
    }

    [Test]
    public void A_trailing_separator_does_not_make_another_node()
    {
        Assert.That(ExplorerPaths.AreEqual(@"C:\Data\", @"C:\Data"), Is.True);
    }

    [Test]
    public void A_directory_is_its_own_ancestor()
    {
        Assert.That(ExplorerPaths.IsSameOrAncestor(@"C:\Data", @"C:\Data"), Is.True);
    }

    [Test]
    public void A_parent_is_an_ancestor_whatever_the_case()
    {
        Assert.That(ExplorerPaths.IsSameOrAncestor(@"C:\Data", @"c:\data\Sub\file.txt"), Is.True);
    }

    [Test]
    public void A_sibling_sharing_a_prefix_is_not_an_ancestor()
    {
        Assert.That(ExplorerPaths.IsSameOrAncestor(@"C:\foo", @"C:\foo2\file.txt"), Is.False);
    }

    [Test]
    public void A_child_is_not_an_ancestor_of_its_parent()
    {
        Assert.That(ExplorerPaths.IsSameOrAncestor(@"C:\Data\Sub", @"C:\Data"), Is.False);
    }

    [Test]
    public void A_node_directly_in_a_directory_is_its_child()
    {
        Assert.That(ExplorerPaths.IsDirectChild(@"C:\Data\file.txt", @"c:\data\"), Is.True);
    }

    [Test]
    public void A_node_deeper_in_a_directory_is_not_its_child()
    {
        Assert.That(ExplorerPaths.IsDirectChild(@"C:\Data\Sub\file.txt", @"C:\Data"), Is.False);
    }

    [Test]
    public void An_invalid_path_is_the_child_of_no_directory()
    {
        Assert.That(ExplorerPaths.IsDirectChild("C:\\Data\\fi\0le.txt", @"C:\Data"), Is.False);
    }

    [Test]
    public void Rebasing_moves_a_path_under_the_new_ancestor()
    {
        string rebased = ExplorerPaths.Rebase(@"C:\Old\Sub\Deeper", @"C:\Old", @"C:\New");

        Assert.That(rebased, Is.EqualTo(@"C:\New\Sub\Deeper"));
    }

    [Test]
    public void Rebasing_the_ancestor_itself_gives_the_new_ancestor()
    {
        Assert.That(ExplorerPaths.Rebase(@"C:\Old", @"C:\Old", @"C:\New"), Is.EqualTo(@"C:\New"));
    }

    [Test]
    public void Rebasing_does_not_depend_on_how_the_ancestor_is_written()
    {
        string rebased = ExplorerPaths.Rebase(@"C:\OLD\Sub", @"c:\old\", @"C:\New");

        Assert.That(rebased, Is.EqualTo(@"C:\New\Sub"));
    }

    [Test]
    public void A_copy_is_named_like_windows_names_it()
    {
        Assert.That(ExplorerPaths.CopyName("Report.txt", isTaken: _ => false), Is.EqualTo("Report - Copy.txt"));
    }

    [Test]
    public void A_copy_whose_name_is_taken_is_numbered_from_two()
    {
        string name = ExplorerPaths.CopyName("Report.txt", isTaken: candidate => candidate == "Report - Copy.txt");

        Assert.That(name, Is.EqualTo("Report - Copy (2).txt"));
    }

    [Test]
    public void A_copy_of_a_name_without_extension_gets_none()
    {
        Assert.That(ExplorerPaths.CopyName("Folder", isTaken: _ => false), Is.EqualTo("Folder - Copy"));
    }

    [Test]
    public void A_new_directory_whose_name_is_taken_is_numbered_from_two()
    {
        string name = ExplorerPaths.NewDirectoryName(isTaken: candidate => candidate == "New folder");

        Assert.That(name, Is.EqualTo("New folder (2)"));
    }
}
