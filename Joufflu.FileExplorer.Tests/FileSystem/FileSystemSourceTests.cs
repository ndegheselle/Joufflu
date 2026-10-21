using System.IO;
using Joufflu.FileExplorer.FileSystem;
using Joufflu.FileExplorer.Nodes;

namespace Joufflu.FileExplorer.Tests.FileSystem;

/// <summary>
/// Opening directories of a real, temporary directory : the disk is fast enough here to be used as it is.
/// </summary>
public class FileSystemSourceTests
{
    private string _root = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), "Joufflu.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "Sub"));
        File.WriteAllText(Path.Combine(_root, "a.txt"), "a");
        File.WriteAllText(Path.Combine(_root, "Sub", "b.txt"), "b");
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_root, recursive: true);

    private static IEnumerable<string> Names(IExplorerDirectory directory)
        => directory.Children.Select(child => child.Name).Order();

    private static IExplorerDirectory Child(IExplorerDirectory directory, string name)
        => directory.Children.OfType<IExplorerDirectory>().Single(child => child.Name == name);

    [Test]
    public async Task Opening_shows_the_directory_and_its_sub_directories_and_makes_it_current()
    {
        var source = new FileSystemSource(_root, toasts: null);

        await source.Open();

        Assert.That(source.Current, Is.SameAs(source.Root));
        Assert.That(Names(source.Root!), Is.EqualTo(new[] { "a.txt", "Sub" }));
        Assert.That(Names(Child(source.Root!, "Sub")), Is.EqualTo(new[] { "b.txt" }));
    }

    [Test]
    public async Task Opening_a_directory_that_cannot_be_read_keeps_the_current_one()
    {
        var source = new FileSystemSource(_root, toasts: null);
        await source.Open();
        var missing = new FileSystemDirectory(new DirectoryInfo(Path.Combine(_root, "Missing")), source.Root);

        await source.Open(missing);

        Assert.That(source.Current, Is.SameAs(source.Root));
    }

    [Test]
    public async Task Opening_again_keeps_the_nodes_still_on_the_disk()
    {
        var source = new FileSystemSource(_root, toasts: null);
        await source.Open();
        IExplorerDirectory sub = Child(source.Root!, "Sub");
        File.WriteAllText(Path.Combine(_root, "c.txt"), "c");

        await source.Open(source.Root!);

        Assert.That(Child(source.Root!, "Sub"), Is.SameAs(sub));
        Assert.That(Names(source.Root!), Is.EqualTo(new[] { "a.txt", "c.txt", "Sub" }));
    }

    [Test]
    public async Task An_override_of_ApplyDirectory_sees_every_directory_read()
    {
        var source = new RecordingSource(_root);

        await source.Open();

        Assert.That(source.Applied, Is.EquivalentTo(new[] { _root, Path.Combine(_root, "Sub") }));
    }

    private sealed class RecordingSource(string root) : FileSystemSource(root, toasts: null)
    {
        public List<string> Applied { get; } = [];

        protected override void ApplyDirectory(IExplorerDirectory directory, DirectoryRead read)
        {
            base.ApplyDirectory(directory, read);
            Applied.Add(directory.Path);
        }
    }
}
