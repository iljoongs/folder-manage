using MakeFolder.Services;
using MakeFolder.ViewModels;

namespace MakeFolder.Tests;

public class MainViewModelTests : IDisposable
{
    private readonly string _tempRoot;

    public MainViewModelTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "MakeFolderVmTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private sealed class FakeDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;

        public int ConfirmCallCount { get; private set; }

        public string? SelectFolder(string? initialDirectory) => null;

        public bool Confirm(string message, string title)
        {
            ConfirmCallCount++;
            return ConfirmResult;
        }

        public void ShowError(string message, string title)
        {
        }
    }

    private MainViewModel CreateViewModel(FakeDialogService? dialog = null)
    {
        return new MainViewModel(dialog ?? new FakeDialogService())
        {
            ParentFolderPath = _tempRoot,
        };
    }

    private void MakeFolders(params string[] names)
    {
        foreach (var name in names)
        {
            Directory.CreateDirectory(Path.Combine(_tempRoot, name));
        }
    }

    private string[] ExistingFolderNames() =>
        Directory.GetDirectories(_tempRoot).Select(p => Path.GetFileName(p)!).ToArray();

    [Fact]
    public void Defaults_DigitCountIsOne_AndSuffixIsHwa()
    {
        var viewModel = new MainViewModel(new FakeDialogService());

        Assert.Equal("1", viewModel.DigitCountText);
        Assert.Equal("화", viewModel.Suffix);
    }

    [Fact]
    public void AutoCreate_FillsMissingNumbersBetweenSmallestAndLargest()
    {
        MakeFolders("1화", "2화", "20화");
        var viewModel = CreateViewModel();

        viewModel.AutoCreateCommand.Execute(null);

        var expected = Enumerable.Range(1, 20).Select(n => $"{n}화").OrderBy(n => n).ToArray();
        Assert.Equal(expected, ExistingFolderNames().OrderBy(n => n).ToArray());
        Assert.Equal(20, viewModel.PreviewItems.Count);
        Assert.All(viewModel.PreviewItems, row => Assert.Equal("이미 존재", row.StatusText));
        Assert.Contains("생성 17개", viewModel.StatusMessage);
        Assert.Contains("이미 존재 3개", viewModel.StatusMessage);
        Assert.False(viewModel.CanCreate);
    }

    [Fact]
    public void AutoCreate_UsesDigitCountForNewNames_AndIgnoresPaddingWhenDetecting()
    {
        MakeFolders("01화", "04화");
        var viewModel = CreateViewModel();
        viewModel.DigitCountText = "2";

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal(new[] { "01화", "02화", "03화", "04화" }, ExistingFolderNames().OrderBy(n => n).ToArray());
    }

    [Fact]
    public void AutoCreate_RespectsStep()
    {
        MakeFolders("1화", "7화");
        var viewModel = CreateViewModel();
        viewModel.StepText = "3";

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal(new[] { "1화", "4화", "7화" }, ExistingFolderNames().OrderBy(n => n).ToArray());
    }

    [Fact]
    public void AutoCreate_NothingMissing_CreatesNothingAndSaysSo()
    {
        MakeFolders("1화", "2화", "3화");
        var viewModel = CreateViewModel();

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal(3, ExistingFolderNames().Length);
        Assert.Contains("새로 만들 폴더가 없습니다", viewModel.StatusMessage);
    }

    [Fact]
    public void AutoCreate_NoMatchingFolders_ReportsAndCreatesNothing()
    {
        MakeFolders("abc", "10회");
        var viewModel = CreateViewModel();

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal(2, ExistingFolderNames().Length);
        Assert.Contains("형식의 폴더가 없습니다", viewModel.StatusMessage);
    }

    [Fact]
    public void AutoCreate_FileInTheGap_IsReportedAsConflictAndNotOverwritten()
    {
        MakeFolders("1화", "3화");
        File.WriteAllText(Path.Combine(_tempRoot, "2화"), "not a folder");
        var viewModel = CreateViewModel();

        viewModel.AutoCreateCommand.Execute(null);

        Assert.True(File.Exists(Path.Combine(_tempRoot, "2화")));
        Assert.Contains("충돌 1개", viewModel.StatusMessage);
    }

    [Fact]
    public void AutoCreate_UsesCurrentPrefixAndSuffix()
    {
        MakeFolders("Ch1권", "Ch3권", "1화");
        var viewModel = CreateViewModel();
        viewModel.Prefix = "Ch";
        viewModel.Suffix = "권";

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal(new[] { "1화", "Ch1권", "Ch2권", "Ch3권" }, ExistingFolderNames().OrderBy(n => n).ToArray());
    }

    [Fact]
    public void AutoCreate_LargeRange_AsksForConfirmation_AndCancelCreatesNothing()
    {
        MakeFolders("1화", "2000화");
        var dialog = new FakeDialogService { ConfirmResult = false };
        var viewModel = CreateViewModel(dialog);

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal(1, dialog.ConfirmCallCount);
        Assert.Equal(2, ExistingFolderNames().Length);
    }

    [Fact]
    public void AutoCreate_LargeRange_ProceedsWhenConfirmed()
    {
        MakeFolders("1화", "1001화");
        var dialog = new FakeDialogService { ConfirmResult = true };
        var viewModel = CreateViewModel(dialog);

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal(1, dialog.ConfirmCallCount);
        Assert.Equal(1001, ExistingFolderNames().Length);
    }

    [Fact]
    public void AutoCreate_NoParentFolder_ShowsError()
    {
        var viewModel = new MainViewModel(new FakeDialogService());

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal("상위 폴더를 선택하세요.", viewModel.StatusMessage);
    }

    [Fact]
    public void AutoCreate_ParentFolderDoesNotExist_ShowsError()
    {
        var viewModel = new MainViewModel(new FakeDialogService())
        {
            ParentFolderPath = Path.Combine(_tempRoot, "missing"),
        };

        viewModel.AutoCreateCommand.Execute(null);

        Assert.Equal("상위 폴더를 찾을 수 없습니다.", viewModel.StatusMessage);
    }
}
