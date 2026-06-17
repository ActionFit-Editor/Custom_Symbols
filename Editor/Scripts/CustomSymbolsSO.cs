using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SymbolsSettings", menuName = "CustomEditor/CustomSymbols")]
public class CustomSymbolsSO : ScriptableObject
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

    /// <summary>
    /// AssetDatabase에서 CustomSymbolsSO 에셋을 검색하여 반환합니다.
    /// EditorPrefs에 저장된 경로를 우선 사용하고, 없으면 AssetDatabase에서 검색합니다.
    /// </summary>
    public static CustomSymbolsSO FindSettingsAsset()
    {
        // SymbolsWindow에서 저장한 경로 우선 확인
        string savedPath = EditorPrefs.GetString("LastUsedSettingPath", "");
        if (!string.IsNullOrEmpty(savedPath))
        {
            var asset = AssetDatabase.LoadAssetAtPath<CustomSymbolsSO>(savedPath);
            if (asset != null) return asset;
        }

        // 경로가 없거나 유효하지 않으면 AssetDatabase 검색
        string[] guids = AssetDatabase.FindAssets("t:CustomSymbolsSO");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<CustomSymbolsSO>(path);
        }

        return null;
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
