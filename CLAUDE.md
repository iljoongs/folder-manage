# folder-manage - 메인 지시서

이 문서는 프로젝트의 메인 지시서입니다. 작업 시작 전 아래 보조 지시서를 모두 읽고 진행하세요.

> **앱 이름은 `folder-manage`로 정했다**(여러 기능을 탭으로 담는 앱이 되었기 때문, [doc/05-open-decisions.md](doc/05-open-decisions.md)). 다만 **코드와 저장소는 아직 이전 이름 그대로**다 — 프로젝트 `MakeFolder`, 솔루션 `make-folder.sln`, 실행 파일 `MakeFolder.exe`, 아이콘 원본 `make-folder.png`, GitHub 저장소/로컬 폴더 `make-folder`. 이 문서의 폴더 구조·배포 명령 등은 코드의 실제 상태를 설명하므로 이전 이름으로 적혀 있고, 이름을 바꾸는 작업 때 함께 갱신한다 → [doc/06-main-window-tabs.md](doc/06-main-window-tabs.md) "앱 이름 변경 계획".

이 문서의 작업 원칙은 상위 폴더의 형제 프로젝트(`image-readers`, `text-readers`, `english-training`, `video-vault`)들이 공통으로 따르는 지시서 구조에서 가져왔습니다.

## 프로젝트 한 줄 요약
여러 기능을 탭으로 나눠 제공하는 WPF 데스크톱 앱. 메인 윈도우는 탭 구조이고 탭은 기능 단위로 나눈다([doc/06-main-window-tabs.md](doc/06-main-window-tabs.md)). 탭 1 "폴더 만들기"는 상위 폴더를 선택하면 그 안에 시작~종료 범위의 연속된 번호 폴더를 만들어주는 기능(지금까지 만든 make-folder 기능)이고, 탭 2 "이미지 파일 이름 변경"은 한 폴더 안에서 공통 이름이 같은 폴더·파일들의 이름을 함께 바꾸는 기능이다. 그 외 탭이 있다면 사용자가 설명할 예정이다.

## 기술 스택
- .NET 8
- WPF (Windows Presentation Foundation) — C#
- MVVM 패턴 (Service → ViewModel → View 계층 분리, [doc/02-architecture.md](doc/02-architecture.md) 참고)

## 폴더 구조

```
make-folder/
├── CLAUDE.md              # 메인 지시서 (이 파일)
├── make-folder.sln
├── .gitignore
├── make-folder.png        # 앱 아이콘 원본 PNG (512x512, 코드 아님, 루트 유지)
├── doc/                   # 기능별 상세 보조 지시서
├── src/
│   └── MakeFolder/        # 앱 본체 (Models/Services/ViewModels/Views, Assets/AppIcon.ico)
└── tests/
    └── MakeFolder.Tests/  # xUnit 단위 테스트 (FolderNameGenerator, FolderCreationService)
```

## 앱 아이콘

exe 아이콘(탐색기/작업 표시줄)과 `MainWindow` 타이틀바 아이콘 모두 `src/MakeFolder/Assets/AppIcon.ico`를 사용한다. 원본은 루트의 `make-folder.png`(512x512, 알파 채널 포함)이며, 16/32/48/256px로 고품질 리샘플링(`InterpolationMode.HighQualityBicubic`, 알파 유지) 후 PNG-압축 아이콘 항목으로 묶은 `.ico`로 변환했다(video-vault와 동일한 방식, 변환 스크립트 자체는 프로젝트에 포함하지 않음). `MakeFolder.csproj`의 `<ApplicationIcon>`과 `MainWindow.xaml`의 `Icon` 속성에서 이 파일을 참조한다.

## 단독 실행 파일(배포용) 만들기

.NET 런타임이 설치되어 있지 않은 PC에서도 바로 실행할 수 있는 단일 exe 파일을 아래 명령으로 만든다(개발 중 `dotnet build`/`dotnet run`에는 영향 없음).

```
dotnet publish src/MakeFolder/MakeFolder.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish/win-x64
```

- 결과물은 `publish/win-x64/MakeFolder.exe` 하나다(같이 생기는 `.pdb`는 디버그 심볼이라 배포에는 필요 없음). .NET 런타임을 그 안에 포함하므로 이 exe 하나만 다른 PC에 복사해도 실행된다.
- `publish/`는 빌드 산출물이라 `.gitignore`에 포함되어 저장소에 커밋되지 않는다. 배포가 필요할 때마다 위 명령으로 다시 생성한다.

## 보조 지시서 목록 (doc/ 폴더)

작업 성격에 맞는 문서를 참조하세요.

| 문서 | 내용 |
|---|---|
| [doc/01-overview.md](doc/01-overview.md) | 앱 개요(탭 구조), 탭 1(폴더 만들기)의 목적·v1(MVP) 범위 |
| [doc/02-architecture.md](doc/02-architecture.md) | 앱 구조(메인 윈도우 + 탭), MVVM 계층 구조, 프로젝트/폴더 구성 |
| [doc/03-folder-naming-spec.md](doc/03-folder-naming-spec.md) | (탭 1) 폴더 이름 생성 규칙 (시작/종료/증가 단위/자리수/접두사·접미사), 입력 기본값, 유효성 검사, 자동 생성(빠진 번호 채우기) |
| [doc/04-ui-flow.md](doc/04-ui-flow.md) | (탭 1) 화면 구성, 미리보기 → 생성 흐름, 자동 생성 버튼, 접미사 입력란 동작 |
| [doc/05-open-decisions.md](doc/05-open-decisions.md) | 확정한 결정 사항 기록과 아직 정해지지 않은 항목 (새 미결정 항목이 생기면 여기에 추가) |
| [doc/06-main-window-tabs.md](doc/06-main-window-tabs.md) | 메인 윈도우의 탭 구조, 탭 목록, 탭 공통 규칙, 새 탭 추가 절차, 앱 이름 변경 계획, 탭 구조로의 구현 개편 계획 |
| [doc/07-image-rename-spec.md](doc/07-image-rename-spec.md) | (탭 2) 이미지 파일 이름 변경: 공통 이름 판단 규칙(확장자 앞 `.debug`/`.debug-result` 접미사), 이름 그룹, 사용자가 직접 입력하는 새 이름, 안전 규칙(초안) |

## 개발 단계

**진행 중: 메인 윈도우를 탭 구조로 개편하는 계획 단계.** 앱 전체를 기능별 탭으로 바꾸고, 지금까지 만든 make-folder는 탭 1이 된다. 탭 1·2의 기능 설명은 문서에 반영했고(탭 2는 초안, 미결정 질문은 [doc/05-open-decisions.md](doc/05-open-decisions.md) "탭 2 미결정 항목"), 사용자가 답해 주면 문서를 확정한다. 사용자가 명시적으로 "코드 만들어", "구현해줘" 등으로 지시하기 전까지는 소스 코드(.cs, .xaml 등)를 바꾸지 않는다. 아래는 현재 코드(탭 구조 개편 전)의 상태다.

**탭 1(폴더 만들기) v1(MVP) 구현 완료.** [doc/01-overview.md](doc/01-overview.md)~[doc/05-open-decisions.md](doc/05-open-decisions.md) 계획대로 상위 폴더 선택, 접두사/접미사, 시작/종료/증가 단위/자리수 입력, 미리보기(신규/이미 존재/충돌 구분, 대량 생성 확인), 폴더 생성까지 동작한다. 이후 추가: 입력 기본값(접미사 `화`, 자리수 `1`), 접미사 기본값 자동 삭제, "자동 생성" 버튼(기존 숫자 폴더 사이의 빠진 번호 채우기). `FolderNameGenerator`/`FolderCreationService`/`MainViewModel`은 `tests/MakeFolder.Tests`의 xUnit 테스트로 검증했고, 실제 앱을 실행해 골든 패스(생성)와 스킵/충돌/유효성 오류 케이스를 확인했다.

## 작업 원칙

형제 프로젝트들에 공통으로 적용되는 규칙:

1. 각 기능을 구현/수정하기 전, 해당 작업과 관련된 보조 지시서(doc/ 폴더)를 먼저 확인한다.
2. 보조 지시서 간 내용이 충돌하거나, 아직 결정되지 않은 항목을 다뤄야 할 때는 임의로 확정하지 말고 합리적인 기본값을 제안한 뒤 사용자 확인을 받는다.
3. 코드는 MVVM 패턴을 따르며, View(XAML)와 로직(ViewModel/Service/Repository)을 분리한다.
4. 기능/구조/결정 사항이 변경되면 관련된 보조 지시서(doc/ 폴더 및 이 메인 지시서)를 함께 업데이트한다. 문서와 코드가 어긋난 상태로 두지 않는다.
5. 문서를 고칠 때 별도의 백업(history 폴더 등)은 두지 않는다 — git 커밋 이력이 변경 기록 역할을 한다. 의미 있는 단위로 커밋한다.
6. 사용자 명령으로 파일이 수정되고 작업이 성공적으로 끝나면, 별도 요청/확인 없이 git commit(커밋 메시지는 영어로 직접 작성)과 push까지 수행한다.
7. 명령 수행 후, 이번 작업에서 참조하거나 수정한 보조 지시서(doc/ 폴더) 목록을 사용자에게 알려준다.
