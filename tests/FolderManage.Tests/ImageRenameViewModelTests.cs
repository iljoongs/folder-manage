using FolderManage.Features.ImageRename;

namespace FolderManage.Tests;

public class ImageRenameViewModelTests : IDisposable
{
    private readonly string _root;

    public ImageRenameViewModelTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "FolderManageRenameVmTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class FakeSuffixStore : ISuffixSettingsStore
    {
        public SuffixSettings Saved { get; private set; } = SuffixSettings.Default;

        public int SaveCount { get; private set; }

        public SuffixSettings Load() => Saved;

        public void Save(SuffixSettings settings)
        {
            Saved = settings;
            SaveCount++;
        }
    }

    private void MakeFolder(string name) => Directory.CreateDirectory(Path.Combine(_root, name));

    private void MakeFile(string name) => File.WriteAllText(Path.Combine(_root, name), "x");

    private string[] NamesInRoot() =>
        Directory.EnumerateFileSystemEntries(_root).Select(p => Path.GetFileName(p)!).OrderBy(n => n, StringComparer.Ordinal).ToArray();

    private ImageRenameViewModel CreateViewModel(FakeSuffixStore? store = null)
    {
        return new ImageRenameViewModel(new FakeDialogService(), store ?? new FakeSuffixStore());
    }

    private ImageRenameViewModel CreateLoadedViewModel(FakeSuffixStore? store = null)
    {
        var viewModel = CreateViewModel(store);
        viewModel.TargetFolderPath = _root;
        viewModel.RefreshCommand.Execute(null);
        return viewModel;
    }

    private void MakeStandardGroup001()
    {
        MakeFolder("001");
        MakeFile("001.png");
        MakeFile("001.debug.png");
        MakeFile("001.debug-result.png");
    }

    [Fact]
    public void Constructor_LoadsSuffixesFromStore()
    {
        var viewModel = CreateViewModel();

        Assert.Equal(new[] { ".debug", ".debug-result" }, viewModel.Suffixes);
        Assert.Equal(new[] { "_files" }, viewModel.FolderSuffixes);
    }

    [Fact]
    public void Refresh_BuildsGroupsFromTheFolder()
    {
        MakeStandardGroup001();
        MakeFolder("002");
        MakeFile("002.png");

        var viewModel = CreateLoadedViewModel();

        Assert.Equal(new[] { "001", "002" }, viewModel.Groups.Select(g => g.CommonName));
        Assert.Null(viewModel.SelectedGroup);
    }

    [Fact]
    public void Refresh_ReportsMissingFolder()
    {
        var viewModel = CreateViewModel();
        viewModel.TargetFolderPath = Path.Combine(_root, "nope");

        viewModel.RefreshCommand.Execute(null);

        Assert.Empty(viewModel.Groups);
        Assert.Contains("찾을 수 없습니다", viewModel.StatusMessage);
    }

    [Fact]
    public void BrowseFolder_LoadsGroupsOfTheSelectedFolder()
    {
        MakeFile("001.png");
        var dialog = new FakeDialogService { SelectedFolderToReturn = _root };
        var viewModel = new ImageRenameViewModel(dialog, new FakeSuffixStore());

        viewModel.BrowseFolderCommand.Execute(null);

        Assert.Equal(_root, viewModel.TargetFolderPath);
        Assert.Single(viewModel.Groups);
    }

    [Fact]
    public void SelectingAGroup_PrefillsNewNameWithItsCommonName()
    {
        MakeFile("001.png");
        var viewModel = CreateLoadedViewModel();

        viewModel.SelectedGroup = viewModel.Groups[0];

        Assert.Equal("001", viewModel.NewName);
    }

    [Fact]
    public void Preview_WithoutASelectedGroupAsksToSelectOne()
    {
        MakeFile("001.png");
        var viewModel = CreateLoadedViewModel();

        viewModel.PreviewCommand.Execute(null);

        Assert.False(viewModel.CanRename);
        Assert.Contains("선택", viewModel.StatusMessage);
    }

    [Fact]
    public void Preview_ShowsBeforeAfterAndEnablesRename()
    {
        MakeStandardGroup001();
        var viewModel = CreateLoadedViewModel();
        viewModel.SelectedGroup = viewModel.Groups[0];
        viewModel.NewName = "ch01";

        viewModel.PreviewCommand.Execute(null);

        Assert.True(viewModel.CanRename);
        Assert.Equal(4, viewModel.PreviewItems.Count);
        Assert.Contains(viewModel.PreviewItems, p => p.OldName == "001.debug-result.png" && p.NewName == "ch01.debug-result.png");
        Assert.Equal(new[] { "001", "001.debug-result.png", "001.debug.png", "001.png" }, NamesInRoot());
    }

    [Fact]
    public void Preview_ExistingNameBlocksRenameAndChangesNothing()
    {
        MakeStandardGroup001();
        MakeFile("ch01.png");
        var viewModel = CreateLoadedViewModel();
        viewModel.SelectedGroup = viewModel.Groups.Single(g => g.CommonName == "001");
        viewModel.NewName = "ch01";

        viewModel.PreviewCommand.Execute(null);

        Assert.False(viewModel.CanRename);
        Assert.False(viewModel.RenameCommand.CanExecute(null));
        Assert.Contains("이미 같은 이름", viewModel.StatusMessage);
        Assert.Contains(viewModel.PreviewItems, p => p.NewName == "ch01.png" && p.StatusText == "이미 존재");
        Assert.Contains("001.png", NamesInRoot());
    }

    [Fact]
    public void EditingTheNewNameAfterPreviewDisablesRename()
    {
        MakeFile("001.png");
        var viewModel = CreateLoadedViewModel();
        viewModel.SelectedGroup = viewModel.Groups[0];
        viewModel.NewName = "ch01";
        viewModel.PreviewCommand.Execute(null);
        Assert.True(viewModel.CanRename);

        viewModel.NewName = "ch02";

        Assert.False(viewModel.CanRename);
        Assert.Empty(viewModel.PreviewItems);
    }

    [Fact]
    public void Rename_RenamesGroupRefreshesListAndEnablesUndo()
    {
        MakeStandardGroup001();
        MakeFolder("002");
        var viewModel = CreateLoadedViewModel();
        viewModel.SelectedGroup = viewModel.Groups.Single(g => g.CommonName == "001");
        viewModel.NewName = "ch01";
        viewModel.PreviewCommand.Execute(null);

        viewModel.RenameCommand.Execute(null);

        Assert.Equal(new[] { "002", "ch01", "ch01.debug-result.png", "ch01.debug.png", "ch01.png" }, NamesInRoot());
        Assert.Equal(new[] { "002", "ch01" }, viewModel.Groups.Select(g => g.CommonName));
        Assert.Equal("ch01", viewModel.SelectedGroup?.CommonName);
        Assert.True(viewModel.CanUndo);
        Assert.False(viewModel.CanRename);
        Assert.Contains("변경 완료", viewModel.StatusMessage);
    }

    [Fact]
    public void Undo_RestoresTheLastRenameAndCanBeRepeatedNewestFirst()
    {
        MakeFile("001.png");
        MakeFile("002.png");
        var viewModel = CreateLoadedViewModel();
        RenameGroup(viewModel, "001", "a");
        RenameGroup(viewModel, "002", "b");
        Assert.Equal(new[] { "a.png", "b.png" }, NamesInRoot());

        viewModel.UndoCommand.Execute(null);
        Assert.Equal(new[] { "002.png", "a.png" }, NamesInRoot());
        Assert.True(viewModel.CanUndo);

        viewModel.UndoCommand.Execute(null);
        Assert.Equal(new[] { "001.png", "002.png" }, NamesInRoot());
        Assert.False(viewModel.CanUndo);
        Assert.False(viewModel.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void Undo_FailureKeepsTheRecordAndReportsWhy()
    {
        MakeFile("001.png");
        var viewModel = CreateLoadedViewModel();
        RenameGroup(viewModel, "001", "a");
        MakeFile("001.png"); // 원래 이름을 다른 파일이 차지했다.

        viewModel.UndoCommand.Execute(null);

        Assert.True(viewModel.CanUndo);
        Assert.Contains("되돌리기 실패", viewModel.StatusMessage);
        Assert.Equal(new[] { "001.png", "a.png" }, NamesInRoot());
    }

    [Fact]
    public void Rename_FailureBecauseFolderChangedAfterPreviewReportsAndKeepsFiles()
    {
        MakeFile("001.png");
        var viewModel = CreateLoadedViewModel();
        viewModel.SelectedGroup = viewModel.Groups[0];
        viewModel.NewName = "ch01";
        viewModel.PreviewCommand.Execute(null);
        MakeFile("ch01.png"); // 미리보기 뒤에 같은 이름이 생겼다.

        viewModel.RenameCommand.Execute(null);

        Assert.Equal(new[] { "001.png", "ch01.png" }, NamesInRoot());
        Assert.False(viewModel.CanUndo);
        Assert.Contains("이미 있어", viewModel.StatusMessage);
    }

    [Fact]
    public void AddSuffix_SavesAndRegroupsTheLoadedFolder()
    {
        MakeFile("001.png");
        MakeFile("001.raw.png");
        var store = new FakeSuffixStore();
        var viewModel = CreateLoadedViewModel(store);
        Assert.Equal(2, viewModel.Groups.Count);

        viewModel.NewSuffixText = " .raw ";
        viewModel.AddSuffixCommand.Execute(null);

        Assert.Contains(".raw", viewModel.Suffixes);
        Assert.Contains(".raw", store.Saved.FileSuffixes);
        Assert.Single(viewModel.Groups);
        Assert.Equal(string.Empty, viewModel.NewSuffixText);
    }

    [Fact]
    public void AddSuffix_RejectsInvalidOrDuplicateSuffixWithoutSaving()
    {
        var store = new FakeSuffixStore();
        var viewModel = CreateViewModel(store);

        viewModel.NewSuffixText = "raw";
        viewModel.AddSuffixCommand.Execute(null);
        Assert.Contains("'.'", viewModel.StatusMessage);

        viewModel.NewSuffixText = ".DEBUG";
        viewModel.AddSuffixCommand.Execute(null);
        Assert.Contains("이미 있는", viewModel.StatusMessage);

        Assert.Equal(0, store.SaveCount);
        Assert.Equal(2, viewModel.Suffixes.Count);
    }

    [Fact]
    public void RemoveSuffix_SavesAndRegroups()
    {
        MakeFile("001.png");
        MakeFile("001.debug.png");
        var store = new FakeSuffixStore();
        var viewModel = CreateLoadedViewModel(store);
        Assert.Single(viewModel.Groups);

        viewModel.SelectedSuffix = ".debug";
        viewModel.RemoveSuffixCommand.Execute(null);

        Assert.DoesNotContain(".debug", viewModel.Suffixes);
        Assert.DoesNotContain(".debug", store.Saved.FileSuffixes);
        Assert.Equal(2, viewModel.Groups.Count);
    }

    [Fact]
    public void RemoveSuffix_WithoutSelectionAsksToSelect()
    {
        var viewModel = CreateViewModel();

        viewModel.RemoveSuffixCommand.Execute(null);

        Assert.Contains("선택", viewModel.StatusMessage);
        Assert.Equal(2, viewModel.Suffixes.Count);
    }

    [Fact]
    public void Refresh_GroupsFilesFolderWithItsFile()
    {
        MakeFolder("001_files");
        MakeFile("001.html");
        MakeFolder("002_files");
        MakeFile("002.html");

        var viewModel = CreateLoadedViewModel();

        Assert.Equal(new[] { "001", "002" }, viewModel.Groups.Select(g => g.CommonName));
        Assert.Equal("폴더 1개, 파일 1개", viewModel.Groups[0].CountText);
    }

    [Fact]
    public void Rename_KeepsFilesSuffixOnTheFolderAndUndoRestoresIt()
    {
        MakeFolder("001_files");
        MakeFile("001.html");
        var viewModel = CreateLoadedViewModel();

        RenameGroup(viewModel, "001", "page");

        Assert.Equal(new[] { "page.html", "page_files" }, NamesInRoot());

        viewModel.UndoCommand.Execute(null);

        Assert.Equal(new[] { "001.html", "001_files" }, NamesInRoot());
    }

    [Fact]
    public void Refresh_ListsGroupsInNaturalNumberOrder()
    {
        foreach (var name in new[] { "1.png", "10.png", "2.png", "20.png", "3.png" })
        {
            MakeFile(name);
        }

        var viewModel = CreateLoadedViewModel();

        Assert.Equal(new[] { "1", "2", "3", "10", "20" }, viewModel.Groups.Select(g => g.CommonName));
    }

    [Fact]
    public void AddFolderSuffix_SavesRegroupsAndRejectsDuplicate()
    {
        MakeFolder("001-data");
        MakeFile("001.html");
        var store = new FakeSuffixStore();
        var viewModel = CreateLoadedViewModel(store);
        Assert.Equal(2, viewModel.Groups.Count);

        viewModel.NewFolderSuffixText = " -data ";
        viewModel.AddFolderSuffixCommand.Execute(null);

        Assert.Contains("-data", viewModel.FolderSuffixes);
        Assert.Contains("-data", store.Saved.FolderSuffixes);
        Assert.Single(viewModel.Groups);

        viewModel.NewFolderSuffixText = "-DATA";
        viewModel.AddFolderSuffixCommand.Execute(null);

        Assert.Contains("이미 있는", viewModel.StatusMessage);
        Assert.Equal(1, store.SaveCount);
    }

    [Fact]
    public void RemoveFolderSuffix_SavesAndRegroups()
    {
        MakeFolder("001_files");
        MakeFile("001.html");
        var store = new FakeSuffixStore();
        var viewModel = CreateLoadedViewModel(store);
        Assert.Single(viewModel.Groups);

        viewModel.SelectedFolderSuffix = "_files";
        viewModel.RemoveFolderSuffixCommand.Execute(null);

        Assert.Empty(viewModel.FolderSuffixes);
        Assert.Empty(store.Saved.FolderSuffixes);
        Assert.Equal(2, viewModel.Groups.Count);
    }

    private static void RenameGroup(ImageRenameViewModel viewModel, string commonName, string newName)
    {
        viewModel.SelectedGroup = viewModel.Groups.Single(g => g.CommonName == commonName);
        viewModel.NewName = newName;
        viewModel.PreviewCommand.Execute(null);
        viewModel.RenameCommand.Execute(null);
    }
}
