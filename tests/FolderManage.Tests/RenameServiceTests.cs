using FolderManage.Features.ImageRename;

namespace FolderManage.Tests;

public class RenameServiceTests : IDisposable
{
    private readonly string _root;

    public RenameServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "FolderManageRenameTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private void MakeFolder(string name) => Directory.CreateDirectory(Path.Combine(_root, name));

    private void MakeFile(string name, string content = "x") => File.WriteAllText(Path.Combine(_root, name), content);

    private bool Exists(string name)
    {
        var path = Path.Combine(_root, name);
        return File.Exists(path) || Directory.Exists(path);
    }

    private string[] NamesInRoot() =>
        Directory.EnumerateFileSystemEntries(_root).Select(p => Path.GetFileName(p)!).OrderBy(n => n).ToArray();

    [Fact]
    public void ReadEntries_ReportsFoldersAndFiles()
    {
        MakeFolder("001");
        MakeFile("001.png");

        var entries = RenameService.ReadEntries(_root).OrderBy(e => e.Name).ToList();

        Assert.Equal(new[] { "001", "001.png" }, entries.Select(e => e.Name));
        Assert.True(entries[0].IsDirectory);
        Assert.False(entries[1].IsDirectory);
    }

    [Fact]
    public void ApplyMoves_RenamesFolderAndFilesAndKeepsContent()
    {
        MakeFolder("001");
        MakeFile("001.png", "image");
        MakeFile("001.debug.png", "debug");
        var moves = new[]
        {
            new RenameMove("001", "ch01", true),
            new RenameMove("001.png", "ch01.png", false),
            new RenameMove("001.debug.png", "ch01.debug.png", false),
        };

        var result = RenameService.ApplyMoves(_root, moves);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.MovedCount);
        Assert.Equal(new[] { "ch01", "ch01.debug.png", "ch01.png" }, NamesInRoot());
        Assert.Equal("image", File.ReadAllText(Path.Combine(_root, "ch01.png")));
    }

    [Fact]
    public void ApplyMoves_ChangesNothingWhenATargetNameAlreadyExists()
    {
        MakeFolder("001");
        MakeFile("001.png");
        MakeFile("ch01.png", "other");
        var moves = new[]
        {
            new RenameMove("001", "ch01", true),
            new RenameMove("001.png", "ch01.png", false),
        };

        var result = RenameService.ApplyMoves(_root, moves);

        Assert.False(result.IsSuccess);
        Assert.Contains("ch01.png", result.Message);
        Assert.Equal(new[] { "001", "001.png", "ch01.png" }, NamesInRoot());
        Assert.Equal("other", File.ReadAllText(Path.Combine(_root, "ch01.png")));
    }

    [Fact]
    public void ApplyMoves_ChangesNothingWhenASourceIsMissing()
    {
        MakeFolder("001");
        var moves = new[]
        {
            new RenameMove("001", "ch01", true),
            new RenameMove("001.png", "ch01.png", false),
        };

        var result = RenameService.ApplyMoves(_root, moves);

        Assert.False(result.IsSuccess);
        Assert.Equal(new[] { "001" }, NamesInRoot());
    }

    [Fact]
    public void ApplyMoves_RollsBackAlreadyRenamedItemsWhenALaterOneFails()
    {
        MakeFolder("001");
        MakeFile("001.png");
        var moves = new[]
        {
            new RenameMove("001", "ch01", true),
            new RenameMove("001.png", "ch01.png", false),
        };

        // 파일을 잠가 두 번째 항목의 이름 변경이 실패하게 한다.
        using (new FileStream(Path.Combine(_root, "001.png"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var result = RenameService.ApplyMoves(_root, moves);

            Assert.False(result.IsSuccess);
            Assert.Contains("취소", result.Message);
        }

        Assert.Equal(new[] { "001", "001.png" }, NamesInRoot());
        Assert.True(Directory.Exists(Path.Combine(_root, "001")));
    }

    [Fact]
    public void ApplyMoves_CaseOnlyChangeWorksThroughTemporaryName()
    {
        MakeFolder("abc");
        MakeFile("abc.png");
        var moves = new[]
        {
            new RenameMove("abc", "ABC", true),
            new RenameMove("abc.png", "ABC.png", false),
        };

        var result = RenameService.ApplyMoves(_root, moves);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "ABC", "ABC.png" }, NamesInRoot());
    }

    [Fact]
    public void Undo_RestoresOriginalNames()
    {
        MakeFolder("001");
        MakeFile("001.png");
        var moves = new[]
        {
            new RenameMove("001", "ch01", true),
            new RenameMove("001.png", "ch01.png", false),
        };
        Assert.True(RenameService.ApplyMoves(_root, moves).IsSuccess);
        var record = new RenameRecord(_root, "001", "ch01", moves);

        var result = RenameService.Undo(record);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "001", "001.png" }, NamesInRoot());
    }

    [Fact]
    public void Undo_IsRefusedWhenTheOriginalNameIsTakenAgain()
    {
        MakeFolder("001");
        MakeFile("001.png");
        var moves = new[]
        {
            new RenameMove("001", "ch01", true),
            new RenameMove("001.png", "ch01.png", false),
        };
        Assert.True(RenameService.ApplyMoves(_root, moves).IsSuccess);
        MakeFile("001.png", "newcomer");
        var record = new RenameRecord(_root, "001", "ch01", moves);

        var result = RenameService.Undo(record);

        Assert.False(result.IsSuccess);
        Assert.True(Exists("ch01"));
        Assert.True(Exists("ch01.png"));
        Assert.Equal("newcomer", File.ReadAllText(Path.Combine(_root, "001.png")));
    }

    [Fact]
    public void Undo_IsRefusedWhenARenamedItemWasChangedOutsideTheApp()
    {
        MakeFile("001.png");
        var moves = new[] { new RenameMove("001.png", "ch01.png", false) };
        Assert.True(RenameService.ApplyMoves(_root, moves).IsSuccess);
        File.Delete(Path.Combine(_root, "ch01.png"));

        var result = RenameService.Undo(new RenameRecord(_root, "001", "ch01", moves));

        Assert.False(result.IsSuccess);
    }
}
