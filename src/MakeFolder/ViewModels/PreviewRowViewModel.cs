using MakeFolder.Models;

namespace MakeFolder.ViewModels;

public sealed class PreviewRowViewModel
{
    public PreviewRowViewModel(FolderPreviewItem item)
    {
        Name = item.Name;
        Status = item.Status;
    }

    public string Name { get; }

    public FolderItemStatus Status { get; }

    public string StatusText => Status switch
    {
        FolderItemStatus.New => "신규",
        FolderItemStatus.AlreadyExists => "이미 존재",
        FolderItemStatus.Conflict => "충돌 (파일 있음)",
        _ => Status.ToString(),
    };
}
