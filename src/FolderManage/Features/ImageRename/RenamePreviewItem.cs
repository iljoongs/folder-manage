namespace FolderManage.Features.ImageRename;

public sealed record RenamePreviewItem(string OldName, string NewName, bool IsDirectory, bool HasConflict);
