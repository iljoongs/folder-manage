namespace FolderManage.Features.ImageRename;

/// <summary>미리보기 목록의 한 줄을 화면용(종류·상태를 한글 텍스트로)으로 감싼 것.</summary>
public sealed class RenamePreviewRowViewModel
{
    public RenamePreviewRowViewModel(RenamePreviewItem item)
    {
        OldName = item.OldName;
        NewName = item.NewName;
        KindText = item.IsDirectory ? "폴더" : "파일";
        StatusText = item.HasConflict ? "이미 존재" : "변경 가능";
    }

    public string OldName { get; }

    public string NewName { get; }

    public string KindText { get; }

    public string StatusText { get; }
}
