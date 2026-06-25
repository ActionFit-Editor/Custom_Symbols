# Custom Symbols (com.actionfit.customsymbols)

스크립팅 디파인 심볼을 **플랫폼별로 관리**하고, 빌드 시 빌드 미포함 심볼을 자동 제외했다가 복원하는 Unity 에디터 툴입니다.

## 설치 (manifest.json, Git URL)

```json
{
  "dependencies": {
    "com.actionfit.customsymbols": "https://github.com/ActionFit-Editor/Custom_Symbols.git#1.0.1"
  }
}
```

## 구성

- **Editor** (`com.actionfit.customsymbols.Editor`):
  - `SymbolsWindow` — 메뉴 `Build > Custom Symbols`
  - `CustomSymbolsSO` — 설정 SO (`Create New`로 소비자 Assets에 생성, 위치 자유)
  - `SymbolsBuildProcessor` — 빌드 전/후 심볼 적용·복원

## 설정 저장

`CustomSymbolsSO` 에셋에 저장되며, `EditorPrefs` 최근 경로 → 타입 기반 `FindAssets`로 위치와 무관하게 탐색합니다. 패키지에는 설정을 저장하지 않습니다.

## 선택적 연동

`BuildSettingsSO`(별도 BuildSetting 도구)가 프로젝트에 있으면 그 `manageSymbolsOnBuild` 값으로 빌드 시 심볼 관리 on/off를 따릅니다. 없으면 항상 활성(기본값)으로 동작하므로 단독 사용에 문제 없습니다.
