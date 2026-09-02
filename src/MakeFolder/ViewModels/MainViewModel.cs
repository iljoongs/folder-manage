using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MakeFolder.Models;
using MakeFolder.Services;

namespace MakeFolder.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const int LargeBatchThreshold = 1000;

    private readonly IDialogService _dialogService;

    private IReadOnlyList<FolderPreviewItem> _currentItems = Array.Empty<FolderPreviewItem>();
    private string _currentParentFolderPath = string.Empty;

    public MainViewModel()
        : this(new DialogService())
    {
    }

    public MainViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private string _parentFolderPath = string.Empty;

    [ObservableProperty]
    private string _prefix = string.Empty;

    [ObservableProperty]
    private string _suffix = string.Empty;

    [ObservableProperty]
    private string _startText = "1";

    [ObservableProperty]
    private string _endText = "10";

    [ObservableProperty]
    private string _stepText = "1";

    [ObservableProperty]
    private string _digitCountText = "2";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private bool _canCreate;

    public ObservableCollection<PreviewRowViewModel> PreviewItems { get; } = new();

    [RelayCommand]
    private void BrowseFolder()
    {
        var selected = _dialogService.SelectFolder(ParentFolderPath);
        if (selected is not null)
        {
            ParentFolderPath = selected;
        }
    }

    [RelayCommand]
    private void Preview()
    {
        CanCreate = false;
        PreviewItems.Clear();

        var input = new FolderSequenceInput
        {
            ParentFolderPath = ParentFolderPath,
            Prefix = Prefix,
            Suffix = Suffix,
            StartText = StartText,
            EndText = EndText,
            StepText = StepText,
            DigitCountText = DigitCountText,
        };

        var validation = FolderNameGenerator.Validate(input);
        if (!validation.IsSuccess)
        {
            StatusMessage = validation.ErrorMessage!;
            return;
        }

        var parsed = validation.Parsed!;

        if (parsed.Count > LargeBatchThreshold)
        {
            var proceed = _dialogService.Confirm(
                $"{parsed.Count}개의 폴더를 만들게 됩니다. 계속하시겠습니까?",
                "대량 생성 확인");

            if (!proceed)
            {
                StatusMessage = "미리보기를 취소했습니다.";
                return;
            }
        }

        var names = FolderNameGenerator.GenerateNames(parsed);
        var items = FolderCreationService.BuildPreview(parsed.ParentFolderPath, names);

        _currentItems = items;
        _currentParentFolderPath = parsed.ParentFolderPath;

        foreach (var item in items)
        {
            PreviewItems.Add(new PreviewRowViewModel(item));
        }

        StatusMessage = BuildCountSummary("미리보기", items);
        CanCreate = items.Any(i => i.Status == FolderItemStatus.New);
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        var summary = FolderCreationService.CreateAll(_currentParentFolderPath, _currentItems);

        var failureText = summary.Failures.Count > 0 ? $", 실패 {summary.Failures.Count}개" : string.Empty;
        StatusMessage =
            $"생성 완료: 생성 {summary.CreatedCount}개, 이미 존재 {summary.AlreadyExistsCount}개, 충돌 {summary.ConflictCount}개{failureText}";

        var refreshedNames = _currentItems.Select(i => i.Name).ToList();
        var refreshed = FolderCreationService.BuildPreview(_currentParentFolderPath, refreshedNames);
        _currentItems = refreshed;

        PreviewItems.Clear();
        foreach (var item in refreshed)
        {
            PreviewItems.Add(new PreviewRowViewModel(item));
        }

        CanCreate = refreshed.Any(i => i.Status == FolderItemStatus.New);
    }

    private static string BuildCountSummary(string label, IReadOnlyList<FolderPreviewItem> items)
    {
        var newCount = items.Count(i => i.Status == FolderItemStatus.New);
        var existingCount = items.Count(i => i.Status == FolderItemStatus.AlreadyExists);
        var conflictCount = items.Count(i => i.Status == FolderItemStatus.Conflict);
        return $"{label}: 전체 {items.Count}개 (신규 {newCount}개, 이미 존재 {existingCount}개, 충돌 {conflictCount}개)";
    }
}
