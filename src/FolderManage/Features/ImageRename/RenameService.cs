using System.IO;

namespace FolderManage.Features.ImageRename;

/// <summary>
/// 대상 폴더의 항목을 읽고 이름을 바꾸는 파일 시스템 서비스.
/// 이름 변경은 그룹 단위로 전부 성공하거나 전부 취소한다(doc/07-image-rename-spec.md 안전 규칙).
/// </summary>
public static class RenameService
{
    public static IReadOnlyList<DirectoryEntry> ReadEntries(string directoryPath)
    {
        return new DirectoryInfo(directoryPath)
            .EnumerateFileSystemInfos()
            .Select(info => new DirectoryEntry(info.Name, info is DirectoryInfo))
            .ToList();
    }

    /// <summary>
    /// 이름 변경을 실행한다. 시작 전에 원본이 있고 새 이름이 비어 있는지 다시 검사하고(하나라도 어긋나면 아무것도
    /// 바꾸지 않는다), 도중에 하나가 실패하면 이미 바꾼 항목을 원래대로 되돌린다.
    /// </summary>
    public static RenameResult ApplyMoves(string directoryPath, IReadOnlyList<RenameMove> moves)
    {
        foreach (var move in moves)
        {
            var source = Path.Combine(directoryPath, move.OldName);
            var sourceExists = move.IsDirectory ? Directory.Exists(source) : File.Exists(source);
            if (!sourceExists)
            {
                return RenameResult.Failure($"'{move.OldName}'을(를) 찾을 수 없습니다.");
            }

            var target = Path.Combine(directoryPath, move.NewName);
            var isSameEntry = string.Equals(move.OldName, move.NewName, StringComparison.OrdinalIgnoreCase);
            if (!isSameEntry && (File.Exists(target) || Directory.Exists(target)))
            {
                return RenameResult.Failure($"'{move.NewName}' 이름이 이미 있어 변경하지 않았습니다.");
            }
        }

        var done = new List<RenameMove>();
        foreach (var move in moves)
        {
            try
            {
                MoveEntry(directoryPath, move.OldName, move.NewName, move.IsDirectory);
                done.Add(move);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                var notRestored = Rollback(directoryPath, done);
                var message = $"'{move.OldName}' 이름 변경에 실패해 이 그룹의 변경을 모두 취소했습니다: {ex.Message}";
                if (notRestored.Count > 0)
                {
                    message += $" (원래 이름으로 되돌리지 못한 항목: {string.Join(", ", notRestored)})";
                }

                return RenameResult.Failure(message);
            }
        }

        return RenameResult.Success(done.Count);
    }

    /// <summary>기록된 변경을 반대로 실행한다(새 이름 → 원래 이름). 충돌 검사와 원복 규칙은 변경과 같다.</summary>
    public static RenameResult Undo(RenameRecord record)
    {
        var reversed = record.Moves
            .Reverse()
            .Select(m => new RenameMove(m.NewName, m.OldName, m.IsDirectory))
            .ToList();

        return ApplyMoves(record.DirectoryPath, reversed);
    }

    private static IReadOnlyList<string> Rollback(string directoryPath, List<RenameMove> done)
    {
        var notRestored = new List<string>();
        for (var i = done.Count - 1; i >= 0; i--)
        {
            var move = done[i];
            try
            {
                MoveEntry(directoryPath, move.NewName, move.OldName, move.IsDirectory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                notRestored.Add(move.NewName);
            }
        }

        return notRestored;
    }

    private static void MoveEntry(string directoryPath, string oldName, string newName, bool isDirectory)
    {
        if (string.Equals(oldName, newName, StringComparison.Ordinal))
        {
            return;
        }

        var source = Path.Combine(directoryPath, oldName);
        var target = Path.Combine(directoryPath, newName);

        // 대소문자만 다르면 Windows는 같은 이름으로 보므로 임시 이름을 거쳐 바꾼다.
        if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
        {
            var temp = Path.Combine(directoryPath, $".rename-{Guid.NewGuid():N}.tmp");
            Move(source, temp, isDirectory);
            try
            {
                Move(temp, target, isDirectory);
            }
            catch
            {
                Move(temp, source, isDirectory);
                throw;
            }

            return;
        }

        Move(source, target, isDirectory);
    }

    private static void Move(string source, string target, bool isDirectory)
    {
        if (isDirectory)
        {
            Directory.Move(source, target);
        }
        else
        {
            File.Move(source, target);
        }
    }
}
