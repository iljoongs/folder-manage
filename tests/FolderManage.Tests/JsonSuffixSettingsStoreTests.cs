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
        var settings = new JsonSuffixSettingsStore(SettingsPath).Load();

        Assert.Equal(new[] { ".debug", ".debug-result" }, settings.FileSuffixes);
        Assert.Equal(new[] { "_files" }, settings.FolderSuffixes);
    }

    [Fact]
    public void SaveThenLoad_RoundTripsBothListsAndCreatesFolder()
    {
        var store = new JsonSuffixSettingsStore(SettingsPath);

        store.Save(new SuffixSettings(new[] { ".raw", ".debug" }, new[] { "_files", "-data" }));

        var loaded = new JsonSuffixSettingsStore(SettingsPath).Load();
        Assert.Equal(new[] { ".raw", ".debug" }, loaded.FileSuffixes);
        Assert.Equal(new[] { "_files", "-data" }, loaded.FolderSuffixes);
    }

    [Fact]
    public void SaveThenLoad_KeepsEmptyListsEmpty()
    {
        var store = new JsonSuffixSettingsStore(SettingsPath);

        store.Save(new SuffixSettings(Array.Empty<string>(), Array.Empty<string>()));

        var loaded = store.Load();
        Assert.Empty(loaded.FileSuffixes);
        Assert.Empty(loaded.FolderSuffixes);
    }

    [Fact]
    public void Load_UsesDefaultFolderSuffixesWhenFileHasOnlyFileSuffixes()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, "{ \"Suffixes\": [\".raw\"] }");

        var loaded = new JsonSuffixSettingsStore(SettingsPath).Load();

        Assert.Equal(new[] { ".raw" }, loaded.FileSuffixes);
        Assert.Equal(new[] { "_files" }, loaded.FolderSuffixes);
    }

    [Fact]
    public void Load_ReturnsDefaultsWhenFileIsCorrupt()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, "{ not json");

        var loaded = new JsonSuffixSettingsStore(SettingsPath).Load();

        Assert.Equal(new[] { ".debug", ".debug-result" }, loaded.FileSuffixes);
        Assert.Equal(new[] { "_files" }, loaded.FolderSuffixes);
    }
}
