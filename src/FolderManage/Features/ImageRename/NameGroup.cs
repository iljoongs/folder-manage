namespace FolderManage.Features.ImageRename;

/// <summary>대상 폴더 바로 아래의 항목 하나(폴더 또는 파일). 이름만 담는다.</summary>
public sealed record DirectoryEntry(string Name, bool IsDirectory);

/// <summary>이름을 공통 이름 / 접미사 / 확장자로 나눈 결과. 폴더는 이름 전체가 공통 이름이다.</summary>
public sealed record NameParts(string CommonName, string Suffix, string Extension);

/// <summary>그룹에 실제로 있는 항목 하나. 접미사와 확장자는 이름을 바꿔도 그대로 유지된다.</summary>
public sealed record GroupItem(string Name, bool IsDirectory, string Suffix, string Extension);

/// <summary>공통 이름이 같은 폴더·파일들의 묶음.</summary>
public sealed record NameGroup(string CommonName, IReadOnlyList<GroupItem> Items);
