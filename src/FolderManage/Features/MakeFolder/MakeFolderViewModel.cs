using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FolderManage.Common;

namespace FolderManage.Features.MakeFolder;

public partial class MakeFolderViewModel : ObservableObject
{
    public const string DefaultSuffix = "화";

    private const int LargeBatchThreshold = 1000;

    private readonly IDialogService _dialogService;

    private IReadOnlyList<FolderPreviewItem> _currentItems = Array.Empty<FolderPreviewItem>();
    private string _currentParentFolderPath = string.Empty;

    public MakeFolderViewModel()
        : this(new DialogService())
    {
    }

    public MakeFolderViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private string _parentFolderPath = string.Empty;

    [ObservableProperty]
    private string _prefix = string.Empty;

    [ObservableProperty]
    private string _suffix = DefaultSuffix;

    [ObservableProperty]
    private string _startText = "1";

    [ObservableProperty]
    private string _endText = "10";

    [ObservableProperty]
    private string _stepText = "1";

    [ObservableProperty]
    private string _digitCountText = "1";

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

        ShowPreview(parsed.ParentFolderPath, items);
        StatusMessage = BuildCountSummary("미리보기", items);
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        var summary = FolderCreationService.CreateAll(_currentParentFolderPath, _currentItems);
        var names = _currentItems.Select(i => i.Name).ToList();

        ShowPreview(_currentParentFolderPath, FolderCreationService.BuildPreview(_currentParentFolderPath, names));
        StatusMessage = $"생성 완료: {BuildCreateSummary(summary)}";
    }

    /// <summary>
    /// 상위 폴더에서 현재 접두사/접미사 형식의 숫자 폴더를 찾아, 가장 작은 번호와 가장 큰 번호 사이에서
    /// 빠진 폴더(증가 단위·자리수는 입력값 그대로)를 바로 만든다. doc/03-folder-naming-spec.md 참고.
    /// </summary>
    [RelayCommand]
    private void AutoCreate()
    {
        CanCreate = false;
        PreviewItems.Clear();

        if (string.IsNullOrWhiteSpace(ParentFolderPath))
        {
            StatusMessage = "상위 폴더를 선택하세요.";
            return;
        }

        if (!Directory.Exists(ParentFolderPath))
        {
            StatusMessage = "상위 폴더를 찾을 수 없습니다.";
            return;
        }

        IReadOnlyList<string> existingNames;
        try
        {
            existingNames = FolderCreationService.GetSubfolderNames(ParentFolderPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusMessage = $"상위 폴더를 읽을 수 없습니다: {ex.Message}";
            return;
        }

        var numbers = FolderNameGenerator.ExtractNumbers(existingNames, Prefix, Suffix);
        if (numbers.Count == 0)
        {
            StatusMessage = $"'{Prefix}숫자{Suffix}' 형식의 폴더가 없습니다.";
            return;
        }

        var first = numbers[0];
        var last = numbers[^1];

        var input = new FolderSequenceInput
        {
            ParentFolderPath = ParentFolderPath,
            Prefix = Prefix,
            Suffix = Suffix,
            StartText = first.ToString(),
            EndText = last.ToString(),
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
                $"{first}~{last} 범위의 폴더 {parsed.Count}개를 확인하고 빠진 폴더를 만듭니다. 계속하시겠습니까?",
                "대량 생성 확인");

            if (!proceed)
            {
                StatusMessage = "자동 생성을 취소했습니다.";
                return;
            }
        }

        var names = FolderNameGenerator.GenerateNames(parsed);
        var items = FolderCreationService.BuildPreview(parsed.ParentFolderPath, names);
        var summary = FolderCreationService.CreateAll(parsed.ParentFolderPath, items);

        ShowPreview(parsed.ParentFolderPath, FolderCreationService.BuildPreview(parsed.ParentFolderPath, names));

        StatusMessage = summary.CreatedCount == 0 && summary.Failures.Count == 0
            ? $"자동 생성 ({first}~{last}): 새로 만들 폴더가 없습니다 ({BuildCreateSummary(summary)})"
            : $"자동 생성 완료 ({first}~{last}): {BuildCreateSummary(summary)}";
    }

    private void ShowPreview(string parentFolderPath, IReadOnlyList<FolderPreviewItem> items)
    {
        _currentItems = items;
        _currentParentFolderPath = parentFolderPath;

        PreviewItems.Clear();
        foreach (var item in items)
        {
            PreviewItems.Add(new PreviewRowViewModel(item));
        }

        CanCreate = items.Any(i => i.Status == FolderItemStatus.New);
    }

    private static string BuildCreateSummary(FolderCreationSummary summary)
    {
        var failureText = summary.Failures.Count > 0 ? $", 실패 {summary.Failures.Count}개" : string.Empty;
        return $"생성 {summary.CreatedCount}개, 이미 존재 {summary.AlreadyExistsCount}개, 충돌 {summary.ConflictCount}개{failureText}";
    }

    private static string BuildCountSummary(string label, IReadOnlyList<FolderPreviewItem> items)
    {
        var newCount = items.Count(i => i.Status == FolderItemStatus.New);
        var existingCount = items.Count(i => i.Status == FolderItemStatus.AlreadyExists);
        var conflictCount = items.Count(i => i.Status == FolderItemStatus.Conflict);
        return $"{label}: 전체 {items.Count}개 (신규 {newCount}개, 이미 존재 {existingCount}개, 충돌 {conflictCount}개)";
    }
}
