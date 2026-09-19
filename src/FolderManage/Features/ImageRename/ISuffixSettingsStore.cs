namespace FolderManage.Features.ImageRename;

/// <summary>접미사 목록의 저장소. ViewModel이 파일 시스템에 직접 의존하지 않도록 분리했다(테스트에서는 가짜 구현).</summary>
public interface ISuffixSettingsStore
{
    IReadOnlyList<string> Load();

    void Save(IReadOnlyList<string> suffixes);
}
