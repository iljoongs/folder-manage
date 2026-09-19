namespace FolderManage.Features.MakeFolder;

public sealed record ParsedFolderSequence(
    string ParentFolderPath,
    string Prefix,
    string Suffix,
    int Start,
    int End,
    int Step,
    int DigitCount,
    long Count);
