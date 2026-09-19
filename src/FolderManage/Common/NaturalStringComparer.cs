using System.Collections;

namespace FolderManage.Common;

/// <summary>
/// 숫자 덩어리를 숫자 크기로 비교하는 자연 정렬 비교기 ("2"가 "10"보다 앞). 문자는 대소문자를 구분하지 않고 비교한다.
/// 숫자가 같으면(예: "01"과 "1") 원래 문자열을 순서대로 비교해 결과가 항상 같은 순서가 되게 한다.
/// </summary>
public sealed class NaturalStringComparer : IComparer<string>, IComparer
{
    public static NaturalStringComparer Instance { get; } = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var i = 0;
        var j = 0;
        while (i < x.Length && j < y.Length)
        {
            if (IsDigit(x[i]) && IsDigit(y[j]))
            {
                var startX = i;
                while (i < x.Length && IsDigit(x[i]))
                {
                    i++;
                }

                var startY = j;
                while (j < y.Length && IsDigit(y[j]))
                {
                    j++;
                }

                var byNumber = CompareNumbers(x.AsSpan(startX, i - startX), y.AsSpan(startY, j - startY));
                if (byNumber != 0)
                {
                    return byNumber;
                }
            }
            else
            {
                var cx = char.ToUpperInvariant(x[i]);
                var cy = char.ToUpperInvariant(y[j]);
                if (cx != cy)
                {
                    return cx < cy ? -1 : 1;
                }

                i++;
                j++;
            }
        }

        // 한쪽이 먼저 끝나면 짧은 쪽이 앞이다.
        var byRemaining = (x.Length - i).CompareTo(y.Length - j);
        if (byRemaining != 0)
        {
            return byRemaining;
        }

        return Math.Sign(string.CompareOrdinal(x, y));
    }

    int IComparer.Compare(object? x, object? y) => Compare(x as string, y as string);

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    // 자릿수가 다르면 긴 쪽이 크다(앞의 0 제외). 같으면 문자 순서가 곧 숫자 순서다. 아주 긴 숫자도 오버플로 없이 비교된다.
    private static int CompareNumbers(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        a = a.TrimStart('0');
        b = b.TrimStart('0');

        if (a.Length != b.Length)
        {
            return a.Length < b.Length ? -1 : 1;
        }

        return Math.Sign(a.SequenceCompareTo(b));
    }
}
