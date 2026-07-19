using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActionFit.SOSingleton;
using ActionFit.SOSingleton.Editor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

[CreateAssetMenu(fileName = "SymbolsSettings", menuName = "CustomEditor/CustomSymbols")]
[ActionFitSettingsAsset(
    "CustomSymbols",
    ActionFitSettingsAssetLifetime.EditorOnly,
    LegacyPaths = new string[]
    {
        "Assets/_Data/_CustomSymbols/SymbolsSettings.asset"
    })]
public class CustomSymbolsSO : ScriptableObject, IActionFitSettingsAssetInitializer
{
    [Header("Custom All Symbols")] // 등록된 모든 심볼을 저장
    public List<CustomSymbolEntry> customAllSymbols = new();

    [Header("All Platform Symbols")] // 플랫폼 상관 없이 적용할 심볼
    public List<string> allPlatformSymbols = new();

    [Header("Window Platform Symbols")] // 윈도우 플랫폼에 적용할 심볼
    public List<string> windowPlatformSymbols = new();

    [Header("Mac Platform Symbols")] // 맥 플랫폼에 적용할 심볼
    public List<string> macPlatformSymbols = new();

    [Header("AOS Platform Symbols")] // AOS 플랫폼에 적용할 심볼
    public List<string> aosPlatformSymbols = new();

    [Header("IOS Platform Symbols")] // IOS 플랫폼에 적용할 심볼
    public List<string> iosPlatformSymbols = new();

#if UNITY_EDITOR

    #region Build Utility

    public const string SettingsPrefsKey = "LastUsedSettingPath";
    public const string DefaultSettingsAssetPath = "Assets/_Data/_CustomSymbols/SymbolsSettings.asset";

    /// <summary>
    /// AssetDatabase에서 CustomSymbolsSO 에셋을 검색하여 반환합니다.
    /// EditorPrefs 경로, 기본 _Data 경로, 타입 검색 순서로 확인합니다.
    /// </summary>
    public static CustomSymbolsSO FindSettingsAsset()
    {
        var savedSettings = LoadAndRemember(EditorPrefs.GetString(SettingsPrefsKey, ""));
        if (savedSettings != null) return savedSettings;

        var result = ActionFitSettingsAssetProvider.Resolve(typeof(CustomSymbolsSO), false);
        return LoadAndRemember(result.ActualPath);
    }

    /// <summary>
    /// 기존 CustomSymbolsSO를 반환하거나 기본 _Data 경로에 현재 프로젝트 심볼로 초기화된 에셋을 생성합니다.
    /// </summary>
    public static CustomSymbolsSO FindOrCreateSettingsAsset()
    {
        var settings = ActionFitSettingsAssetProvider.GetOrCreate<CustomSymbolsSO>();
        return settings == null
            ? null
            : LoadAndRemember(AssetDatabase.GetAssetPath(settings));
    }

    public void InitializeNewSettingsAsset()
    {
        InitializeFromCurrentProjectSymbols();
    }

    internal static CustomSymbolsSO CreateSettingsAsset(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            UnityEngine.Debug.LogError("[CustomSymbolsSO] Settings asset path is empty.");
            return null;
        }

        UnityEngine.Object existingAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (existingAsset != null)
        {
            UnityEngine.Debug.LogError($"[CustomSymbolsSO] Cannot create settings asset because the path is already in use: {assetPath}");
            return null;
        }

        EnsureFolder(Path.GetDirectoryName(assetPath)?.Replace("\\", "/"));

        var settings = CreateInstance<CustomSymbolsSO>();
        settings.InitializeFromCurrentProjectSymbols();
        AssetDatabase.CreateAsset(settings, assetPath);
        EditorPrefs.SetString(SettingsPrefsKey, assetPath);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UnityEngine.Debug.Log($"[CustomSymbolsSO] Settings asset created: {assetPath}");
        return settings;
    }

    internal void InitializeFromCurrentProjectSymbols()
    {
        InitializeFromPlatformSymbols(
            GetProjectSymbols(NamedBuildTarget.Standalone),
            GetProjectSymbols(NamedBuildTarget.Android),
            GetProjectSymbols(NamedBuildTarget.iOS));
    }

    internal void InitializeFromPlatformSymbols(
        IEnumerable<string> standaloneSymbols,
        IEnumerable<string> androidSymbols,
        IEnumerable<string> iosSymbols)
    {
        List<string> standalone = NormalizeSymbols(standaloneSymbols);
        List<string> android = NormalizeSymbols(androidSymbols);
        List<string> ios = NormalizeSymbols(iosSymbols);

        customAllSymbols ??= new List<CustomSymbolEntry>();
        allPlatformSymbols ??= new List<string>();
        windowPlatformSymbols ??= new List<string>();
        macPlatformSymbols ??= new List<string>();
        aosPlatformSymbols ??= new List<string>();
        iosPlatformSymbols ??= new List<string>();

        customAllSymbols.Clear();
        allPlatformSymbols.Clear();
        windowPlatformSymbols.Clear();
        macPlatformSymbols.Clear();
        aosPlatformSymbols.Clear();
        iosPlatformSymbols.Clear();

        var commonSymbols = new HashSet<string>(standalone, StringComparer.Ordinal);
        commonSymbols.IntersectWith(android);
        commonSymbols.IntersectWith(ios);

        allPlatformSymbols.AddRange(commonSymbols.OrderBy(symbol => symbol, StringComparer.Ordinal));
        windowPlatformSymbols.AddRange(standalone);
        macPlatformSymbols.AddRange(standalone);
        aosPlatformSymbols.AddRange(android);
        iosPlatformSymbols.AddRange(ios);

        IEnumerable<string> allSymbols = standalone
            .Concat(android)
            .Concat(ios)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(symbol => symbol, StringComparer.Ordinal);
        customAllSymbols.AddRange(allSymbols.Select(symbol => new CustomSymbolEntry
        {
            symbolName = symbol,
            includedInBuild = true
        }));
    }

    private static IEnumerable<string> GetProjectSymbols(NamedBuildTarget target)
    {
        PlayerSettings.GetScriptingDefineSymbols(target, out string[] symbols);
        return symbols ?? Array.Empty<string>();
    }

    private static List<string> NormalizeSymbols(IEnumerable<string> symbols)
    {
        return (symbols ?? Enumerable.Empty<string>())
            .Select(symbol => symbol?.Trim())
            .Where(symbol => !string.IsNullOrEmpty(symbol))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(symbol => symbol, StringComparer.Ordinal)
            .ToList();
    }

    private static CustomSymbolsSO LoadAndRemember(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath)) return null;

        var settings = AssetDatabase.LoadAssetAtPath<CustomSymbolsSO>(assetPath);
        if (settings != null)
            EditorPrefs.SetString(SettingsPrefsKey, assetPath);

        return settings;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath)) return;

        string parentPath = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        EnsureFolder(parentPath);
        AssetDatabase.CreateFolder(parentPath, Path.GetFileName(folderPath));
    }

    // 현재 플랫폼에 해당하는 전체 심볼 목록 반환 (allPlatform + 플랫폼별)
    public List<string> GetPlatformSymbols(BuildTarget target)
    {
        List<string> result = new List<string>(allPlatformSymbols);

        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                result.AddRange(windowPlatformSymbols);
                break;
            case BuildTarget.StandaloneOSX:
                result.AddRange(macPlatformSymbols);
                break;
            case BuildTarget.Android:
                result.AddRange(aosPlatformSymbols);
                break;
            case BuildTarget.iOS:
                result.AddRange(iosPlatformSymbols);
                break;
        }

        return result.Distinct().ToList();
    }

    // Build 체크가 해제된 심볼 목록 반환 (해당 플랫폼에 등록되어 있지만 Build 미체크)
    public List<string> GetExcludedSymbols(BuildTarget target)
    {
        List<string> platformSymbols = GetPlatformSymbols(target);
        HashSet<string> excluded = new HashSet<string>(
            customAllSymbols
                .Where(e => !e.includedInBuild)
                .Select(e => e.symbolName)
        );

        return platformSymbols.Where(s => excluded.Contains(s)).ToList();
    }

    // Build 체크 해제된 심볼만 제외한 최종 심볼 리스트 반환
    public List<string> GetBuildSymbols(BuildTarget target)
    {
        List<string> platformSymbols = GetPlatformSymbols(target);
        HashSet<string> excluded = new HashSet<string>(
            customAllSymbols
                .Where(e => !e.includedInBuild)
                .Select(e => e.symbolName)
        );

        return platformSymbols.Where(s => !excluded.Contains(s)).ToList();
    }

    #endregion

#endif
}

[Serializable]
public class CustomSymbolEntry
{
    public string symbolName; // 심볼 이름
    public bool includedInBuild = true; // 빌드 포함 여부
}
