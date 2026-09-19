using FolderManage.Common;

namespace FolderManage.Features.ImageRename;

/// <summary>
/// 그룹의 새 이름으로 변경 계획을 만들고 변경 전 충돌 검사를 하는 순수 로직.
/// 대상 폴더에 이미 있는 이름 목록만 받으며 파일 시스템에는 접근하지 않는다.
/// </summary>
public static class RenamePlanner
{
    public static RenamePlan BuildPlan(NameGroup group, string newCommonName, IEnumerable<string> existingNames)
    {
        var nameError = FileNameRules.ValidateName(newCommonName);
        if (nameError is not null)
        {
            return Invalid(group, newCommonName, nameError);
        }

        if (string.Equals(newCommonName, group.CommonName, StringComparison.Ordinal))
        {
            return Invalid(group, newCommonName, "현재 이름과 같습니다. 다른 새 이름을 입력하세요.");
        }

        var existing = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        var items = group.Items
            .Select(item =>
            {
                var newName = newCommonName + item.Suffix + item.Extension;

                // 자기 자신의 이름은 충돌이 아니다 (대소문자만 바꾸는 경우).
                var conflict = existing.Contains(newName)
                    && !string.Equals(newName, item.Name, StringComparison.OrdinalIgnoreCase);

                return new RenamePreviewItem(item.Name, newName, item.IsDirectory, conflict);
            })
            .ToList();

        var conflictNames = items.Where(i => i.HasConflict).Select(i => $"'{i.NewName}'").ToList();
        var error = conflictNames.Count > 0
            ? $"이미 같은 이름이 있어 변경할 수 없습니다: {string.Join(", ", conflictNames)}"
            : null;

        return new RenamePlan(group, newCommonName, items, error);
    }

    private static RenamePlan Invalid(NameGroup group, string newCommonName, string message)
    {
        return new RenamePlan(group, newCommonName, Array.Empty<RenamePreviewItem>(), message);
    }
}
