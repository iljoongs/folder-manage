namespace FolderManage.Features.ImageRename;

/// <summary>
/// 그룹 하나를 새 이름으로 바꾸는 계획. 검증 실패나 이름 충돌이 있으면 <see cref="ErrorMessage"/>가 채워지고
/// 실행할 수 없다(변경을 시도하지 않는다).
/// </summary>
public sealed record RenamePlan(
    NameGroup Group,
    string NewCommonName,
    IReadOnlyList<RenamePreviewItem> Items,
    string? ErrorMessage)
{
    public bool CanExecute => ErrorMessage is null && Items.Count > 0;

    public IReadOnlyList<RenameMove> ToMoves()
    {
        return Items.Select(i => new RenameMove(i.OldName, i.NewName, i.IsDirectory)).ToList();
    }
}
