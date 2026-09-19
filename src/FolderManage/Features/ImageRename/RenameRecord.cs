namespace FolderManage.Features.ImageRename;

/// <summary>성공한 이름 변경 한 번의 기록. 되돌리기에 쓴다.</summary>
public sealed record RenameRecord(
    string DirectoryPath,
    string OldCommonName,
    string NewCommonName,
    IReadOnlyList<RenameMove> Moves);
