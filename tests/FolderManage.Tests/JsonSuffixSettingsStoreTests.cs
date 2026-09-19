using FolderManage.Features.ImageRename;

namespace FolderManage.Tests;

public class JsonSuffixSettingsStoreTests : IDisposable
{
    private readonly string _root;

    public JsonSuffixSettingsStoreTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "FolderManageSettingsTests_" + Guid.NewGuid());
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string SettingsPath => Path.Combine(_root, "sub", "settings.json");

    [Fact]
    public void Load_ReturnsDefaultsWhenFileDoesNotExist()
    {
        var store = new JsonSuffixSettingsStore(SettingsPath);

        Assert.Equal(new[] { ".debug", ".debug-result" }, store.Load());
    }

    [Fact]
    public void SaveThenLoad_RoundTripsAndCreatesFolder()
    {
        var store = new JsonSuffixSettingsStore(SettingsPath);

        store.Save(new[] { ".raw", ".debug" });

        Assert.Equal(new[] { ".raw", ".debug" }, new JsonSuffixSettingsStore(SettingsPath).Load());
    }

    [Fact]
    public void SaveThenLoad_KeepsAnEmptyListEmpty()
    {
        var store = new JsonSuffixSettingsStore(SettingsPath);

        store.Save(Array.Empty<string>());

        Assert.Empty(store.Load());
    }

    [Fact]
    public void Load_ReturnsDefaultsWhenFileIsCorrupt()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, "{ not json");

        Assert.Equal(new[] { ".debug", ".debug-result" }, new JsonSuffixSettingsStore(SettingsPath).Load());
    }
}
