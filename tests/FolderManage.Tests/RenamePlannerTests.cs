using FolderManage.Features.ImageRename;

namespace FolderManage.Tests;

public class RenamePlannerTests
{
    private static readonly SuffixSettings DefaultSuffixes = SuffixSettings.Default;

    private static NameGroup GroupOf(string commonName, params string[] names)
    {
        // 폴더는 확장자가 없는 이름(점 없음)으로 간주한다.
        var entries = names.Select(n => new DirectoryEntry(n, !n.Contains('.')));
        return NameGroupBuilder.Build(entries, DefaultSuffixes).Single(g => g.CommonName == commonName);
    }

    [Fact]
    public void BuildPlan_KeepsSuffixAndExtension()
    {
        var names = new[] { "001", "001.png", "001.debug.png", "001.debug-result.png" };
        var group = GroupOf("001", names);

        var plan = RenamePlanner.BuildPlan(group, "ch01", names);

        Assert.True(plan.CanExecute);
        Assert.Equal(
            new[] { "ch01", "ch01.debug-result.png", "ch01.debug.png", "ch01.png" },
            plan.Items.Select(i => i.NewName));
        Assert.All(plan.Items, i => Assert.False(i.HasConflict));
    }

    [Fact]
    public void BuildPlan_OnlyRenamesItemsThatExistInTheGroup()
    {
        var names = new[] { "003.debug.png", "004", "004.png" };
        var group = GroupOf("003", names);

        var plan = RenamePlanner.BuildPlan(group, "ch03", names);

        var item = Assert.Single(plan.Items);
        Assert.Equal("003.debug.png", item.OldName);
        Assert.Equal("ch03.debug.png", item.NewName);
    }

    [Fact]
    public void BuildPlan_ExistingNameIsConflictAndBlocksPlan()
    {
        var group = GroupOf("001", "001", "001.png");
        var existing = new[] { "001", "001.png", "ch01.png", "other.txt" };

        var plan = RenamePlanner.BuildPlan(group, "ch01", existing);

        Assert.False(plan.CanExecute);
        Assert.Contains("ch01.png", plan.ErrorMessage);
        Assert.False(plan.Items.Single(i => i.OldName == "001").HasConflict);
        Assert.True(plan.Items.Single(i => i.OldName == "001.png").HasConflict);
    }

    [Fact]
    public void BuildPlan_ConflictCheckIgnoresCase()
    {
        var group = GroupOf("001", "001.png");

        var plan = RenamePlanner.BuildPlan(group, "ch01", new[] { "001.png", "CH01.PNG" });

        Assert.False(plan.CanExecute);
    }

    [Fact]
    public void BuildPlan_CaseOnlyChangeIsNotAConflict()
    {
        var group = GroupOf("abc", "abc", "abc.png");

        var plan = RenamePlanner.BuildPlan(group, "ABC", new[] { "abc", "abc.png" });

        Assert.True(plan.CanExecute);
        Assert.Equal(new[] { "ABC", "ABC.png" }, plan.Items.Select(i => i.NewName));
    }

    [Fact]
    public void BuildPlan_SameNameIsRejected()
    {
        var group = GroupOf("001", "001.png");

        var plan = RenamePlanner.BuildPlan(group, "001", new[] { "001.png" });

        Assert.False(plan.CanExecute);
        Assert.Empty(plan.Items);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a/b")]
    [InlineData("CON")]
    [InlineData("name.")]
    public void BuildPlan_InvalidNewNameIsRejected(string newName)
    {
        var group = GroupOf("001", "001.png");

        var plan = RenamePlanner.BuildPlan(group, newName, new[] { "001.png" });

        Assert.False(plan.CanExecute);
        Assert.Empty(plan.Items);
        Assert.NotNull(plan.ErrorMessage);
    }

    [Fact]
    public void BuildPlan_NewNameLandingOnAnotherItemOfTheSameGroupIsAConflict()
    {
        // "a" → "a.debug"이면 a.png가 이미 있는 a.debug.png(같은 그룹)와 겹친다. 순차 변경은 지원하지 않으므로 충돌이다.
        var names = new[] { "a.png", "a.debug.png" };
        var group = GroupOf("a", names);

        var plan = RenamePlanner.BuildPlan(group, "a.debug", names);

        Assert.False(plan.CanExecute);
    }

    [Fact]
    public void BuildPlan_KeepsFolderSuffixOnTheFilesFolder()
    {
        var names = new[] { "001_files", "001.html" };
        var group = GroupOf("001", names);

        var plan = RenamePlanner.BuildPlan(group, "ch01", names);

        Assert.True(plan.CanExecute);
        Assert.Equal(new[] { "ch01_files", "ch01.html" }, plan.Items.Select(i => i.NewName));
    }

    [Fact]
    public void BuildPlan_ExistingFilesFolderNameIsAConflict()
    {
        var names = new[] { "001_files", "001.html", "ch01_files" };
        var group = GroupOf("001", "001_files", "001.html");

        var plan = RenamePlanner.BuildPlan(group, "ch01", names);

        Assert.False(plan.CanExecute);
        Assert.Contains("ch01_files", plan.ErrorMessage);
    }
}
