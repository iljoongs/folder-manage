namespace FolderManage.Features.ImageRename;

public sealed record RenameResult(bool IsSuccess, string Message, int MovedCount)
{
    public static RenameResult Success(int movedCount) => new(true, string.Empty, movedCount);

    public static RenameResult Failure(string message) => new(false, message, 0);
}
