namespace FolderManage.Features.MakeFolder;

public sealed class FolderSequenceValidationResult
{
    private FolderSequenceValidationResult(bool isSuccess, ParsedFolderSequence? parsed, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Parsed = parsed;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public ParsedFolderSequence? Parsed { get; }
    public string? ErrorMessage { get; }

    public static FolderSequenceValidationResult Success(ParsedFolderSequence parsed) =>
        new(true, parsed, null);

    public static FolderSequenceValidationResult Failure(string errorMessage) =>
        new(false, null, errorMessage);
}
