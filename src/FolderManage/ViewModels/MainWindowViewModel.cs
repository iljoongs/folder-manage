using FolderManage.Features.ImageRename;
using FolderManage.Features.MakeFolder;

namespace FolderManage.ViewModels;

// 메인 윈도우(탭 셸)의 ViewModel. 각 탭 ViewModel을 들고 있을 뿐 기능 로직은 갖지 않는다.
public class MainWindowViewModel
{
    public MainWindowViewModel(MakeFolderViewModel makeFolder, ImageRenameViewModel imageRename)
    {
        MakeFolder = makeFolder;
        ImageRename = imageRename;
    }

    public MakeFolderViewModel MakeFolder { get; }

    public ImageRenameViewModel ImageRename { get; }
}
