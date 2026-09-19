using FolderManage.Common;

namespace FolderManage.Tests;

/// <summary>ViewModel 테스트용 가짜 창 서비스. 실제 창을 띄우지 않고 호출 횟수만 센다.</summary>
internal sealed class FakeDialogService : IDialogService
{
    public bool ConfirmResult { get; set; } = true;

    public int ConfirmCallCount { get; private set; }

    public string? SelectedFolderToReturn { get; set; }

    public string? SelectFolder(string? initialDirectory) => SelectedFolderToReturn;

    public bool Confirm(string message, string title)
    {
        ConfirmCallCount++;
        return ConfirmResult;
    }

    public void ShowError(string message, string title)
    {
    }
}
