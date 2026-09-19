using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FolderManage.Common;

namespace FolderManage.Features.ImageRename;

/// <summary>탭 2(이미지 파일 이름 변경)의 ViewModel. 규칙은 doc/07-image-rename-spec.md 참고.</summary>
public partial class ImageRenameViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;
    private readonly ISuffixSettingsStore _suffixStore;

    // 되돌리기 기록. 앱을 실행하는 동안만 유지하고 가장 최근 변경부터 되돌린다.
    private readonly Stack<RenameRecord> _history = new();

    private RenamePlan? _currentPlan;

    // 그룹 목록을 읽어 온 폴더. 경로 입력란을 고쳐도 새로 읽기 전까지는 이 폴더를 기준으로 동작한다.
    private string _loadedFolderPath = string.Empty;

    public ImageRenameViewModel(IDialogService dialogService, ISuffixSettingsStore suffixStore)
    {
        _dialogService = dialogService;
        _suffixStore = suffixStore;

        foreach (var suffix in _suffixStore.Load())
        {
            Suffixes.Add(suffix);
        }
    }

    [ObservableProperty]
    private string _targetFolderPath = string.Empty;

    [ObservableProperty]
    private NameGroupRowViewModel? _selectedGroup;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newSuffixText = string.Empty;

    [ObservableProperty]
    private string? _selectedSuffix;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
    private bool _canRename;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    private bool _canUndo;

    public ObservableCollection<NameGroupRowViewModel> Groups { get; } = new();

    public ObservableCollection<RenamePreviewRowViewModel> PreviewItems { get; } = new();

    public ObservableCollection<string> Suffixes { get; } = new();

    partial void OnTargetFolderPathChanged(string value) => InvalidatePreview();

    partial void OnNewNameChanged(string value) => InvalidatePreview();

    partial void OnSelectedGroupChanged(NameGroupRowViewModel? value)
    {
        // 그룹을 고르면 현재 공통 이름을 채워 두어 고쳐 쓸 수 있게 한다.
        NewName = value?.CommonName ?? string.Empty;
        InvalidatePreview();
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var selected = _dialogService.SelectFolder(TargetFolderPath);
        if (selected is not null)
        {
            TargetFolderPath = selected;
            LoadGroups(selected, null);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadGroups(TargetFolderPath, SelectedGroup?.CommonName);
    }

    [RelayCommand]
    private void Preview()
    {
        InvalidatePreview();

        if (SelectedGroup is null)
        {
            StatusMessage = "이름을 바꿀 그룹을 목록에서 선택하세요.";
            return;
        }

        if (!TryReadEntries(_loadedFolderPath, out var entries))
        {
            return;
        }

        var plan = RenamePlanner.BuildPlan(SelectedGroup.Group, NewName, entries.Select(e => e.Name));

        foreach (var item in plan.Items)
        {
            PreviewItems.Add(new RenamePreviewRowViewModel(item));
        }

        if (!plan.CanExecute)
        {
            StatusMessage = plan.ErrorMessage ?? "변경할 항목이 없습니다.";
            return;
        }

        _currentPlan = plan;
        CanRename = true;
        StatusMessage = $"미리보기: {plan.Items.Count}개 항목의 이름을 '{plan.Group.CommonName}' → '{plan.NewCommonName}'(으)로 바꿉니다. 충돌이 없습니다.";
    }

    [RelayCommand(CanExecute = nameof(CanRename))]
    private void Rename()
    {
        var plan = _currentPlan;
        if (plan is null)
        {
            return;
        }

        var moves = plan.ToMoves();
        var result = RenameService.ApplyMoves(_loadedFolderPath, moves);
        if (!result.IsSuccess)
        {
            _currentPlan = null;
            CanRename = false;
            StatusMessage = $"{result.Message} 목록을 새로고침한 뒤 다시 미리보기 하세요.";
            return;
        }

        _history.Push(new RenameRecord(_loadedFolderPath, plan.Group.CommonName, plan.NewCommonName, moves));
        CanUndo = true;

        // 바뀐 결과를 보여주기 위해 목록을 새로 읽고 새 이름의 그룹을 선택한다.
        LoadGroups(_loadedFolderPath, plan.NewCommonName);
        StatusMessage = $"변경 완료: {result.MovedCount}개 항목 '{plan.Group.CommonName}' → '{plan.NewCommonName}'. '되돌리기'로 취소할 수 있습니다.";
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (!_history.TryPeek(out var record))
        {
            return;
        }

        var result = RenameService.Undo(record);
        if (!result.IsSuccess)
        {
            StatusMessage = $"되돌리기 실패: {result.Message}";
            return;
        }

        _history.Pop();
        CanUndo = _history.Count > 0;

        if (IsSamePath(record.DirectoryPath, _loadedFolderPath))
        {
            LoadGroups(_loadedFolderPath, record.OldCommonName);
        }

        StatusMessage = $"되돌리기 완료: {result.MovedCount}개 항목 '{record.NewCommonName}' → '{record.OldCommonName}'.";
    }

    [RelayCommand]
    private void AddSuffix()
    {
        var suffix = NewSuffixText.Trim();
        var error = SuffixRules.Validate(suffix, Suffixes);
        if (error is not null)
        {
            StatusMessage = error;
            return;
        }

        Suffixes.Add(suffix);
        NewSuffixText = string.Empty;
        ApplySuffixChange($"접미사 '{suffix}'을(를) 추가했습니다.");
    }

    [RelayCommand]
    private void RemoveSuffix()
    {
        if (SelectedSuffix is null)
        {
            StatusMessage = "삭제할 접미사를 목록에서 선택하세요.";
            return;
        }

        var suffix = SelectedSuffix;
        Suffixes.Remove(suffix);
        ApplySuffixChange($"접미사 '{suffix}'을(를) 삭제했습니다.");
    }

    // 접미사 목록이 바뀌면 저장하고, 이미 읽은 폴더가 있으면 그룹을 다시 나눈다.
    private void ApplySuffixChange(string message)
    {
        string? saveError = null;
        try
        {
            _suffixStore.Save(Suffixes.ToList());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            saveError = ex.Message;
        }

        if (_loadedFolderPath.Length > 0 && Directory.Exists(_loadedFolderPath))
        {
            LoadGroups(_loadedFolderPath, SelectedGroup?.CommonName);
        }

        StatusMessage = saveError is null ? message : $"{message} (설정을 저장하지 못했습니다: {saveError})";
    }

    private void LoadGroups(string folderPath, string? selectCommonName)
    {
        InvalidatePreview();

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Groups.Clear();
            StatusMessage = "대상 폴더를 선택하세요.";
            return;
        }

        if (!TryReadEntries(folderPath, out var entries))
        {
            Groups.Clear();
            return;
        }

        var groups = NameGroupBuilder.Build(entries, Suffixes.ToList());
        _loadedFolderPath = folderPath;

        Groups.Clear();
        foreach (var group in groups)
        {
            Groups.Add(new NameGroupRowViewModel(group));
        }

        SelectedGroup = selectCommonName is null
            ? null
            : Groups.FirstOrDefault(g => string.Equals(g.CommonName, selectCommonName, StringComparison.OrdinalIgnoreCase));

        StatusMessage = groups.Count == 0
            ? "대상 폴더에 폴더나 파일이 없습니다."
            : $"이름 그룹 {groups.Count}개를 읽었습니다. 이름을 바꿀 그룹을 선택하세요.";
    }

    private bool TryReadEntries(string folderPath, out IReadOnlyList<DirectoryEntry> entries)
    {
        entries = Array.Empty<DirectoryEntry>();

        if (!Directory.Exists(folderPath))
        {
            StatusMessage = "대상 폴더를 찾을 수 없습니다.";
            return false;
        }

        try
        {
            entries = RenameService.ReadEntries(folderPath);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusMessage = $"대상 폴더를 읽을 수 없습니다: {ex.Message}";
            return false;
        }
    }

    private void InvalidatePreview()
    {
        _currentPlan = null;
        PreviewItems.Clear();
        CanRename = false;
    }

    private static bool IsSamePath(string a, string b)
    {
        return string.Equals(a.TrimEnd('\\', '/'), b.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
    }
}
