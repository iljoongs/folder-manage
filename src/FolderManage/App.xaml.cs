using System.Windows;
using FolderManage.Services;
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
        var mainWindow = new MainWindow(new MainWindowViewModel(makeFolderViewModel));

        mainWindow.Show();
    }
}

