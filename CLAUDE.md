# make-folder - 메인 지시서

이 문서는 프로젝트의 메인 지시서입니다. 작업 시작 전 아래 보조 지시서를 모두 읽고 진행하세요.

이 문서의 작업 원칙은 상위 폴더의 형제 프로젝트(`image-readers`, `text-readers`, `english-training`, `video-vault`)들이 공통으로 따르는 지시서 구조에서 가져왔습니다.

## 프로젝트 한 줄 요약
특정 상위 폴더를 선택하면, 그 안에 시작~종료 범위의 연속된 번호 폴더를 자동으로 만들어주는 WPF 데스크톱 도구.

## 기술 스택
- .NET 8
- WPF (Windows Presentation Foundation) — C#
- MVVM 패턴 (Service → ViewModel → View 계층 분리, [doc/02-architecture.md](doc/02-architecture.md) 참고)

## 폴더 구조

```
make-folder/
├── CLAUDE.md          # 메인 지시서 (이 파일)
├── make-folder.sln    # (구현 시작 시 생성)
├── .gitignore
├── doc/               # 기능별 상세 보조 지시서
└── src/
    └── MakeFolder/    # 앱 본체 소스 (구현 시작 시 생성, 이름은 doc/05-open-decisions.md에서 확정)
```

## 보조 지시서 목록 (doc/ 폴더)

작업 성격에 맞는 문서를 참조하세요.

| 문서 | 내용 |
|---|---|
| [doc/01-overview.md](doc/01-overview.md) | 프로젝트 개요, 목적, v1(MVP) 범위 |
| [doc/02-architecture.md](doc/02-architecture.md) | MVVM 계층 구조, 프로젝트/폴더 구성 |
| [doc/03-folder-naming-spec.md](doc/03-folder-naming-spec.md) | 폴더 이름 생성 규칙 (시작/종료/증가 단위/자리수/접두사·접미사), 유효성 검사 |
| [doc/04-ui-flow.md](doc/04-ui-flow.md) | 화면 구성, 미리보기 → 생성 흐름 |
| [doc/05-open-decisions.md](doc/05-open-decisions.md) | 구현 전 확정한 결정 사항 기록 (새 미결정 항목이 생기면 여기에 추가) |

## 개발 단계

**개발 계획 검토 및 확정 완료** — 개요/아키텍처/이름 생성 규칙/UI 흐름 문서화 및 결정 사항([doc/05-open-decisions.md](doc/05-open-decisions.md)) 확정까지 마쳤다. 사용자가 명시적으로 "코드 만들어", "구현해줘" 등으로 지시하기 전까지는 소스 코드(.cs, .xaml 등)를 작성하지 않는다.

## 작업 원칙

형제 프로젝트들에 공통으로 적용되는 규칙:

1. 각 기능을 구현/수정하기 전, 해당 작업과 관련된 보조 지시서(doc/ 폴더)를 먼저 확인한다.
2. 보조 지시서 간 내용이 충돌하거나, 아직 결정되지 않은 항목을 다뤄야 할 때는 임의로 확정하지 말고 합리적인 기본값을 제안한 뒤 사용자 확인을 받는다.
3. 코드는 MVVM 패턴을 따르며, View(XAML)와 로직(ViewModel/Service/Repository)을 분리한다.
4. 기능/구조/결정 사항이 변경되면 관련된 보조 지시서(doc/ 폴더 및 이 메인 지시서)를 함께 업데이트한다. 문서와 코드가 어긋난 상태로 두지 않는다.
5. 문서를 고칠 때 별도의 백업(history 폴더 등)은 두지 않는다 — git 커밋 이력이 변경 기록 역할을 한다. 의미 있는 단위로 커밋한다.
6. 사용자 명령으로 파일이 수정되고 작업이 성공적으로 끝나면, 별도 요청/확인 없이 git commit(커밋 메시지는 영어로 직접 작성)과 push까지 수행한다.
7. 명령 수행 후, 이번 작업에서 참조하거나 수정한 보조 지시서(doc/ 폴더) 목록을 사용자에게 알려준다.
