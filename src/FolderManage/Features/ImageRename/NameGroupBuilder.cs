using FolderManage.Common;

namespace FolderManage.Features.ImageRename;

/// <summary>
/// 폴더·파일 이름에서 공통 이름을 뽑아 이름 그룹으로 묶는 순수 로직(doc/07-image-rename-spec.md).
/// 파일 시스템에 접근하지 않는다.
/// </summary>
public static class NameGroupBuilder
{
    /// <summary>
    /// 이름 하나를 공통 이름 / 접미사 / 확장자로 나눈다.
    /// 폴더는 확장자가 없고, 이름 끝에 폴더 접미사(<c>_files</c>)가 있으면 뗀다.
    /// 파일은 마지막 점 뒤가 확장자이고, 확장자 앞에 파일 접미사(<c>.debug</c>)가 있으면 뗀다.
    /// </summary>
    public static NameParts Split(string name, bool isDirectory, SuffixSettings suffixes)
    {
        if (isDirectory)
        {
            var (folderCommon, folderSuffix) = StripSuffix(name, suffixes.FolderSuffixes);
            return new NameParts(folderCommon, folderSuffix, string.Empty);
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

        var (common, suffix) = StripSuffix(stem, suffixes.FileSuffixes);
        return new NameParts(common, suffix, extension);
    }

    /// <summary>
    /// 항목들을 공통 이름(대소문자 무시)으로 묶어 공통 이름의 자연 정렬 순서(숫자는 숫자 크기순)로 돌려준다.
    /// 그룹 안에서는 폴더가 먼저 온다.
    /// </summary>
    public static IReadOnlyList<NameGroup> Build(IEnumerable<DirectoryEntry> entries, SuffixSettings suffixes)
    {
        var groups = new Dictionary<string, (string CommonName, List<GroupItem> Items)>(StringComparer.OrdinalIgnoreCase);

        var ordered = entries
            .OrderByDescending(e => e.IsDirectory)
            .ThenBy(e => e.Name, NaturalStringComparer.Instance);

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
            .OrderBy(g => g.CommonName, NaturalStringComparer.Instance)
            .Select(g => new NameGroup(g.CommonName, g.Items))
            .ToList();
    }

    // 다른 접미사로 끝나는 접미사가 있을 수 있어 긴 것부터 확인한다. 이름 전체가 접미사인 경우는 뗄 수 없다.
    private static (string CommonName, string Suffix) StripSuffix(string text, IReadOnlyList<string> suffixes)
    {
        foreach (var suffix in suffixes.Where(s => s.Length > 0).OrderByDescending(s => s.Length))
        {
            if (text.Length > suffix.Length && text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return (text[..^suffix.Length], text[^suffix.Length..]);
            }
        }

        return (text, string.Empty);
    }
}
