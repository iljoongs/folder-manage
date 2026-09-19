# 06. 메인 윈도우와 탭 구조

> **상태: 탭 셸 개편(2026-09-19)과 탭 2 구현(2026-09-20) 완료.** 메인 윈도우는 `TabControl` 셸이고 탭은 두 개다(폴더 만들기, 이미지 파일 이름 변경). 탭 3 이후는 사용자가 설명하기 전까지 만들지 않으며, 새 기능은 사용자가 명시적으로 구현을 지시하기 전까지 코드를 만들지 않는다.

## 개요

앱의 메인 윈도우를 **탭(TabControl)** 으로 구성한다. 탭은 **기능 단위**로 나눈다 — 한 탭은 하나의 기능을 맡는다. 지금까지 만든 make-folder 기능(연속 번호 폴더 만들기, 앱의 이전 이름)은 그중 **하나의 탭(탭 1)** 이 된다.

## 탭 목록

| 순서 | 탭 이름 | 기능 | 상세 문서 |
|---|---|---|---|
| 1 | 폴더 만들기 | 기존 make-folder 기능(앱의 이전 이름): 상위 폴더를 선택해 연속 번호 폴더를 만들고, 빠진 번호를 자동으로 채운다 | [01-overview.md](01-overview.md) "탭 1", [03-folder-naming-spec.md](03-folder-naming-spec.md), [04-ui-flow.md](04-ui-flow.md) |
| 2 | 이미지 파일 이름 변경 | 폴더와 파일이 함께 있는 폴더에서, (확장자·`.debug`/`.debug-result` 접미사를 뺀) 공통 이름이 같은 폴더·파일들의 이름을 그룹 하나씩 함께 바꾸고, 되돌릴 수 있다 | [07-image-rename-spec.md](07-image-rename-spec.md) |
| 3 이후 | (미정) | 사용자가 설명하면 이 표에 추가한다 | (기능 설명 후 작성) |

탭 이름과 순서(1, 2)는 사용자가 정했다. 탭 2의 규칙은 사용자 답변으로 확정해 구현했고([07-image-rename-spec.md](07-image-rename-spec.md)), 답변이 없어 제안값으로 구현한 세부는 [05-open-decisions.md](05-open-decisions.md)의 "탭 2 남은 세부 항목"에 있다.

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

## 앱 이름 변경 (완료)

앱 이름을 **`folder-manage`** 로 바꿨다([05-open-decisions.md](05-open-decisions.md)). 이름만 바꾼 작업이라 동작은 그대로이고, 기존 테스트 51개가 그대로 통과하며 실행 파일·창 제목·아이콘도 실제로 확인했다(2026-09-19).

| 대상 | 이전 | 현재 | 상태 |
|---|---|---|---|
| 솔루션 | `make-folder.sln` | `folder-manage.sln` | 완료 |
| 앱 프로젝트 | `src/MakeFolder/MakeFolder.csproj` | `src/FolderManage/FolderManage.csproj` | 완료 |
| 네임스페이스/어셈블리 | `MakeFolder`, `MakeFolder.Models` … | `FolderManage`, `FolderManage.Models` … | 완료 |
| 테스트 프로젝트 | `tests/MakeFolder.Tests` | `tests/FolderManage.Tests` | 완료 |
| 실행 파일 | `MakeFolder.exe` | `FolderManage.exe` | 완료 |
| 배포 명령/문서 경로 | `src/MakeFolder/...` | [CLAUDE.md](../CLAUDE.md)의 `dotnet publish` 명령, 폴더 구조를 새 이름으로 갱신 | 완료 |
| 아이콘 원본(루트) | `make-folder.png` | `folder-manage.png` (`Assets/AppIcon.ico`는 이름 그대로) | 완료 |
| 창 제목 | `MakeFolder` | `folder-manage` | 완료 |
| GitHub 저장소 | `iljoongs/make-folder` | `iljoongs/folder-manage` (`origin` 연결) | 완료 |
| 로컬 작업 폴더 | `e:\code\make-folder` | `e:\code\folder-manage` | 완료 (사용자가 직접 변경, 2026-09-19. 이후 새 세션에서 `bin/obj` 재생성, 빌드·테스트 51개 통과 확인) |

파일 이동은 `git mv`로 해서 이력을 유지했다. **작업 순서**: ① 앱 이름 변경(완료) → ② 탭 셸 개편(완료, 아래 "구현 현황") → ③ 탭 2 구현. 각 단계를 따로 커밋해서, 문제가 생기면 어느 단계 때문인지 바로 알 수 있게 한다.

## 로컬 폴더 이름 변경 후 후속 작업 (완료, 2026-09-19)

새 세션에서 `git status`(깨끗함)와 `git remote -v`(`origin` = `https://github.com/iljoongs/folder-manage.git`)를 확인했고, 이전 경로가 남은 `bin/obj`를 지운 뒤 `dotnet build`(경고·오류 0)와 `dotnet test`(51개 통과)를 확인했다. "로컬 폴더는 아직 `make-folder`"라고 적었던 문서 표기도 모두 현재 상태로 고쳤다. 새 세션에는 이전 대화와 프로젝트 메모리(경로별로 저장됨)가 이어지지 않으므로, 필요한 작업 원칙은 [CLAUDE.md](../CLAUDE.md)의 "작업 원칙"에 들어 있다.

## 구현 현황

### 탭 셸 개편 (완료, 2026-09-19)

동작은 그대로 두고 구조만 바꿨다. 기존 테스트 51개가 그대로 통과하고, 실행해서 탭 1 화면(입력 기본값 포함)을 확인했다. 파일 이동은 `git mv`로 이력을 유지했다.

| 이전 | 현재 |
|---|---|
| `Views/MainWindow.xaml` — 폴더 만들기 UI가 창 전체를 차지 | `MainWindow.xaml`은 `TabControl` 셸(탭 하나: "폴더 만들기"). 기존 UI는 `Views/MakeFolderTabView.xaml`(UserControl)로 옮겼고 접미사 입력란의 기본값 자동 삭제 동작([04-ui-flow.md](04-ui-flow.md))도 그 code-behind로 함께 옮겼다 |
| `ViewModels/MainViewModel.cs` | `MakeFolderViewModel`로 이름을 바꿨다. 새 `MainWindowViewModel`은 탭 ViewModel(`MakeFolder`)을 들고 있는 최소한의 역할만 한다 |
| `App.xaml.cs` — `MainViewModel` 조립 | `DialogService` → `MakeFolderViewModel` → `MainWindowViewModel` → `MainWindow` 순서로 조립 |
| `tests/.../MainViewModelTests.cs` | `MakeFolderViewModelTests.cs`. 나머지 서비스 테스트는 그대로 |

탭은 XAML에 고정으로 적는다(사용자가 탭을 추가·삭제·순서 변경하지 않는다는 [05-open-decisions.md](05-open-decisions.md)의 제안대로).

### 탭 2 구현과 코드 폴더 정리 (완료, 2026-09-20)

- **탭 2**: `Features/ImageRename`(모델, `NameGroupBuilder`/`RenamePlanner`/`RenameService`/`SuffixRules`, `JsonSuffixSettingsStore`, `ImageRenameViewModel`, `ImageRenameTabView`)을 만들고 셸에 두 번째 `TabItem`으로 붙였다. `MainWindowViewModel`은 `MakeFolder`와 `ImageRename` 두 탭 ViewModel을 들고 있다. 구성은 [02-architecture.md](02-architecture.md) "탭 2 구성", 규칙은 [07-image-rename-spec.md](07-image-rename-spec.md).
- **코드 폴더 구조**: 계층별(Models/Services/ViewModels/Views)에서 기능(탭)별로 바꿨다. 탭 1 파일은 `Features/MakeFolder/`로, `IDialogService`/`DialogService`는 `Common/`으로 옮겼다(`git mv`로 이력 유지, 네임스페이스는 폴더와 같게 변경). 탭 셸(`MainWindow`, `MainWindowViewModel`)만 `Views/`, `ViewModels/`에 남았다.
- **이름 검증 공용화**: 금지 문자 검사와 새 이름 검증(빈 이름, 끝의 `.`/공백, 예약 이름)을 `Common/FileNameRules`로 뽑았다. 탭 1의 접두사/접미사 검사(`FolderNameGenerator.Validate`)도 이것을 쓰며 동작과 메시지는 그대로다.
- **창 크기**: 탭 2 화면(그룹 목록 + 미리보기 목록)이 들어가도록 640×560(최소 520×480)에서 720×700(최소 600×560)으로 키웠다.
- **탭 2 보완(2026-09-20)**: 폴더 접미사 `_files` 지원(폴더 전용 접미사 목록 추가, 화면에서 파일/폴더 접미사를 나란히 관리)과 목록의 숫자 자연 정렬(기본 순서 + 열 머리글 정렬)을 추가했다 → [07-image-rename-spec.md](07-image-rename-spec.md). 테스트는 173개, 실제 앱에서 `N_files` 폴더가 `N.html` 그룹으로 묶이고 머리글 클릭이 `1, 2, 3, 10, 20`(내림차순은 반대)으로 정렬되는 것을 확인했다.
- **탭 1 자연 정렬(2026-09-20)**: 탭 1의 미리보기 목록 열 머리글 정렬에도 같은 자연 정렬을 적용했다(`1화, 2화, … 10화`, 내림차순은 반대). 실제 앱에서 1~12화 미리보기로 확인했다.
- **검증**: 테스트 140개 통과(기존 51 + 탭 2·공용 89). 실제 앱에서 임시 폴더로 그룹 읽기 → 그룹 선택(새 이름 자동 채움) → 미리보기 → 이름 변경(폴더 1 + 파일 3) → 되돌리기를 UI Automation으로 실행해 디스크의 이름이 예상대로 바뀌고 돌아오는 것을 확인했다.

탭 상태 유지 규칙(공통 규칙 2)은 탭 콘텐츠가 `TabItem` 안에 그대로 있고 ViewModel이 셸에 붙어 있어서 두 탭 모두 지켜진다.

### 아직 안 한 것

- 탭 3 이후(사용자 설명 대기).
- 탭 1 개선 후보 A(상위 폴더 존재 검사), B(실패 항목 표시) — [05-open-decisions.md](05-open-decisions.md).
