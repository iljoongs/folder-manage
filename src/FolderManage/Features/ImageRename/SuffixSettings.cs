namespace FolderManage.Features.ImageRename;

/// <summary>
/// 공통 이름을 판단할 때 떼어내는 접미사 목록. 파일은 확장자 앞의 접미사(<c>.debug</c>),
/// 폴더는 이름 끝의 접미사(<c>_files</c>)를 따로 갖는다.
/// </summary>
public sealed record SuffixSettings(IReadOnlyList<string> FileSuffixes, IReadOnlyList<string> FolderSuffixes)
{
    public static SuffixSettings Default => new(SuffixRules.DefaultFileSuffixes, SuffixRules.DefaultFolderSuffixes);
}
