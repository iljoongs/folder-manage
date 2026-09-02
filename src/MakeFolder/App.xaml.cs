using System.Windows;
using MakeFolder.Services;
using MakeFolder.ViewModels;
using MakeFolder.Views;

namespace MakeFolder;

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

