using System.IO;
using System.Text.RegularExpressions;
using MakeFolder.Models;

namespace MakeFolder.Services;

/// <summary>
/// 폴더 이름 생성 규칙(doc/03-folder-naming-spec.md)을 구현하는 순수 로직.
/// 파일 시스템에 접근하지 않는다.
/// </summary>
public static class FolderNameGenerator
{
    public static FolderSequenceValidationResult Validate(FolderSequenceInput input)
    {
        if (string.IsNullOrWhiteSpace(input.ParentFolderPath))
        {
            return FolderSequenceValidationResult.Failure("상위 폴더를 선택하세요.");
        }

        if (!int.TryParse(input.StartText, out var start) ||
            !int.TryParse(input.EndText, out var end) ||
            !int.TryParse(input.StepText, out var step) ||
            !int.TryParse(input.DigitCountText, out var digitCount))
        {
            return FolderSequenceValidationResult.Failure("시작 번호, 종료 번호, 증가 단위, 자리수는 숫자로 입력하세요.");
        }

        if (start > end)
        {
            return FolderSequenceValidationResult.Failure("시작 번호는 종료 번호보다 클 수 없습니다.");
        }

        if (step <= 0)
        {
            return FolderSequenceValidationResult.Failure("증가 단위는 1 이상이어야 합니다.");
        }

        if (digitCount < 1)
        {
            return FolderSequenceValidationResult.Failure("자리수는 1 이상이어야 합니다.");
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        if (input.Prefix.IndexOfAny(invalidChars) >= 0 || input.Suffix.IndexOfAny(invalidChars) >= 0)
        {
            return FolderSequenceValidationResult.Failure(
                "접두사/접미사에 폴더 이름으로 사용할 수 없는 문자가 포함되어 있습니다: \\ / : * ? \" < > |");
        }

        var count = ((long)end - start) / step + 1;
        var parsed = new ParsedFolderSequence(input.ParentFolderPath, input.Prefix, input.Suffix, start, end, step, digitCount, count);
        return FolderSequenceValidationResult.Success(parsed);
    }

    /// <summary>
    /// 폴더 이름 목록에서 <c>접두사 + 숫자 + 접미사</c> 형식에 맞는 이름의 숫자만 뽑아 오름차순·중복 없이 돌려준다.
    /// 자리수(0 패딩)는 무시한다 (예: "01화"와 "1화"는 모두 1).
    /// </summary>
    public static IReadOnlyList<int> ExtractNumbers(IEnumerable<string> folderNames, string prefix, string suffix)
    {
        var pattern = new Regex(
            $@"^{Regex.Escape(prefix)}([0-9]+){Regex.Escape(suffix)}\z",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var numbers = new SortedSet<int>();
        foreach (var name in folderNames)
        {
            var match = pattern.Match(name);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var number))
            {
                numbers.Add(number);
            }
        }

        return numbers.ToList();
    }

    public static IReadOnlyList<string> GenerateNames(ParsedFolderSequence parsed)
    {
        var names = new List<string>();
        for (long n = parsed.Start; n <= parsed.End; n += parsed.Step)
        {
            var numberText = n.ToString().PadLeft(parsed.DigitCount, '0');
            names.Add($"{parsed.Prefix}{numberText}{parsed.Suffix}");
        }

        return names;
    }
}
