using System.Windows;
using Microsoft.Win32;

namespace MakeFolder.Services;

public sealed class DialogService : IDialogService
{
    public string? SelectFolder(string? initialDirectory)
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = initialDirectory ?? string.Empty,
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public bool Confirm(string message, string title)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    public void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
