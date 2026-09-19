using System.IO;

namespace FolderManage.Common;

/// <summary>
/// 폴더/파일 이름에 공통으로 적용하는 Windows 이름 규칙. 탭 1(접두사/접미사)과 탭 2(새 이름)가 함께 쓴다.
/// 파일 시스템에 접근하지 않는 순수 로직이다.
/// </summary>
public static class FileNameRules
{
    public const string InvalidCharsDisplay = "\\ / : * ? \" < > |";

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    public static bool ContainsInvalidChars(string text)
    {
        return text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
    }

    /// <summary>
    /// 이름 하나가 폴더/파일 이름으로 쓸 수 있는지 검사한다. 문제가 있으면 오류 메시지, 없으면 null.
    /// </summary>
    public static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "이름을 입력하세요.";
        }

        if (ContainsInvalidChars(name))
        {
            return $"이름에 사용할 수 없는 문자가 포함되어 있습니다: {InvalidCharsDisplay}";
        }

        if (name.EndsWith('.') || name.EndsWith(' '))
        {
            return "이름은 마침표(.)나 공백으로 끝날 수 없습니다.";
        }

        // 예약 이름은 확장자가 붙어도 예약이다 (예: "con.png").
        var stem = name.Split('.')[0].TrimEnd();
        if (ReservedNames.Contains(stem))
        {
            return $"'{stem}'은(는) Windows 예약 이름이라 사용할 수 없습니다.";
        }

        return null;
    }
}
