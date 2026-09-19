using System.Windows;
using FolderManage.Common;
using FolderManage.Features.ImageRename;
using FolderManage.Features.MakeFolder;
using FolderManage.ViewModels;
using FolderManage.Views;

namespace FolderManage;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IDialogService dialogService = new DialogService();
        var makeFolderViewModel = new MakeFolderViewModel(dialogService);
        var imageRenameViewModel = new ImageRenameViewModel(
            dialogService,
            new JsonSuffixSettingsStore(JsonSuffixSettingsStore.DefaultFilePath));
        var mainWindow = new MainWindow(new MainWindowViewModel(makeFolderViewModel, imageRenameViewModel));

        mainWindow.Show();
    }
}

