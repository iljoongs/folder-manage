# 06. 메인 윈도우와 탭 구조

> **상태: 계획 단계 (코드 미변경).** 현재 코드는 메인 윈도우 하나에 폴더 만들기 기능만 들어 있는 상태다. 이 문서는 그것을 탭 구조로 바꾸기 위한 계획이며, 사용자가 명시적으로 구현을 지시하기 전까지는 코드를 바꾸지 않는다.

## 개요

앱의 메인 윈도우를 **탭(TabControl)** 으로 구성한다. 탭은 **기능 단위**로 나눈다 — 한 탭은 하나의 기능을 맡는다. 지금까지 만든 make-folder(연속 번호 폴더 만들기)는 그중 **하나의 탭(탭 1)** 이 된다.

## 탭 목록

| 순서 | 탭 이름 | 기능 | 상세 문서 |
|---|---|---|---|
| 1 | 폴더 만들기 | 기존 MakeFolder 기능: 상위 폴더를 선택해 연속 번호 폴더를 만들고, 빠진 번호를 자동으로 채운다 | [01-overview.md](01-overview.md) "탭 1", [03-folder-naming-spec.md](03-folder-naming-spec.md), [04-ui-flow.md](04-ui-flow.md) |
| 2 | 이미지 파일 이름 변경 | 폴더와 파일이 함께 있는 폴더에서, (확장자·`.debug`/`.debug-result` 접미사를 뺀) 공통 이름이 같은 폴더·파일들의 이름을 함께 바꾼다 | [07-image-rename-spec.md](07-image-rename-spec.md) |
| 3 이후 | (미정) | 사용자가 설명하면 이 표에 추가한다 | (기능 설명 후 작성) |

탭 이름과 순서(1, 2)는 사용자가 정했다. 탭 2의 세부 규칙 중 정해지지 않은 부분은 [05-open-decisions.md](05-open-decisions.md)의 "탭 2 미결정 항목"에 있다.

## 메인 윈도우(셸)의 역할

- 탭을 담는 틀이다. 창 제목, 아이콘, 창 크기/최소 크기 같은 앱 공통 요소만 맡고, 기능 로직은 갖지 않는다.
- 각 탭의 내용은 그 탭 전용 View와 ViewModel이 책임진다.

## 탭 공통 규칙

1. **독립성**: 각 탭은 자기 View(UserControl) + ViewModel + 필요한 Model/Service를 갖고, 다른 탭을 직접 참조하지 않는다. 한 탭의 기능을 고쳐도 다른 탭에 영향이 없어야 한다.
2. **상태 유지**: 탭을 전환했다 돌아와도 그 탭의 입력값과 결과(예: 폴더 만들기 탭의 입력란, 미리보기 목록, 상태 메시지)가 그대로 남아 있어야 한다.
3. **MVVM 유지**: 탭 안에서도 View(XAML)와 로직(ViewModel/Service)을 분리한다 ([02-architecture.md](02-architecture.md), [CLAUDE.md](../CLAUDE.md) 작업 원칙 3).
4. **문서는 탭별로**: 탭마다 그 기능의 상세 규칙을 다루는 보조 지시서를 `doc/`에 둔다. 위 탭 목록 표에서 링크하고, [CLAUDE.md](../CLAUDE.md)의 보조 지시서 목록에도 추가한다.

## 새 탭을 추가하는 절차

1. 사용자가 그 탭의 기능을 설명한다.
2. 이 문서의 탭 목록 표를 갱신하고, [05-open-decisions.md](05-open-decisions.md)에 새로 생기는 미결정 항목을 적는다(임의로 확정하지 않는다).
3. 그 탭의 보조 지시서를 `doc/`에 작성하고 [CLAUDE.md](../CLAUDE.md) 문서 목록에 추가한다.
4. 사용자가 구현을 지시하면 View/ViewModel/Service와 테스트를 만든다.

## 앱 이름 변경 계획 (미구현)

앱 이름을 **`folder-manage`** 로 바꾸기로 했다([05-open-decisions.md](05-open-decisions.md)). 아직 코드·저장소는 바뀌지 않았고, 아래 표는 바꿀 때의 대상 목록이다. 이름만 바꾸는 작업이라 동작은 달라지지 않아야 하며, 기존 테스트가 그대로 통과하는지로 확인한다.

| 대상 | 현재 | 변경 후 |
|---|---|---|
| 솔루션 | `make-folder.sln` | `folder-manage.sln` |
| 앱 프로젝트 | `src/MakeFolder/MakeFolder.csproj` | `src/FolderManage/FolderManage.csproj` |
| 네임스페이스/어셈블리 | `MakeFolder`, `MakeFolder.Models` … | `FolderManage`, `FolderManage.Models` … |
| 테스트 프로젝트 | `tests/MakeFolder.Tests` | `tests/FolderManage.Tests` |
| 실행 파일 | `MakeFolder.exe` | `FolderManage.exe` |
| 배포 명령/문서 경로 | [CLAUDE.md](../CLAUDE.md)의 `dotnet publish` 명령, 폴더 구조 | 새 이름으로 갱신 |
| 아이콘 원본(루트) | `make-folder.png` | `folder-manage.png` (`Assets/AppIcon.ico`는 이름 그대로) |
| 창 제목 | `MakeFolder` | `folder-manage` |
| 문서 표기 | [CLAUDE.md](../CLAUDE.md) 제목, doc/01~07의 `MakeFolder`/`make-folder` | 코드 변경과 같은 작업 단위로 갱신 |
| GitHub 저장소 | `iljoongs/make-folder` | **완료**: `iljoongs/folder-manage`로 이름이 바뀌었고 `origin`도 `https://github.com/iljoongs/folder-manage.git`로 연결했다 |
| 로컬 작업 폴더 | `e:\code\make-folder` | **사용자가 직접** 이름 변경. Claude Code 세션 경로가 바뀌므로 변경 후 새 세션으로 시작 |

파일 이동은 `git mv`로 해서 이력을 유지한다. **작업 순서 제안**: ① 앱 이름 변경 → ② 폴더 구조/탭 셸 개편(아래) → ③ 탭 2 구현. 각 단계를 따로 커밋해서, 문제가 생기면 어느 단계 때문인지 바로 알 수 있게 한다.

## 구현 계획 (미구현 — 이름은 모두 가칭)

현재 코드에서 탭 구조로 옮길 때의 변경 방향이다. 실제 구현 시점에 [05-open-decisions.md](05-open-decisions.md)의 결정을 반영해 다시 확인한다.

| 현재 | 변경 방향 |
|---|---|
| `Views/MainWindow.xaml` — 폴더 만들기 UI가 창 전체를 차지 | 탭을 담는 셸(TabControl)로 교체. 기존 UI는 `Views/MakeFolderTabView.xaml`(UserControl)로 옮긴다. 접미사 입력란의 기본값 자동 삭제 동작([04-ui-flow.md](04-ui-flow.md))도 함께 옮긴다 |
| `ViewModels/MainViewModel.cs` — 폴더 만들기 로직 | `MakeFolderViewModel`로 이름을 바꾼다. 메인 윈도우용 ViewModel(`MainWindowViewModel`)은 탭 ViewModel 목록을 들고 있는 최소한의 역할만 한다 |
| (없음) | 탭 2용으로 `Views/ImageRenameTabView.xaml`(UserControl)과 `ImageRenameViewModel`(가칭), 이름 그룹 판정/이름 변경 서비스를 새로 만든다 → [07-image-rename-spec.md](07-image-rename-spec.md) |
| `App.xaml.cs` — `MainViewModel`, `DialogService` 조립 | 각 탭 ViewModel과 공용 서비스(`IDialogService` 등)를 조립해 메인 윈도우에 넘긴다 |
| `tests/MakeFolder.Tests/MainViewModelTests.cs` | 이름 변경을 따라간다. 나머지 서비스 테스트는 그대로 |
| `Models/`, `Services/` (계층별 폴더) | 탭이 2개가 되므로 기능(탭)별 폴더로 나눌지 결정한다 — [05-open-decisions.md](05-open-decisions.md) 참고 |
| 폴더/파일 이름 검증(금지 문자 등, 현재 `FolderNameGenerator.Validate` 안에 있음) | 탭 1과 탭 2가 함께 쓰므로 공용 서비스로 뽑는 것을 제안 — [02-architecture.md](02-architecture.md) |
