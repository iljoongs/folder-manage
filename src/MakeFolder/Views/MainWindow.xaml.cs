using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MakeFolder.ViewModels;

namespace MakeFolder.Views;

public partial class MainWindow : Window
{
    // 접미사 입력란의 기본값("화")이 손대지 않은 채로 남아 있는 동안 true.
    private bool _suffixIsUntouchedDefault = true;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    // 접미사 기본값이 그대로일 때 처음 입력하려 하면 기본값이 전체 선택되어, 입력한 내용이 기본값을 대체한다.
    // 마우스 클릭은 캐럿 배치가 선택을 풀어버리므로, 아직 포커스가 없으면 포커스만 주고 클릭은 소비한다.
    private void SuffixTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_suffixIsUntouchedDefault && !SuffixTextBox.IsKeyboardFocusWithin)
        {
            SuffixTextBox.Focus();
            e.Handled = true;
        }
    }

    private void SuffixTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_suffixIsUntouchedDefault && SuffixTextBox.Text == MainViewModel.DefaultSuffix)
        {
            SuffixTextBox.SelectAll();
        }
    }

    private void SuffixTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (SuffixTextBox.Text != MainViewModel.DefaultSuffix)
        {
            _suffixIsUntouchedDefault = false;
        }
    }
}
