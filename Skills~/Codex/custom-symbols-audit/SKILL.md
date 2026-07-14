---
name: custom-symbols-audit
description: Audit CustomSymbolsSO assets and compare configured platform/build symbols with current PlayerSettings without creating settings, applying defines, switching targets, or building.
---

# Audit Custom Symbols

Keep settings assets, EditorPrefs, PlayerSettings, active build target, and build output unchanged.

1. Read repository instructions and the package `README.md` and `AI_GUIDE.md`.
2. Resolve the exact absolute project or worktree. Require an already-running ready Editor, verify its project path and state, and record `git status --short`.
3. Use `unity-cli exec` with direct read-only APIs:
   - find settings with `AssetDatabase.FindAssets("t:CustomSymbolsSO")`;
   - load each result with `AssetDatabase.LoadAssetAtPath<CustomSymbolsSO>`;
   - read Standalone, Android, and iOS defines with `PlayerSettings.GetScriptingDefineSymbols`;
   - compare those values with the public All/platform lists and `includedInBuild` entries.

Do not call `CustomSymbolsSO.FindSettingsAsset()` because it remembers the selected path in EditorPrefs. Do not call `FindOrCreateSettingsAsset()`.

4. Check for no settings asset, multiple settings assets, blank or duplicate entries, a symbol referenced by a platform list but absent from `customAllSymbols`, configured/current differences per target, excluded build symbols, and inconsistent Standalone mapping between Windows and Mac.
5. Report settings paths, counts and differences per platform, excluded symbols, duplicates, and the active build target. Re-run `git status --short` and report any unexpected durable change.

Never open `Setting SO` or `SymbolsWindow`, write EditorPrefs, call any `SetScriptingDefineSymbols` overload, switch build targets, invoke `SymbolsBuildProcessor`, or start a build.
