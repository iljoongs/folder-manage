namespace FolderManage.Features.ImageRename;

/// <summary>
/// 폴더·파일 이름에서 공통 이름을 뽑아 이름 그룹으로 묶는 순수 로직(doc/07-image-rename-spec.md).
/// 파일 시스템에 접근하지 않는다.
/// </summary>
public static class NameGroupBuilder
{
    /// <summary>
    /// 이름 하나를 공통 이름 / 접미사 / 확장자로 나눈다.
    /// 폴더는 접미사도 확장자도 없다. 파일은 마지막 점 뒤가 확장자이고, 확장자 앞에 접미사 목록의 접미사가 있으면 뗀다.
    /// </summary>
    public static NameParts Split(string name, bool isDirectory, IReadOnlyList<string> suffixes)
    {
        if (isDirectory)
        {
            return new NameParts(name, string.Empty, string.Empty);
        }

        var stem = name;
        var extension = string.Empty;

        // 점으로 시작하는 파일(.gitignore)이나 점으로 끝나는 이름은 확장자가 없는 것으로 본다.
        var dot = name.LastIndexOf('.');
        if (dot > 0 && dot < name.Length - 1)
        {
            stem = name[..dot];
            extension = name[dot..];
        }

        // 다른 접미사로 끝나는 접미사가 있을 수 있어 긴 것부터 확인한다. 이름 전체가 접미사인 경우는 뗄 수 없다.
        foreach (var suffix in suffixes.Where(s => s.Length > 0).OrderByDescending(s => s.Length))
        {
            if (stem.Length > suffix.Length && stem.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return new NameParts(stem[..^suffix.Length], stem[^suffix.Length..], extension);
            }
        }

        return new NameParts(stem, string.Empty, extension);
    }

    /// <summary>항목들을 공통 이름(대소문자 무시)으로 묶어 공통 이름 순으로 돌려준다. 그룹 안에서는 폴더가 먼저 온다.</summary>
    public static IReadOnlyList<NameGroup> Build(IEnumerable<DirectoryEntry> entries, IReadOnlyList<string> suffixes)
    {
        var groups = new Dictionary<string, (string CommonName, List<GroupItem> Items)>(StringComparer.OrdinalIgnoreCase);

        var ordered = entries
            .OrderByDescending(e => e.IsDirectory)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in ordered)
        {
            var parts = Split(entry.Name, entry.IsDirectory, suffixes);
            if (!groups.TryGetValue(parts.CommonName, out var group))
            {
                group = (parts.CommonName, new List<GroupItem>());
                groups[parts.CommonName] = group;
            }

            group.Items.Add(new GroupItem(entry.Name, entry.IsDirectory, parts.Suffix, parts.Extension));
        }

        return groups.Values
            .OrderBy(g => g.CommonName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new NameGroup(g.CommonName, g.Items))
            .ToList();
    }
}
