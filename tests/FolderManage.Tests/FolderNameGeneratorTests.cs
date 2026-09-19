using FolderManage.Features.MakeFolder;

namespace FolderManage.Tests;

public class FolderNameGeneratorTests
{
    private static FolderSequenceInput MakeInput(
        string parentFolderPath = @"C:\parent",
        string prefix = "",
        string suffix = "",
        string startText = "1",
        string endText = "5",
        string stepText = "1",
        string digitCountText = "1")
    {
        return new FolderSequenceInput
        {
            ParentFolderPath = parentFolderPath,
            Prefix = prefix,
            Suffix = suffix,
            StartText = startText,
            EndText = endText,
            StepText = stepText,
            DigitCountText = digitCountText,
        };
    }

    [Fact]
    public void Generate_SimpleRange_ProducesSequentialNames()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(startText: "1", endText: "5", stepText: "1", digitCountText: "1"));

        Assert.True(validation.IsSuccess);
        var names = FolderNameGenerator.GenerateNames(validation.Parsed!);

        Assert.Equal(new[] { "1", "2", "3", "4", "5" }, names);
    }

    [Fact]
    public void Generate_WithStep_SkipsIntermediateValuesAndNeverExceedsEnd()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(startText: "1", endText: "10", stepText: "2", digitCountText: "1"));

        var names = FolderNameGenerator.GenerateNames(validation.Parsed!);

        Assert.Equal(new[] { "1", "3", "5", "7", "9" }, names);
    }

    [Theory]
    [InlineData(1, "01")]
    [InlineData(9, "09")]
    [InlineData(10, "10")]
    [InlineData(100, "100")]
    public void Generate_DigitCountIsMinimumWidth_DoesNotTruncateLongerNumbers(int number, string expected)
    {
        var validation = FolderNameGenerator.Validate(MakeInput(startText: number.ToString(), endText: number.ToString(), digitCountText: "2"));

        var names = FolderNameGenerator.GenerateNames(validation.Parsed!);

        Assert.Equal(new[] { expected }, names);
    }

    [Fact]
    public void Generate_WithPrefixAndSuffix_WrapsPaddedNumber()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(prefix: "Chapter ", suffix: "_raw", startText: "1", endText: "1", digitCountText: "2"));

        var names = FolderNameGenerator.GenerateNames(validation.Parsed!);

        Assert.Equal(new[] { "Chapter 01_raw" }, names);
    }

    [Fact]
    public void Validate_StartGreaterThanEnd_Fails()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(startText: "5", endText: "1"));

        Assert.False(validation.IsSuccess);
        Assert.NotNull(validation.ErrorMessage);
    }

    [Fact]
    public void Validate_StepZeroOrNegative_Fails()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(stepText: "0"));

        Assert.False(validation.IsSuccess);
    }

    [Fact]
    public void Validate_DigitCountLessThanOne_Fails()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(digitCountText: "0"));

        Assert.False(validation.IsSuccess);
    }

    [Theory]
    [InlineData("abc", "5", "1", "1")]
    [InlineData("1", "abc", "1", "1")]
    [InlineData("1", "5", "abc", "1")]
    [InlineData("1", "5", "1", "abc")]
    [InlineData("", "5", "1", "1")]
    public void Validate_NonNumericOrEmptyNumericFields_Fails(string start, string end, string step, string digits)
    {
        var validation = FolderNameGenerator.Validate(MakeInput(startText: start, endText: end, stepText: step, digitCountText: digits));

        Assert.False(validation.IsSuccess);
    }

    [Fact]
    public void Validate_EmptyParentFolderPath_Fails()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(parentFolderPath: ""));

        Assert.False(validation.IsSuccess);
    }

    [Theory]
    [InlineData("bad\\prefix")]
    [InlineData("bad/prefix")]
    [InlineData("bad:prefix")]
    [InlineData("bad*prefix")]
    [InlineData("bad?prefix")]
    [InlineData("bad\"prefix")]
    [InlineData("bad<prefix")]
    [InlineData("bad>prefix")]
    [InlineData("bad|prefix")]
    public void Validate_InvalidCharactersInPrefix_Fails(string prefix)
    {
        var validation = FolderNameGenerator.Validate(MakeInput(prefix: prefix));

        Assert.False(validation.IsSuccess);
    }

    [Fact]
    public void Validate_InvalidCharactersInSuffix_Fails()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(suffix: "bad:suffix"));

        Assert.False(validation.IsSuccess);
    }

    [Fact]
    public void ExtractNumbers_MatchesPrefixNumberSuffix_SortedAndDistinct()
    {
        var names = new[] { "20화", "1화", "2화", "abc", "3", "10회", "화", "1화 ", "-1화", "2화" };

        var numbers = FolderNameGenerator.ExtractNumbers(names, "", "화");

        Assert.Equal(new[] { 1, 2, 20 }, numbers);
    }

    [Fact]
    public void ExtractNumbers_IgnoresZeroPadding()
    {
        var numbers = FolderNameGenerator.ExtractNumbers(new[] { "01화", "1화", "003화" }, "", "화");

        Assert.Equal(new[] { 1, 3 }, numbers);
    }

    [Fact]
    public void ExtractNumbers_WithPrefixAndRegexSpecialCharacters_TreatsThemLiterally()
    {
        var names = new[] { "(v)1.x", "(v)2.x", "(v)3Yx", "v1.x" };

        var numbers = FolderNameGenerator.ExtractNumbers(names, "(v)", ".x");

        Assert.Equal(new[] { 1, 2 }, numbers);
    }

    [Fact]
    public void ExtractNumbers_EmptyPrefixAndSuffix_MatchesPureNumberNames()
    {
        var numbers = FolderNameGenerator.ExtractNumbers(new[] { "1", "02", "a", "1화" }, "", "");

        Assert.Equal(new[] { 1, 2 }, numbers);
    }

    [Fact]
    public void ExtractNumbers_NumberTooLargeForInt_IsIgnored()
    {
        var numbers = FolderNameGenerator.ExtractNumbers(new[] { "99999999999화", "5화" }, "", "화");

        Assert.Equal(new[] { 5 }, numbers);
    }

    [Fact]
    public void ExtractNumbers_IsCaseInsensitive()
    {
        var numbers = FolderNameGenerator.ExtractNumbers(new[] { "EP1", "ep2" }, "Ep", "");

        Assert.Equal(new[] { 1, 2 }, numbers);
    }

    [Fact]
    public void Validate_ComputesCountWithoutMaterializingNames()
    {
        var validation = FolderNameGenerator.Validate(MakeInput(startText: "1", endText: "9", stepText: "2"));

        Assert.True(validation.IsSuccess);
        Assert.Equal(5, validation.Parsed!.Count);
    }
}
