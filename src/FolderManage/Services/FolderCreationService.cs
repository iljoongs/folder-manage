using System.IO;
using FolderManage.Models;

namespace FolderManage.Services;

/// <summary>
/// 상위 폴더 기준으로 폴더 이름의 상태를 확인하고 실제로 생성하는 로직 (doc/03-folder-naming-spec.md).
/// </summary>
public static class FolderCreationService
{
    public static IReadOnlyList<string> GetSubfolderNames(string parentFolderPath)
    {
        return new DirectoryInfo(parentFolderPath)
            .EnumerateDirectories()
            .Select(d => d.Name)
            .ToList();
    }

    public static FolderPreviewItem GetStatus(string parentFolderPath, string name)
    {
        var fullPath = Path.Combine(parentFolderPath, name);

        if (Directory.Exists(fullPath))
        {
            return new FolderPreviewItem(name, FolderItemStatus.AlreadyExists);
        }

        if (File.Exists(fullPath))
        {
            return new FolderPreviewItem(name, FolderItemStatus.Conflict);
        }

        return new FolderPreviewItem(name, FolderItemStatus.New);
    }

    public static IReadOnlyList<FolderPreviewItem> BuildPreview(string parentFolderPath, IReadOnlyList<string> names)
    {
        var items = new List<FolderPreviewItem>(names.Count);
        foreach (var name in names)
        {
            items.Add(GetStatus(parentFolderPath, name));
        }

        return items;
    }

    public static FolderCreationSummary CreateAll(string parentFolderPath, IReadOnlyList<FolderPreviewItem> items)
    {
        var created = 0;
        var alreadyExists = 0;
        var conflict = 0;
        var failures = new List<FolderCreationFailure>();

        foreach (var item in items)
        {
            if (item.Status == FolderItemStatus.AlreadyExists)
            {
                alreadyExists++;
                continue;
            }

            if (item.Status == FolderItemStatus.Conflict)
            {
                conflict++;
                continue;
            }

            var fullPath = Path.Combine(parentFolderPath, item.Name);
            try
            {
                Directory.CreateDirectory(fullPath);
                created++;
            }
            catch (Exception ex)
            {
                failures.Add(new FolderCreationFailure(item.Name, ex.Message));
            }
        }

        return new FolderCreationSummary(created, alreadyExists, conflict, failures);
    }
}
