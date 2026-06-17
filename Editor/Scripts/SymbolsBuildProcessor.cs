using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 빌드 직전에 에디터 심볼과 빌드 심볼의 차이를 표시하고,
/// includedInBuild가 false인 심볼을 제거합니다.
/// 빌드 완료 후 에디터용 전체 심볼을 복원합니다.
/// </summary>
public class SymbolsBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    // 빌드 시 심볼 관리 활성 여부 확인
    private static bool IsSymbolManageEnabled()
    {
        string[] guids = AssetDatabase.FindAssets("t:BuildSettingsSO");
        if (guids.Length == 0) return true;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var buildSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (buildSettings == null) return true;
        var so = new SerializedObject(buildSettings);
        var prop = so.FindProperty("manageSymbolsOnBuild");
        return prop == null || prop.boolValue;
    }

    // 빌드 직전: 차이 표시 후 빌드 심볼 적용
    public void OnPreprocessBuild(BuildReport report)
    {
        if (!IsSymbolManageEnabled()) return;

        CustomSymbolsSO settings = CustomSymbolsSO.FindSettingsAsset();
        if (settings == null) return;

        BuildTarget target = report.summary.platform;
        var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));

        List<string> editorSymbols = settings.GetPlatformSymbols(target);
        List<string> buildSymbols = settings.GetBuildSymbols(target);

        // 에디터에만 있고 빌드에는 제외되는 심볼
        List<string> editorOnly = editorSymbols.Where(s => !buildSymbols.Contains(s)).ToList();
        // 빌드에만 있고 에디터에는 없는 심볼 (정상적으로는 발생하지 않음)
        List<string> buildOnly = buildSymbols.Where(s => !editorSymbols.Contains(s)).ToList();

        if (editorOnly.Count > 0 || buildOnly.Count > 0)
        {
            string message = $"[{target}] 에디터 심볼과 빌드 심볼이 다릅니다.\n\n";

            if (editorOnly.Count > 0)
                message += $"[빌드에서 해제될 예정]\n  {string.Join("\n  ", editorOnly)}\n\n";

            if (buildOnly.Count > 0)
                message += $"[빌드에서 포함될 예정]\n  {string.Join("\n  ", buildOnly)}\n\n";

            message += $"빌드 적용 심볼 ({buildSymbols.Count}개):\n  {string.Join("\n  ", buildSymbols)}";

            bool proceed = EditorUtility.DisplayDialog(
                "Custom Symbols - Build",
                message,
                "빌드 진행",
                "빌드 취소"
            );

            if (!proceed)
                throw new BuildFailedException("[Symbols] Build cancelled by user.");
        }

        PlayerSettings.SetScriptingDefineSymbols(namedTarget, buildSymbols.ToArray());
        Debug.Log($"[Symbols] Build pre-process: applied build symbols for {target}");
    }

    // 빌드 완료 후: 에디터용 전체 심볼 복원
    public void OnPostprocessBuild(BuildReport report)
    {
        if (!IsSymbolManageEnabled()) return;
        RestoreEditorSymbols(report.summary.platform);
    }

    // 에디터 심볼 복원
    private static void RestoreEditorSymbols(BuildTarget target)
    {
        CustomSymbolsSO settings = CustomSymbolsSO.FindSettingsAsset();
        if (settings == null) return;

        var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
        List<string> symbols = settings.GetPlatformSymbols(target);

        PlayerSettings.SetScriptingDefineSymbols(namedTarget, symbols.ToArray());
        Debug.Log($"[Symbols] Build post-process: restored editor symbols for {target}");
    }
}
