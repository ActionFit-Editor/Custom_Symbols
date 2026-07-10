#if UNITY_EDITOR

using UnityEditor;

[InitializeOnLoad]
public static class CustomSymbolsSettingsBootstrap
{
    static CustomSymbolsSettingsBootstrap()
    {
        EditorApplication.delayCall += EnsureSettingsAsset;
    }

    private static void EnsureSettingsAsset()
    {
        CustomSymbolsSO.FindOrCreateSettingsAsset();
    }
}

#endif
