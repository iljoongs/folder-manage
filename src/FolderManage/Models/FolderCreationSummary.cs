namespace FolderManage.Models;

public sealed record FolderCreationSummary(
    int CreatedCount,
    int AlreadyExistsCount,
    int ConflictCount,
    IReadOnlyList<FolderCreationFailure> Failures);
