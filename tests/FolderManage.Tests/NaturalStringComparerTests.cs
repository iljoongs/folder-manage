using FolderManage.Common;

namespace FolderManage.Tests;

public class NaturalStringComparerTests
{
    private static string[] Sort(params string[] values)
    {
        return values.OrderBy(v => v, NaturalStringComparer.Instance).ToArray();
    }

    [Fact]
    public void ComparesNumbersByValueNotByText()
    {
        Assert.Equal(new[] { "1", "2", "9", "10", "11", "100" }, Sort("10", "2", "100", "1", "11", "9"));
    }

    [Fact]
    public void ComparesNumbersInsideText()
    {
        Assert.Equal(
            new[] { "ch1.png", "ch2.png", "ch10.png", "ch20.png" },
            Sort("ch10.png", "ch2.png", "ch20.png", "ch1.png"));
    }

    [Fact]
    public void ComparesKoreanEpisodeNames()
    {
        Assert.Equal(new[] { "1화", "2화", "10화" }, Sort("10화", "2화", "1화"));
    }

    [Fact]
    public void ZeroPaddedAndPlainNumbersSortByValue()
    {
        var sorted = Sort("010", "9", "02", "1");

        Assert.Equal(new[] { "1", "02", "9", "010" }, sorted);
    }

    [Fact]
    public void SameNumberWithDifferentPaddingKeepsAStableOrder()
    {
        Assert.Equal(new[] { "01", "1" }, Sort("1", "01"));
        Assert.Equal(new[] { "01", "1" }, Sort("01", "1"));
    }

    [Fact]
    public void IgnoresCaseWhenComparingLetters()
    {
        Assert.True(NaturalStringComparer.Instance.Compare("a2", "B1") < 0);
        Assert.True(NaturalStringComparer.Instance.Compare("B1", "a2") > 0);
    }

    [Fact]
    public void ShorterPrefixComesFirst()
    {
        Assert.Equal(new[] { "a", "a1", "a1b" }, Sort("a1b", "a1", "a"));
    }

    [Fact]
    public void HandlesNumbersTooLongForAnyIntegerType()
    {
        var big = new string('9', 40);
        var bigger = "1" + new string('0', 40);

        Assert.Equal(new[] { big, bigger }, Sort(bigger, big));
    }

    [Fact]
    public void NullsSortFirstAndEqualStringsAreEqual()
    {
        Assert.True(NaturalStringComparer.Instance.Compare(null, "a") < 0);
        Assert.True(NaturalStringComparer.Instance.Compare("a", null) > 0);
        Assert.Equal(0, NaturalStringComparer.Instance.Compare("a1", "a1"));
        Assert.Equal(0, NaturalStringComparer.Instance.Compare(null, null));
    }
}
