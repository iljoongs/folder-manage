namespace FolderManage.Models;

public sealed class FolderSequenceInput
{
    public string ParentFolderPath { get; init; } = string.Empty;
    public string Prefix { get; init; } = string.Empty;
    public string Suffix { get; init; } = string.Empty;
    public string StartText { get; init; } = string.Empty;
    public string EndText { get; init; } = string.Empty;
    public string StepText { get; init; } = string.Empty;
    public string DigitCountText { get; init; } = string.Empty;
}
