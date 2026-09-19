using FolderManage.Common;
using FolderManage.Features.ImageRename;

namespace FolderManage.Tests;

public class FileNameRulesTests
{
    [Theory]
    [InlineData("ch01")]
    [InlineData("첫 번째 화")]
    [InlineData("v1.2")]
    [InlineData("console")]
    public void ValidateName_AcceptsNormalNames(string name)
    {
        Assert.Null(FileNameRules.ValidateName(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    [InlineData("a:b")]
    [InlineData("a*b")]
    [InlineData("a?b")]
    [InlineData("a\"b")]
    [InlineData("a<b")]
    [InlineData("a>b")]
    [InlineData("a|b")]
    [InlineData("name.")]
    [InlineData("name ")]
    [InlineData("CON")]
    [InlineData("nul")]
    [InlineData("com1")]
    [InlineData("LPT9.txt")]
    public void ValidateName_RejectsBadNames(string name)
    {
        Assert.NotNull(FileNameRules.ValidateName(name));
    }
}

public class SuffixRulesTests
{
    [Fact]
    public void Validate_AcceptsNewSuffix()
    {
        Assert.Null(SuffixRules.ValidateFileSuffix(".raw", new[] { ".debug" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("debug")]
    [InlineData(".")]
    [InlineData(".a/b")]
    [InlineData(".a*b")]
    public void Validate_RejectsBadSuffix(string suffix)
    {
        Assert.NotNull(SuffixRules.ValidateFileSuffix(suffix, Array.Empty<string>()));
    }

    [Fact]
    public void Validate_RejectsDuplicateIgnoringCase()
    {
        Assert.NotNull(SuffixRules.ValidateFileSuffix(".DEBUG", new[] { ".debug" }));
    }

    [Fact]
    public void ValidateFolderSuffix_AcceptsSuffixWithoutLeadingDot()
    {
        Assert.Null(SuffixRules.ValidateFolderSuffix("_files", Array.Empty<string>()));
        Assert.Null(SuffixRules.ValidateFolderSuffix("-data", new[] { "_files" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("_a/b")]
    [InlineData("_a?")]
    public void ValidateFolderSuffix_RejectsBadSuffix(string suffix)
    {
        Assert.NotNull(SuffixRules.ValidateFolderSuffix(suffix, Array.Empty<string>()));
    }

    [Fact]
    public void ValidateFolderSuffix_RejectsDuplicateIgnoringCase()
    {
        Assert.NotNull(SuffixRules.ValidateFolderSuffix("_FILES", new[] { "_files" }));
    }
}
