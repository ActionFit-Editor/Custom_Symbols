# Custom Symbols (com.actionfit.customsymbols)

스크립팅 디파인 심볼을 **플랫폼별로 관리**하고, 빌드 시 빌드 미포함 심볼을 자동 제외했다가 복원하는 Unity 에디터 툴입니다.

## 설치 (manifest.json, Git URL)

```json
{
  "dependencies": {
    "com.actionfit.customsymbols": "https://github.com/ActionFit-Editor/Custom_Symbols.git#1.0.8"
  }
}
```

## Unity 메뉴

- Package root: `Tools > Package > Custom Symbols`.
- README: `Tools > Package > Custom Symbols > README`.
- Setting SO: `Tools > Package > Custom Symbols > Setting SO` (없으면 기본 경로에 생성 후 선택).
- 패키지 명령은 같은 package root 아래에 유지하며 README/Setting SO 항목이 있으면 분리된 해당 항목보다 위에 표시합니다.

## 구성

- **Editor** (`com.actionfit.customsymbols.Editor`):
  - `SymbolsWindow` — 메뉴 `Tools > Package > Custom Symbols > Open Window`
  - `CustomSymbolsSO` — 설정 SO (최초 자동 생성, 추가 `Create New` 위치 자유)
  - `SymbolsBuildProcessor` — 빌드 전/후 심볼 적용·복원

## 설정 저장

프로젝트에 `CustomSymbolsSO`가 하나도 없으면 Unity Editor 로드 시 `Assets/_Data/_CustomSymbols/SymbolsSettings.asset`을 자동 생성합니다. 최초 생성 시 Standalone, Android, iOS의 현재 scripting define symbols를 모두 가져와 `Build`를 활성화하고, Standalone은 Win/Mac, Android는 AOS, iOS는 iOS 플랫폼 체크에 반영합니다. 모든 대상에 공통인 심볼은 `All`에도 등록합니다.

기존 `CustomSymbolsSO`가 다른 위치에 있으면 새 에셋을 만들거나 기존 값을 덮어쓰지 않습니다. 탐색 순서는 `EditorPrefs` 최근 경로 → 기본 `_Data` 경로 → 타입 기반 `FindAssets`이며, 패키지 내부에는 프로젝트 설정을 저장하지 않습니다. 창의 `Create New`로 추가 에셋을 만들 때도 생성 시점의 현재 프로젝트 심볼로 동일하게 초기화합니다.

## 선택적 연동

`BuildSettingsSO`(별도 BuildSetting 도구)가 프로젝트에 있으면 그 `manageSymbolsOnBuild` 값으로 빌드 시 심볼 관리 on/off를 따릅니다. 없으면 항상 활성(기본값)으로 동작하므로 단독 사용에 문제 없습니다.

`Application.isBatchMode`로 실행되는 CI/AutoBuild에서는 에디터 심볼과 빌드 심볼이 달라도 확인 팝업을 띄우지 않고 로그만 남긴 뒤 빌드용 심볼을 적용합니다. 일반 에디터 빌드에서는 기존처럼 차이를 확인하고 취소할 수 있습니다.

## Agent Skill 안내

schema v2 `Skills~/manifest.json`이 Codex와 Claude에 다음 read-only 스킬을 제공합니다.

- `$custom-symbols-help`: 설정 탐색, 플랫폼 목록과 빌드 전후 심볼 동작을 설명합니다.
- `$custom-symbols-audit`: 기존 `CustomSymbolsSO`와 PlayerSettings의 현재 define을 비교해 차이와 중복을 보고합니다.

audit은 설정을 만들거나 `EditorPrefs`를 갱신하지 않고, define 적용·플랫폼 전환·빌드를 실행하지 않습니다.
