using FolderManage.Models;
using FolderManage.Services;

namespace FolderManage.Tests;

public class FolderCreationServiceTests : IDisposable
{
    private readonly string _tempRoot;

    public FolderCreationServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "FolderManageTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public void GetSubfolderNames_ReturnsOnlyFolderNames()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "1화"));
        Directory.CreateDirectory(Path.Combine(_tempRoot, "2화"));
        File.WriteAllText(Path.Combine(_tempRoot, "3화"), "not a folder");

        var names = FolderCreationService.GetSubfolderNames(_tempRoot);

        Assert.Equal(new[] { "1화", "2화" }, names.OrderBy(n => n).ToArray());
    }

    [Fact]
    public void GetStatus_FolderDoesNotExist_ReturnsNew()
    {
        var status = FolderCreationService.GetStatus(_tempRoot, "01");

        Assert.Equal(FolderItemStatus.New, status.Status);
    }

    [Fact]
    public void GetStatus_FolderAlreadyExists_ReturnsAlreadyExists()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "01"));

        var status = FolderCreationService.GetStatus(_tempRoot, "01");

        Assert.Equal(FolderItemStatus.AlreadyExists, status.Status);
    }

    [Fact]
    public void GetStatus_FileWithSameNameExists_ReturnsConflict()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "01"), "not a folder");

        var status = FolderCreationService.GetStatus(_tempRoot, "01");

        Assert.Equal(FolderItemStatus.Conflict, status.Status);
    }

    [Fact]
    public void CreateAll_CreatesOnlyNewItems_AndCountsEachStatus()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "02"));
        File.WriteAllText(Path.Combine(_tempRoot, "03"), "not a folder");

        var items = new List<FolderPreviewItem>
        {
            new("01", FolderItemStatus.New),
            new("02", FolderItemStatus.AlreadyExists),
            new("03", FolderItemStatus.Conflict),
        };

        var summary = FolderCreationService.CreateAll(_tempRoot, items);

        Assert.Equal(1, summary.CreatedCount);
        Assert.Equal(1, summary.AlreadyExistsCount);
        Assert.Equal(1, summary.ConflictCount);
        Assert.Empty(summary.Failures);
        Assert.True(Directory.Exists(Path.Combine(_tempRoot, "01")));
    }

    [Fact]
    public void BuildPreview_ClassifiesEachNameByCurrentFileSystemState()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "existing"));
        File.WriteAllText(Path.Combine(_tempRoot, "conflict"), "not a folder");

        var preview = FolderCreationService.BuildPreview(_tempRoot, new[] { "new", "existing", "conflict" });

        Assert.Equal(FolderItemStatus.New, preview[0].Status);
        Assert.Equal(FolderItemStatus.AlreadyExists, preview[1].Status);
        Assert.Equal(FolderItemStatus.Conflict, preview[2].Status);
    }
}
