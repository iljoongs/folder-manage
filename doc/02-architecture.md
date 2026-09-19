# 02. 아키텍처 및 프로젝트 구조

## 앱 구조: 메인 윈도우 + 탭

메인 윈도우는 탭을 담는 셸이고, 각 탭이 하나의 기능이다 → [06-main-window-tabs.md](06-main-window-tabs.md). 탭은 저마다 View + ViewModel(+ 필요한 Model/Service)을 갖고 서로 독립적이다.

탭은 현재 두 개다: 탭 1 폴더 만들기(`Features/MakeFolder`, 아래 "탭 1 계층 구조"), 탭 2 이미지 파일 이름 변경(`Features/ImageRename`, 아래 "탭 2 구성", 규칙은 [07-image-rename-spec.md](07-image-rename-spec.md)). 코드는 **기능(탭)별 폴더**로 나누고(2026-09-20), 두 탭이 함께 쓰는 것(`IDialogService`/`DialogService`, 폴더/파일 이름 규칙 `FileNameRules`)은 `Common/`에 둔다 → [05-open-decisions.md](05-open-decisions.md).

> **탭 셸.** `MainWindow`(TabControl)와 `MainWindowViewModel`이 셸이다(`Views/`, `ViewModels/` 폴더). 탭 1은 `MakeFolderTabView`(UserControl)와 `MakeFolderViewModel`, 탭 2는 `ImageRenameTabView`와 `ImageRenameViewModel`이다. 상세는 [06-main-window-tabs.md](06-main-window-tabs.md) "구현 현황".

## 탭 1 계층 구조 (MVVM)

아래는 **탭 1(폴더 만들기)** 의 구현이다(`Features/MakeFolder/`, 네임스페이스 `FolderManage.Features.MakeFolder`). 다른 탭도 같은 계층 원칙(View는 UI만, 로직은 ViewModel/Service)을 따른다.

- **Model**: 입력·결과 데이터.
  - `FolderSequenceInput` — 화면에서 받은 원본 문자열 입력(상위 폴더 경로, 접두사, 접미사, 시작/종료/증가/자리수 텍스트). 숫자 여부 검사가 서비스 한 곳에서 이뤄지도록 문자열 그대로 담는다.
  - `ParsedFolderSequence` — 검사를 통과한 값(숫자로 파싱된 시작/종료/증가/자리수 + 전체 개수).
  - `FolderSequenceValidationResult` — 검사 성공(파싱 결과) 또는 실패(오류 메시지).
  - `FolderPreviewItem`(+ `FolderItemStatus`: 신규/이미 존재/충돌) — 미리보기 목록의 한 줄.
  - `FolderCreationSummary`(+ `FolderCreationFailure`) — 생성 결과 집계.
- **Service**:
  - `FolderNameGenerator` — 입력값을 검사하고(접두사/접미사의 금지 문자 검사는 공용 `FileNameRules`를 쓴다) 생성할 폴더 이름 목록(문자열 리스트)을 계산하는 순수 로직. 파일 시스템에 접근하지 않는다 → 이름 규칙 상세는 [doc/03-folder-naming-spec.md](03-folder-naming-spec.md). 자동 생성용으로, 폴더 이름 목록에서 `접두사 + 숫자 + 접미사`에 맞는 숫자를 뽑는 `ExtractNumbers`도 여기에 있다(마찬가지로 순수 로직).
  - `FolderCreationService` — 상위 폴더 기준으로 각 이름이 이미 존재하는지 확인하고, 없는 것만 실제로 생성한다. 결과(생성됨/건너뜀 개수, 실패 항목)를 반환한다. 상위 폴더의 하위 폴더 이름 목록을 읽는 `GetSubfolderNames`(자동 생성용)도 여기에 있다.
  - `IDialogService` / `DialogService` — 폴더 선택 창, 확인/오류 메시지 창. ViewModel이 창 API에 직접 의존하지 않도록 인터페이스로 분리(테스트에서는 가짜 구현을 넣는다). 두 탭이 함께 쓰므로 `Common/`에 있다.
- **ViewModel**: `PreviewRowViewModel` — 미리보기 한 줄을 화면용(상태를 한글 텍스트로)으로 감싼 것. `MakeFolderViewModel` — 입력 필드 바인딩, "자동 생성"/"미리보기"/"폴더 생성" 커맨드, 미리보기 목록(이름 + 상태: 신규/이미 존재/충돌) 노출.
- **View**: `MakeFolderTabView.xaml`(UserControl) — 입력 폼 + 미리보기 리스트 + 결과 표시. 접미사 입력란의 기본값 자동 삭제처럼 순수하게 화면 조작에 해당하는 것만 code-behind에 두고, 나머지는 ViewModel에 바인딩한다. `MainWindow.xaml`은 이 UserControl을 `TabItem`에 담는 셸이다.

## 탭 2 구성 (이미지 파일 이름 변경)

`Features/ImageRename/`, 네임스페이스 `FolderManage.Features.ImageRename`. 순수 로직과 파일 시스템 접근을 나눠 순수 로직을 파일 시스템 없이 테스트한다(탭 1과 같은 방식).

- **Model**(record): `DirectoryEntry`(폴더/파일 이름), `NameParts`(공통 이름·접미사·확장자), `SuffixSettings`(파일 접미사 목록 + 폴더 접미사 목록), `GroupItem`, `NameGroup`(공통 이름이 같은 항목들), `RenamePreviewItem`, `RenamePlan`(변경 계획 + 충돌/오류 메시지), `RenameMove`(이름 하나의 변경), `RenameRecord`(되돌리기용 기록), `RenameResult`.
- **순수 로직 Service**: `NameGroupBuilder`(이름을 공통 이름/접미사/확장자로 나누고 그룹으로 묶음), `RenamePlanner`(새 이름 검증 + 변경 전 충돌 검사로 계획 생성), `SuffixRules`(추가할 파일/폴더 접미사 검사, 기본 접미사 목록 `.debug`·`.debug-result` / `_files`).
- **파일 시스템 Service**: `RenameService`(폴더 읽기, 이름 변경 실행 — 시작 전 재검사와 그룹 단위 원복, 대소문자만 바꾸는 경우의 임시 이름, 되돌리기), `ISuffixSettingsStore` / `JsonSuffixSettingsStore`(파일·폴더 접미사 목록을 `%AppData%\folder-manage\settings.json`에 저장·읽기, 파일이 없거나 깨지면 기본 목록).
- **ViewModel**: `ImageRenameViewModel`(대상 폴더, 접미사 목록, 그룹 목록/선택, 새 이름, 미리보기, "새로고침"/"미리보기"/"이름 변경"/"되돌리기"/접미사 "추가"·"삭제" 커맨드; 되돌리기 기록은 앱 실행 중에만 유지하는 스택), `NameGroupRowViewModel`, `RenamePreviewRowViewModel`(화면용 한글 텍스트).
- **View**: `ImageRenameTabView.xaml`(UserControl). code-behind는 비어 있다. 목록의 숫자 인식 정렬은 `Common/NaturalStringComparer`(비교기)와 `Common/NaturalSortBehavior`(DataGrid 첨부 속성 `IsEnabled`, 열 머리글 클릭 정렬을 자연 정렬로 대체)를 쓴다(탭 1의 미리보기 목록도 같다). 기본 순서는 `NameGroupBuilder`가 같은 비교기로 정렬해서 만든다.

## 진입점과 코드 규칙 (현재 코드가 따르는 것)

- **진입점**: `App.xaml`에 `StartupUri`를 두지 않고 `App.OnStartup`에서 `DialogService` → `MakeFolderViewModel` → `ImageRenameViewModel`(+ `JsonSuffixSettingsStore`) → `MainWindowViewModel` → `MainWindow` 순서로 직접 조립해 `Show()`한다(DI 컨테이너 없음, 형제 프로젝트 `text-readers`와 같은 방식).
- **ViewModel**: `ObservableObject` 상속. 필드는 `_camelCase` + `[ObservableProperty]`, 명령은 `[RelayCommand]`(메서드 `Preview` → `PreviewCommand`), 버튼 활성 조건은 `CanExecute` + `[NotifyCanExecuteChangedFor]`.
- **서비스**: 순수 로직/파일 시스템 서비스는 `static class`(`FolderNameGenerator`, `FolderCreationService`, `NameGroupBuilder`, `RenameService` 등)이고, 창 API(`IDialogService`)와 설정 저장(`ISuffixSettingsStore`)은 인터페이스로 생성자에 주입받는다.
- **언어**: 화면에 보이는 메시지와 코드 주석은 한국어, 커밋 메시지는 영어.
- **테스트**: xUnit. 파일 시스템을 쓰는 테스트는 임시 폴더(`Path.GetTempPath()` + GUID)를 만들고 `IDisposable.Dispose`에서 지운다. ViewModel 테스트는 공용 `FakeDialogService`(테스트 프로젝트)와 가짜 저장소를 넣는다.

## 폴더 구조

```
folder-manage/
├── CLAUDE.md
├── folder-manage.sln
├── .gitignore
├── doc/                       # 보조 지시서 (이 폴더)
├── src/
│   └── FolderManage/          # WPF 프로젝트 본체
│       ├── Common/            # 여러 탭이 함께 쓰는 것: IDialogService, DialogService, FileNameRules, NaturalStringComparer, NaturalSortBehavior
│       ├── Features/
│       │   ├── MakeFolder/    # 탭 1: 모델·서비스·ViewModel·View(MakeFolderTabView)
│       │   └── ImageRename/   # 탭 2: 모델·서비스·ViewModel·View(ImageRenameTabView)
│       ├── ViewModels/        # MainWindowViewModel (탭 셸)
│       ├── Views/             # MainWindow (탭 셸)
│       └── Assets/
└── tests/
    └── FolderManage.Tests/    # xUnit 단위 테스트
```

기능 폴더 안은 계층별 하위 폴더 없이 한 폴더에 두고(파일 이름이 역할을 드러낸다), 네임스페이스는 기능 폴더와 같다(`FolderManage.Features.MakeFolder` 등). 앱 이름은 `folder-manage`(코드 표기 `FolderManage`)이고 위 트리는 로컬 폴더까지 모두 새 이름으로 바뀐 상태다 → [06-main-window-tabs.md](06-main-window-tabs.md) "앱 이름 변경".

## 테스트 가능성

`FolderNameGenerator`는 파일 시스템 의존성이 없는 순수 함수로 설계해 단위 테스트로 이름 생성 규칙(자리수, 증가 단위, 접두사/접미사 조합)을 검증할 수 있게 한다. `FolderCreationService`만 실제 디렉터리 존재 확인/생성을 담당한다. ViewModel은 `IDialogService`를 주입받아 가짜 구현으로 테스트한다(`MakeFolderViewModelTests`). 탭 2도 같다: `NameGroupBuilder`/`RenamePlanner`/`SuffixRules`/`FileNameRules`는 순수 로직 테스트, `RenameService`/`JsonSuffixSettingsStore`는 임시 폴더 테스트(파일을 잠가 도중 실패와 원복까지 검증), `ImageRenameViewModel`은 가짜 창·저장소로 테스트한다.
