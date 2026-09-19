namespace FolderManage.Features.MakeFolder;

public sealed record FolderCreationSummary(
    int CreatedCount,
    int AlreadyExistsCount,
    int ConflictCount,
    IReadOnlyList<FolderCreationFailure> Failures);
