# AI Guide - Custom Symbols

This file is shipped inside the UPM package so an AI assistant in a consuming Unity project can understand the package without access to the source project's `Docs/AI` folder.

## Package Identity

- Package ID: `com.actionfit.customsymbols`
- Display name: Custom Symbols
- Repository: `https://github.com/ActionFit-Editor/Custom_Symbols.git`
- Current package version at generation time: `1.0.9`
- Unity version: `6000.2`

## Purpose

### Settings SO Lifecycle

- `CustomSymbolsSO` is registered as `EditorOnly`; canonical path is `Assets/_Data/_CustomSymbols/CustomSymbolsSO.asset`.
- Existing `Assets/_Data/_CustomSymbols/SymbolsSettings.asset` is a declared legacy candidate and remains in place.
- A newly created asset initializes from current PlayerSettings symbols. Existing configured symbols are never reset by lifecycle resolution.

Custom Symbols manages scripting define symbols. Use `README.md`, `package.json`, package source files, and `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` together to understand the user-facing workflow and catalog metadata.

## Project Router Registration

This package should be listed in `Packages/com.actionfit.custompackagemanager/PACKAGE_AI_GUIDE_ROUTER.md`.

Requested router entry:

- `Packages/com.actionfit.customsymbols/AI_GUIDE.md` - Custom Symbols manages scripting define symbols. Read when changing platform symbol presets, build inclusion rules, or symbol settings assets.

If the router file is not already included in the AI assistant's default reading sequence, the router file is responsible for asking the user to link it from `Docs/AI/PROJECT.md` when available, or otherwise from `AGENTS.md`, `CLAUDE.md`, or another primary AI markdown entry point.

Read this file when:

- changing files under `Packages/com.actionfit.customsymbols/`
- diagnosing `Custom Symbols` behavior in a consuming project
- preparing a release for `com.actionfit.customsymbols`
- editing package metadata, README, AI guide, package version, or release notes

## Required Reading For AI

- Read this `AI_GUIDE.md` before changing, diagnosing, or explaining this package.
- Read `Packages/com.actionfit.custompackagemanager/PACKAGE_AI_GUIDE_ROUTER.md` when deciding which installed ActionFit package `AI_GUIDE.md` applies to a task.
- Read `README.md` for human-facing setup and usage.
- Read `package.json` for package ID, version, Unity version, and dependencies.
- Read `Editor/PackageInfo/ActionFitPackageInfo_SO.asset` for catalog metadata, repository name, owner, status, description, release note, and dependency override.

## Editing Rules

- Keep changes scoped to this package unless the user explicitly asks for cross-package edits.
- Do not change package IDs, repository names, public menu paths, serialized field names, or package assembly names casually; these can affect installed projects.
- Preserve Unity `.meta` files when adding, moving, or renaming files inside the package.
- When behavior changes, update this `AI_GUIDE.md` in the same package before publishing so consuming projects receive the latest AI context.
- Keep `README.md` focused on human usage. Keep this file focused on AI-facing architecture, constraints, migration notes, and package-specific editing rules.

## Menu And Behavior Notes

- Main menu: `Tools/Package/Custom Symbols/Open Window`.
- If no `CustomSymbolsSO` exists anywhere in the project, the editor bootstrap creates `Assets/_Data/_CustomSymbols/SymbolsSettings.asset`. Existing assets at any location are preserved and prevent default creation.
- First creation reads the current Standalone, Android, and iOS scripting define symbols. Every discovered symbol starts with `includedInBuild=true`; Standalone maps to Win/Mac, Android maps to AOS, iOS maps to iOS, and the intersection is also stored in `allPlatformSymbols`.
- `FindOrCreateSettingsAsset()` is the shared entry point for the bootstrap, settings menu, window, build processor, and Build Automation reflection bridge. Manual `Create New` assets use the same current-project initialization.
- Use this guide when changing scripting define symbol presets, platform filters, build inclusion rules, or symbol settings assets.
- Symbol changes can affect compilation and build output, so verify target platform behavior after changes.
- `SymbolsBuildProcessor` must not show blocking dialogs in `Application.isBatchMode`; CI/AutoBuild should log symbol differences and continue with the computed build symbols. Keep the interactive confirmation dialog for normal editor builds.

## Agent Skills

- `Skills~/manifest.json` uses schema v2 with the unique `custom-symbols` prefix.
- `custom-symbols-help` and `custom-symbols-audit` are read-only for Codex and Claude.
- Audits must use direct `AssetDatabase.FindAssets`/load queries instead of `FindSettingsAsset`, which remembers a path in `EditorPrefs`.
- Audits may compare existing settings with `PlayerSettings.GetScriptingDefineSymbols`; they must not create settings, call any setter, switch targets, invoke the build processor, or build.
- The installed help skill must read generated `PACKAGE_SKILLS.md`; do not author that reserved file in package sources.

## Package Tools Menu

- Unity menu root: `Tools/Package/Custom Symbols/`.
- Keep package commands under this package root.
- Lower separated entries:
- `Setting SO`: creates the default settings asset when none exists, then focuses it.
- `README`: opens this package README.
- Do not add README or Setting SO access back to Custom Package Manager package rows or Project Files.

## Release Note Rules

- `ActionFitPackageInfo_SO.ReleaseNote` must contain only the single version being prepared.
- Do not copy older changelog entries into the newest release note.
- Version history and update-range summaries are composed by Custom Package Manager from separate catalog version rows.
- Do not add headings such as `## 1.0.0` inside ReleaseNote unless a specific package UI requires it; the catalog row already carries the version.
## Publish Notes

- Publishing is manual through Custom Package Manager.
- Before reusing a version, check the remote Git tags. Published tags are immutable.
- If this package is modified after a version was tagged, bump to the next unused patch version before publishing.
- The package repository should include this `AI_GUIDE.md` so other projects can load the AI package context after installing the package.
