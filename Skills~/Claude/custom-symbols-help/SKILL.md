---
name: custom-symbols-help
description: Explain Custom Symbols, its installed skills, settings discovery, platform symbol lists, build-time filtering and restoration, menus, and read-only audit boundaries.
---

# Custom Symbols Help

Answer in the user's language. Explain workflows without creating settings, changing PlayerSettings, applying defines, switching platforms, or starting a build unless the user separately requests those operations.

1. Read `PACKAGE_SKILLS.md` first. Treat its generated package identity, complete related-skill table, `$skill-name` invocations, descriptions, and access values as authoritative.
2. Read `Packages/com.actionfit.customsymbols/README.md` and `AI_GUIDE.md` when available. If downloaded, resolve `Library/PackageCache/com.actionfit.customsymbols@*` without editing it.
3. Explain `CustomSymbolsSO`, All/Windows/Mac/Android/iOS lists, `includedInBuild`, current-project initialization, and build-time apply/restore behavior.
4. Explain that an Editor-load bootstrap may create the default settings asset only when none exists; the audit itself must use direct read-only AssetDatabase lookup and must not trigger creation or remember a path in EditorPrefs.
5. List `Open Window`, `Setting SO`, and `README` under `Tools > Package > Custom Symbols`.

State that the audit reports differences only. It does not call `PlayerSettings.SetScriptingDefineSymbols`, run a build, apply exclusions, restore symbols, or edit any settings asset.
