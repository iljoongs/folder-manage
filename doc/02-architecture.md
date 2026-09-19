# 02. 아키텍처 및 프로젝트 구조

## 계층 구조 (MVVM)

- **Model**: `FolderSequenceRequest` — 상위 폴더 경로, 접두사, 접미사, 시작/종료 번호, 증가 단위, 자리수를 담는 입력 데이터.
- **Service**:
  - `FolderNameGenerator` — 입력값을 받아 생성할 폴더 이름 목록(문자열 리스트)을 계산하는 순수 로직. 파일 시스템에 접근하지 않는다 → 이름 규칙 상세는 [doc/03-folder-naming-spec.md](03-folder-naming-spec.md). 자동 생성용으로, 폴더 이름 목록에서 `접두사 + 숫자 + 접미사`에 맞는 숫자를 뽑는 `ExtractNumbers`도 여기에 있다(마찬가지로 순수 로직).
  - `FolderCreationService` — 상위 폴더 기준으로 각 이름이 이미 존재하는지 확인하고, 없는 것만 실제로 생성한다. 결과(생성됨/건너뜀 개수, 실패 항목)를 반환한다. 상위 폴더의 하위 폴더 이름 목록을 읽는 `GetSubfolderNames`(자동 생성용)도 여기에 있다.
- **ViewModel**: `MainViewModel` — 입력 필드 바인딩, "자동 생성"/"미리보기"/"폴더 생성" 커맨드, 미리보기 목록(이름 + 상태: 신규/이미 존재/충돌) 노출.
- **View**: `MainWindow.xaml` — 입력 폼 + 미리보기 리스트 + 결과 표시. UI 로직은 code-behind에 두지 않고 ViewModel에 바인딩한다.

## 폴더 구조

```
make-folder/
├── CLAUDE.md
├── make-folder.sln
├── .gitignore
├── doc/                       # 보조 지시서 (이 폴더)
└── src/
    └── MakeFolder/            # WPF 프로젝트 본체
        ├── Models/
        ├── Services/
        ├── ViewModels/
        └── Views/
```

프로젝트/솔루션 이름(`MakeFolder`)은 임시 제안이며, 실제 구현 시작 전에 확정한다.

## 테스트 가능성

`FolderNameGenerator`는 파일 시스템 의존성이 없는 순수 함수로 설계해 단위 테스트로 이름 생성 규칙(자리수, 증가 단위, 접두사/접미사 조합)을 검증할 수 있게 한다. `FolderCreationService`만 실제 디렉터리 존재 확인/생성을 담당한다.
