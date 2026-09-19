using FolderManage.Features.ImageRename;

namespace FolderManage.Tests;

public class NameGroupBuilderTests
{
    private static readonly string[] DefaultSuffixes = { ".debug", ".debug-result" };

    [Theory]
    [InlineData("001.png", "001", "", ".png")]
    [InlineData("001.debug.png", "001", ".debug", ".png")]
    [InlineData("001.debug-result.png", "001", ".debug-result", ".png")]
    [InlineData("001.DEBUG.png", "001", ".DEBUG", ".png")]
    [InlineData("001.v2.png", "001.v2", "", ".png")]
    [InlineData("001.v2.debug.png", "001.v2", ".debug", ".png")]
    [InlineData("readme", "readme", "", "")]
    [InlineData(".gitignore", ".gitignore", "", "")]
    [InlineData("abc.", "abc.", "", "")]
    [InlineData(".debug.png", ".debug", "", ".png")]
    public void Split_File(string name, string common, string suffix, string extension)
    {
        var parts = NameGroupBuilder.Split(name, isDirectory: false, DefaultSuffixes);

        Assert.Equal(common, parts.CommonName);
        Assert.Equal(suffix, parts.Suffix);
        Assert.Equal(extension, parts.Extension);
    }

    [Theory]
    [InlineData("001")]
    [InlineData("v1.2")]
    [InlineData("001.debug")]
    public void Split_Folder_UsesWholeNameAndNeverStripsSuffix(string name)
    {
        var parts = NameGroupBuilder.Split(name, isDirectory: true, DefaultSuffixes);

        Assert.Equal(name, parts.CommonName);
        Assert.Equal(string.Empty, parts.Suffix);
        Assert.Equal(string.Empty, parts.Extension);
    }

    [Fact]
    public void Split_LongerSuffixWinsWhenOneSuffixEndsWithAnother()
    {
        var suffixes = new[] { ".result", ".debug-result" };

        var parts = NameGroupBuilder.Split("001.debug-result.png", isDirectory: false, suffixes);

        Assert.Equal("001", parts.CommonName);
        Assert.Equal(".debug-result", parts.Suffix);
    }

    [Fact]
    public void Split_EmptySuffixInListIsIgnored()
    {
        var parts = NameGroupBuilder.Split("001.png", isDirectory: false, new[] { string.Empty });

        Assert.Equal("001", parts.CommonName);
    }

    [Fact]
    public void Build_GroupsFolderAndFilesWithAndWithoutSuffix()
    {
        var entries = new[]
        {
            new DirectoryEntry("001", true),
            new DirectoryEntry("001.png", false),
            new DirectoryEntry("001.debug.png", false),
            new DirectoryEntry("001.debug-result.png", false),
            new DirectoryEntry("002", true),
            new DirectoryEntry("002.png", false),
        };

        var groups = NameGroupBuilder.Build(entries, DefaultSuffixes);

        Assert.Equal(new[] { "001", "002" }, groups.Select(g => g.CommonName));
        Assert.Equal(4, groups[0].Items.Count);
        Assert.Equal(2, groups[1].Items.Count);
        Assert.True(groups[0].Items[0].IsDirectory);
    }

    [Fact]
    public void Build_GroupWithOnlySuffixFileIsStillAGroup()
    {
        var groups = NameGroupBuilder.Build(new[] { new DirectoryEntry("003.debug.png", false) }, DefaultSuffixes);

        var group = Assert.Single(groups);
        Assert.Equal("003", group.CommonName);
        Assert.Equal(".debug", group.Items[0].Suffix);
    }

    [Fact]
    public void Build_FolderWithSuffixLikeNameIsNotGroupedWithFiles()
    {
        var entries = new[]
        {
            new DirectoryEntry("001.debug", true),
            new DirectoryEntry("001.debug.png", false),
        };

        var groups = NameGroupBuilder.Build(entries, DefaultSuffixes);

        Assert.Equal(2, groups.Count);
        Assert.Contains(groups, g => g.CommonName == "001.debug");
        Assert.Contains(groups, g => g.CommonName == "001");
    }

    [Fact]
    public void Build_GroupsCaseInsensitively()
    {
        var entries = new[]
        {
            new DirectoryEntry("Ch01", true),
            new DirectoryEntry("ch01.png", false),
        };

        var groups = NameGroupBuilder.Build(entries, DefaultSuffixes);

        var group = Assert.Single(groups);
        Assert.Equal("Ch01", group.CommonName);
        Assert.Equal(2, group.Items.Count);
    }

    [Fact]
    public void Build_WithChangedSuffixListRegroups()
    {
        var entries = new[] { new DirectoryEntry("001.png", false), new DirectoryEntry("001.raw.png", false) };

        Assert.Equal(2, NameGroupBuilder.Build(entries, DefaultSuffixes).Count);
        Assert.Single(NameGroupBuilder.Build(entries, new[] { ".raw" }));
    }
}
