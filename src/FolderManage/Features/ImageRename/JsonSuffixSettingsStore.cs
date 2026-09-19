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

    /// <summary>파일이 없거나 읽을 수 없으면(깨진 JSON 등) 기본 접미사 목록을 돌려준다.</summary>
    public IReadOnlyList<string> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return SuffixRules.DefaultSuffixes;
            }

            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_filePath));
            return data?.Suffixes is null
                ? SuffixRules.DefaultSuffixes
                : data.Suffixes.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return SuffixRules.DefaultSuffixes;
        }
    }

    public void Save(IReadOnlyList<string> suffixes)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_filePath, JsonSerializer.Serialize(new SettingsData { Suffixes = suffixes.ToList() }, WriteOptions));
    }

    private sealed class SettingsData
    {
        public List<string>? Suffixes { get; set; }
    }
}
