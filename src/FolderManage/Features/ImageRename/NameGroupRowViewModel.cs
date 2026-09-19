namespace FolderManage.Features.ImageRename;

/// <summary>그룹 목록의 한 줄을 화면용으로 감싼 것.</summary>
public sealed class NameGroupRowViewModel
{
    public NameGroupRowViewModel(NameGroup group)
    {
        Group = group;
    }

    public NameGroup Group { get; }

    public string CommonName => Group.CommonName;

    public string CountText
    {
        get
        {
            var folders = Group.Items.Count(i => i.IsDirectory);
            var files = Group.Items.Count - folders;
            var parts = new List<string>();
            if (folders > 0)
            {
                parts.Add($"폴더 {folders}개");
            }

            if (files > 0)
            {
                parts.Add($"파일 {files}개");
            }

            return string.Join(", ", parts);
        }
    }

    public string MembersText => string.Join(", ", Group.Items.Select(i => i.Name));
}
