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
        var mainViewModel = new MainViewModel(dialogService);
        var mainWindow = new MainWindow(mainViewModel);

        mainWindow.Show();
    }
}

