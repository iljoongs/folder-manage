namespace MakeFolder.Services;

public interface IDialogService
{
    string? SelectFolder(string? initialDirectory);

    bool Confirm(string message, string title);

    void ShowError(string message, string title);
}
