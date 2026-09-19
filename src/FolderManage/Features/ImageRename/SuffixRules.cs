using FolderManage.Common;

namespace FolderManage.Features.ImageRename;

/// <summary>접미사 목록에 추가할 접미사 검사 (순수 로직).</summary>
public static class SuffixRules
{
    public static readonly IReadOnlyList<string> DefaultSuffixes = new[] { ".debug", ".debug-result" };

    /// <summary>추가할 접미사가 올바른지 검사한다. 문제가 있으면 오류 메시지, 없으면 null.</summary>
    public static string? Validate(string suffix, IEnumerable<string> existing)
    {
        if (string.IsNullOrWhiteSpace(suffix))
        {
            return "추가할 접미사를 입력하세요.";
        }

        if (!suffix.StartsWith('.') || suffix.Length < 2)
        {
            return "접미사는 '.'으로 시작해야 합니다 (예: .debug).";
        }

        if (FileNameRules.ContainsInvalidChars(suffix))
        {
            return $"접미사에 사용할 수 없는 문자가 포함되어 있습니다: {FileNameRules.InvalidCharsDisplay}";
        }

        if (existing.Contains(suffix, StringComparer.OrdinalIgnoreCase))
        {
            return "이미 있는 접미사입니다.";
        }

        return null;
    }
}
