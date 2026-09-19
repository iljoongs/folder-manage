# 02. 아키텍처 및 프로젝트 구조

## 앱 구조: 메인 윈도우 + 탭

메인 윈도우는 탭을 담는 셸이고, 각 탭이 하나의 기능이다 → [06-main-window-tabs.md](06-main-window-tabs.md). 탭은 저마다 View + ViewModel(+ 필요한 Model/Service)을 갖고 서로 독립적이다.

탭은 현재 두 개다: 탭 1 폴더 만들기(아래 "계층 구조"에 설명한 기존 구현), 탭 2 이미지 파일 변경(계획만 있음, [07-image-rename-spec.md](07-image-rename-spec.md)). 탭 2의 서비스는 "이름 그룹 판정"(폴더·파일 이름에서 공통 이름 추출, 순수 로직)과 "이름 변경 실행"(파일 시스템 접근)으로 나누는 것을 제안한다 — 탭 1의 `FolderNameGenerator`/`FolderCreationService` 분리와 같은 방식이라 순수 로직을 파일 시스템 없이 테스트할 수 있다. 두 탭이 함께 쓰게 될 것(`IDialogService`, 폴더/파일 이름 검증 규칙)은 공용으로 뽑는 것을 제안한다 → [05-open-decisions.md](05-open-decisions.md) "코드 폴더 구조", "이름 검증 공용화".

> **현재 코드는 아직 탭 구조로 바뀌지 않았다.** 메인 윈도우(`MainWindow`)와 `MainViewModel`이 곧 탭 1(폴더 만들기)의 내용이다. 개편 계획은 [06-main-window-tabs.md](06-main-window-tabs.md) "구현 계획".

## 계층 구조 (MVVM)

아래는 **탭 1(폴더 만들기)** 의 현재 구현이다. 다른 탭도 같은 계층 원칙(View는 UI만, 로직은 ViewModel/Service)을 따른다.

- **Model**: 입력·결과 데이터.
  - `FolderSequenceInput` — 화면에서 받은 원본 문자열 입력(상위 폴더 경로, 접두사, 접미사, 시작/종료/증가/자리수 텍스트). 숫자 여부 검사가 서비스 한 곳에서 이뤄지도록 문자열 그대로 담는다.
  - `ParsedFolderSequence` — 검사를 통과한 값(숫자로 파싱된 시작/종료/증가/자리수 + 전체 개수).
  - `FolderSequenceValidationResult` — 검사 성공(파싱 결과) 또는 실패(오류 메시지).
  - `FolderPreviewItem`(+ `FolderItemStatus`: 신규/이미 존재/충돌) — 미리보기 목록의 한 줄.
  - `FolderCreationSummary`(+ `FolderCreationFailure`) — 생성 결과 집계.
- **Service**:
  - `FolderNameGenerator` — 입력값을 검사하고 생성할 폴더 이름 목록(문자열 리스트)을 계산하는 순수 로직. 파일 시스템에 접근하지 않는다 → 이름 규칙 상세는 [doc/03-folder-naming-spec.md](03-folder-naming-spec.md). 자동 생성용으로, 폴더 이름 목록에서 `접두사 + 숫자 + 접미사`에 맞는 숫자를 뽑는 `ExtractNumbers`도 여기에 있다(마찬가지로 순수 로직).
  - `FolderCreationService` — 상위 폴더 기준으로 각 이름이 이미 존재하는지 확인하고, 없는 것만 실제로 생성한다. 결과(생성됨/건너뜀 개수, 실패 항목)를 반환한다. 상위 폴더의 하위 폴더 이름 목록을 읽는 `GetSubfolderNames`(자동 생성용)도 여기에 있다.
  - `IDialogService` / `DialogService` — 폴더 선택 창, 확인/오류 메시지 창. ViewModel이 창 API에 직접 의존하지 않도록 인터페이스로 분리(테스트에서는 가짜 구현을 넣는다). 탭이 늘어나면 여러 탭이 함께 쓰는 공용 서비스가 된다.
- **ViewModel**: `MainViewModel` — 입력 필드 바인딩, "자동 생성"/"미리보기"/"폴더 생성" 커맨드, 미리보기 목록(이름 + 상태: 신규/이미 존재/충돌) 노출. (탭 구조로 바꿀 때 `MakeFolderViewModel`(가칭)로 이름이 바뀐다.)
- **View**: `MainWindow.xaml` — 입력 폼 + 미리보기 리스트 + 결과 표시. 접미사 입력란의 기본값 자동 삭제처럼 순수하게 화면 조작에 해당하는 것만 code-behind에 두고, 나머지는 ViewModel에 바인딩한다. (탭 구조로 바꿀 때 이 내용은 탭 1의 UserControl로 옮겨지고 `MainWindow`는 탭 셸이 된다.)

## 폴더 구조

현재:

```
make-folder/
├── CLAUDE.md
├── make-folder.sln
├── .gitignore
├── doc/                       # 보조 지시서 (이 폴더)
├── src/
│   └── MakeFolder/            # WPF 프로젝트 본체
│       ├── Models/
│       ├── Services/
│       ├── ViewModels/
│       └── Views/
└── tests/
    └── MakeFolder.Tests/      # xUnit 단위 테스트
```

탭이 늘어나면 Models/Services/ViewModels/Views를 기능(탭)별 폴더로 나눌지는 [05-open-decisions.md](05-open-decisions.md)의 미결정 항목이다. 프로젝트/솔루션 이름(`MakeFolder`)도 여러 기능을 담는 앱이 되는 만큼 유지할지 정해야 한다(같은 문서).

## 테스트 가능성

`FolderNameGenerator`는 파일 시스템 의존성이 없는 순수 함수로 설계해 단위 테스트로 이름 생성 규칙(자리수, 증가 단위, 접두사/접미사 조합)을 검증할 수 있게 한다. `FolderCreationService`만 실제 디렉터리 존재 확인/생성을 담당한다. ViewModel은 `IDialogService`를 주입받아 가짜 구현으로 테스트한다(`MainViewModelTests`).
