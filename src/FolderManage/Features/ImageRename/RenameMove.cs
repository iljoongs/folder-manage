namespace FolderManage.Features.ImageRename;

/// <summary>대상 폴더 안에서 항목 하나의 이름을 바꾸는 작업 (전체 이름 기준, 경로 아님).</summary>
public sealed record RenameMove(string OldName, string NewName, bool IsDirectory);
