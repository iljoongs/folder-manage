using System.Windows;
using MakeFolder.ViewModels;

namespace MakeFolder.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
