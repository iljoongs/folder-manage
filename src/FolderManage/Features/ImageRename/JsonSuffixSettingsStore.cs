using System.IO;
using System.Text.Json;

namespace FolderManage.Features.ImageRename;

/// <summary>접미사 목록을 JSON 파일(기본 %AppData%\folder-manage\settings.json)에 저장한다.</summary>
public sealed class JsonSuffixSettingsStore : ISuffixSettingsStore
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public JsonSuffixSettingsStore(string filePath)
    {
        _filePath = filePath;
    }

    public static string DefaultFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "folder-manage",
        "settings.json");

    /// <summary>
    /// 파일이 없거나 읽을 수 없으면(깨진 JSON 등) 기본 접미사 목록을 돌려준다.
    /// 목록 하나가 파일에 없으면(이전 버전이 저장한 파일 등) 그 목록만 기본값을 쓴다.
    /// </summary>
    public SuffixSettings Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return SuffixSettings.Default;
            }

            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_filePath));
            if (data is null)
            {
                return SuffixSettings.Default;
            }

            return new SuffixSettings(
                Clean(data.Suffixes) ?? SuffixRules.DefaultFileSuffixes,
                Clean(data.FolderSuffixes) ?? SuffixRules.DefaultFolderSuffixes);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return SuffixSettings.Default;
        }
    }

    public void Save(SuffixSettings settings)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var data = new SettingsData
        {
            Suffixes = settings.FileSuffixes.ToList(),
            FolderSuffixes = settings.FolderSuffixes.ToList(),
        };
        File.WriteAllText(_filePath, JsonSerializer.Serialize(data, WriteOptions));
    }

    private static List<string>? Clean(List<string>? suffixes)
    {
        return suffixes?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    private sealed class SettingsData
    {
        public List<string>? Suffixes { get; set; }

        public List<string>? FolderSuffixes { get; set; }
    }
}
