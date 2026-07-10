using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditorInternal;
using UnityEngine;
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.

// IActiveBuildTargetChanged를 상속받아 플랫폼 변경을 감지합니다.
public class SymbolsWindow : EditorWindow, IActiveBuildTargetChanged
{
    [SerializeField]
    private CustomSymbolsSO _settingSO;
    private SerializedObject _serializedSettings;
    private Vector2 _scrollPosition = Vector2.zero;
    private ReorderableList _reorderableList;

    // 인터페이스 구현: 콜백 순서
    public int callbackOrder => 0;

    [MenuItem("Tools/Package/Custom Symbols/Open Window", false, 20)]
    public static void ShowWindow() => GetWindow<SymbolsWindow>("Custom Symbols").Show();

    private void OnEnable()
    {
        _settingSO = CustomSymbolsSO.FindOrCreateSettingsAsset();
        if (_settingSO != null)
        {
            _serializedSettings = new SerializedObject(_settingSO);
            InitReorderableList();
        }
    }

    // 플랫폼이 변경되었을 때 실행되는 콜백
    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        if (_settingSO == null)
        {
            _settingSO = CustomSymbolsSO.FindOrCreateSettingsAsset();
        }
        if (_settingSO != null)
        {
            ApplySymbolsToPlatform(newTarget);
            Debug.Log($"[Symbols] Platform changed: symbols updated for {newTarget}.");
        }
    }

    #region ReorderableList

    // ReorderableList 초기화
    private void InitReorderableList()
    {
        _reorderableList = new ReorderableList(
            _settingSO.customAllSymbols,
            typeof(CustomSymbolEntry),
            true, // 드래그 가능
            true, // 헤더 표시
            true, // 추가 버튼
            true  // 삭제 버튼
        );

        _reorderableList.drawHeaderCallback = DrawListHeader;
        _reorderableList.drawElementCallback = DrawListElement;
        _reorderableList.onAddCallback = OnListAdd;
        _reorderableList.onRemoveCallback = OnListRemove;
        _reorderableList.onReorderCallback = OnListReorder;
        _reorderableList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
    }

    // 테이블 헤더 렌더링
    private void DrawListHeader(Rect rect)
    {
        float toggleW = 40f;
        float nameW = rect.width - toggleW * 6;
        float x = rect.x;

        EditorGUI.LabelField(new Rect(x, rect.y, nameW, rect.height), "Symbol Name");
        x += nameW;
        EditorGUI.LabelField(new Rect(x, rect.y, toggleW, rect.height), "Build");
        x += toggleW;
        EditorGUI.LabelField(new Rect(x, rect.y, toggleW, rect.height), "All");
        x += toggleW;
        EditorGUI.LabelField(new Rect(x, rect.y, toggleW, rect.height), "Win");
        x += toggleW;
        EditorGUI.LabelField(new Rect(x, rect.y, toggleW, rect.height), "Mac");
        x += toggleW;
        EditorGUI.LabelField(new Rect(x, rect.y, toggleW, rect.height), "AOS");
        x += toggleW;
        EditorGUI.LabelField(new Rect(x, rect.y, toggleW, rect.height), "iOS");
    }

    // 각 심볼 행 렌더링
    private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (index >= _settingSO.customAllSymbols.Count) return;

        CustomSymbolEntry entry = _settingSO.customAllSymbols[index];
        float y = rect.y + 2;
        float h = EditorGUIUtility.singleLineHeight;
        float toggleW = 40f;
        float nameW = rect.width - toggleW * 6;
        float x = rect.x;

        // 심볼 이름
        EditorGUI.BeginChangeCheck();
        string newName = EditorGUI.TextField(new Rect(x, y, nameW - 5, h), entry.symbolName);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateSymbolNameEverywhere(entry.symbolName, newName);
            entry.symbolName = newName;
            EditorUtility.SetDirty(_settingSO);
        }
        x += nameW;

        // Build 토글
        EditorGUI.BeginChangeCheck();
        bool newBuild = EditorGUI.Toggle(new Rect(x + 12, y, 20, h), entry.includedInBuild);
        if (EditorGUI.EndChangeCheck())
        {
            entry.includedInBuild = newBuild;
            EditorUtility.SetDirty(_settingSO);
        }
        x += toggleW;

        // All 토글
        string symbol = entry.symbolName;
        bool isAll = IsSymbolInAllPlatforms(symbol);
        EditorGUI.BeginChangeCheck();
        bool nextAll = EditorGUI.Toggle(new Rect(x + 12, y, 20, h), isAll);
        if (EditorGUI.EndChangeCheck()) SetSymbolInAllPlatforms(symbol, nextAll);
        x += toggleW;

        // 개별 플랫폼 토글 (Win, Mac, AOS, iOS)
        DrawPlatformToggleRect(symbol, _settingSO.windowPlatformSymbols, new Rect(x + 12, y, 20, h));
        x += toggleW;
        DrawPlatformToggleRect(symbol, _settingSO.macPlatformSymbols, new Rect(x + 12, y, 20, h));
        x += toggleW;
        DrawPlatformToggleRect(symbol, _settingSO.aosPlatformSymbols, new Rect(x + 12, y, 20, h));
        x += toggleW;
        DrawPlatformToggleRect(symbol, _settingSO.iosPlatformSymbols, new Rect(x + 12, y, 20, h));
    }

    // Rect 기반 플랫폼 토글
    private void DrawPlatformToggleRect(string symbol, List<string> list, Rect rect)
    {
        bool exists = list.Contains(symbol);
        EditorGUI.BeginChangeCheck();
        bool nextVal = EditorGUI.Toggle(rect, exists);
        if (EditorGUI.EndChangeCheck())
        {
            if (nextVal)
                list.Add(symbol);
            else
            {
                list.Remove(symbol);
                _settingSO.allPlatformSymbols.Remove(symbol);
            }
            EditorUtility.SetDirty(_settingSO);
        }
    }

    // 심볼 추가 콜백
    private void OnListAdd(ReorderableList list)
    {
        _settingSO.customAllSymbols.Add(new CustomSymbolEntry { symbolName = "NEW_SYMBOL", includedInBuild = true });
        EditorUtility.SetDirty(_settingSO);
    }

    // 심볼 삭제 콜백
    private void OnListRemove(ReorderableList list)
    {
        if (list.index < 0 || list.index >= _settingSO.customAllSymbols.Count) return;
        string symbolToDelete = _settingSO.customAllSymbols[list.index].symbolName;
        RemoveSymbolFromEverywhere(symbolToDelete);
        _settingSO.customAllSymbols.RemoveAt(list.index);
        EditorUtility.SetDirty(_settingSO);
        AssetDatabase.SaveAssets();
    }

    // 순서 변경 콜백
    private void OnListReorder(ReorderableList list)
    {
        EditorUtility.SetDirty(_settingSO);
    }

    #endregion

    #region GUI

    private void OnGUI()
    {
        OnGUI_Settings();
        if (_settingSO == null) return;

        // ReorderableList가 없거나 리스트 참조가 달라진 경우 재초기화
        if (_reorderableList == null) InitReorderableList();

        EditorGUILayout.Space(10);

        // 기능 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("현재 프로젝트 심볼 가져오기", GUILayout.Height(30))) SyncFromProject();
        if (GUILayout.Button("모든 플랫폼에 심볼 적용", GUILayout.Height(30))) ApplySymbolsToAllPlatforms();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        // 메인 테이블
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        _reorderableList?.DoLayoutList();
        EditorGUILayout.EndScrollView();
    }

    // 설정 SO 선택 UI
    private void OnGUI_Settings()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        _settingSO = (CustomSymbolsSO)EditorGUILayout.ObjectField(_settingSO, typeof(CustomSymbolsSO), false);
        if (EditorGUI.EndChangeCheck() && _settingSO != null)
        {
            _serializedSettings = new SerializedObject(_settingSO);
            EditorPrefs.SetString(CustomSymbolsSO.SettingsPrefsKey, AssetDatabase.GetAssetPath(_settingSO));
            InitReorderableList();
        }
        if (GUILayout.Button("Create New", GUILayout.Width(80f)))
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Symbols Settings", "SymbolsSettings", "asset", "");
            if (!string.IsNullOrEmpty(path))
            {
                CustomSymbolsSO newSettings = CustomSymbolsSO.CreateSettingsAsset(path);
                if (newSettings != null)
                {
                    _settingSO = newSettings;
                    _serializedSettings = new SerializedObject(_settingSO);
                    InitReorderableList();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region Symbol Helpers

    // 심볼 이름 변경 시 모든 플랫폼 리스트에 동기화
    private void UpdateSymbolNameEverywhere(string oldName, string newName)
    {
        UpdateNameInList(_settingSO.allPlatformSymbols, oldName, newName);
        UpdateNameInList(_settingSO.windowPlatformSymbols, oldName, newName);
        UpdateNameInList(_settingSO.macPlatformSymbols, oldName, newName);
        UpdateNameInList(_settingSO.aosPlatformSymbols, oldName, newName);
        UpdateNameInList(_settingSO.iosPlatformSymbols, oldName, newName);
    }

    // 리스트 내 심볼 이름 교체
    private void UpdateNameInList(List<string> list, string oldName, string newName)
    {
        int idx = list.IndexOf(oldName);
        if (idx != -1) list[idx] = newName;
    }

    // 4개 플랫폼에 모두 포함되어 있는지 확인
    private bool IsSymbolInAllPlatforms(string symbol)
    {
        return _settingSO.allPlatformSymbols.Contains(symbol) ||
               (_settingSO.windowPlatformSymbols.Contains(symbol) &&
                _settingSO.macPlatformSymbols.Contains(symbol) &&
                _settingSO.aosPlatformSymbols.Contains(symbol) &&
                _settingSO.iosPlatformSymbols.Contains(symbol));
    }

    // All 체크박스 조작 시 4개 플랫폼 일괄 적용/해제
    private void SetSymbolInAllPlatforms(string symbol, bool activate)
    {
        if (activate)
        {
            if (!_settingSO.allPlatformSymbols.Contains(symbol)) _settingSO.allPlatformSymbols.Add(symbol);
            if (!_settingSO.windowPlatformSymbols.Contains(symbol)) _settingSO.windowPlatformSymbols.Add(symbol);
            if (!_settingSO.macPlatformSymbols.Contains(symbol)) _settingSO.macPlatformSymbols.Add(symbol);
            if (!_settingSO.aosPlatformSymbols.Contains(symbol)) _settingSO.aosPlatformSymbols.Add(symbol);
            if (!_settingSO.iosPlatformSymbols.Contains(symbol)) _settingSO.iosPlatformSymbols.Add(symbol);
        }
        else
        {
            _settingSO.allPlatformSymbols.Remove(symbol);
            _settingSO.windowPlatformSymbols.Remove(symbol);
            _settingSO.macPlatformSymbols.Remove(symbol);
            _settingSO.aosPlatformSymbols.Remove(symbol);
            _settingSO.iosPlatformSymbols.Remove(symbol);
        }
        EditorUtility.SetDirty(_settingSO);
    }

    // 모든 플랫폼 리스트에서 심볼 제거
    private void RemoveSymbolFromEverywhere(string symbol)
    {
        _settingSO.allPlatformSymbols.Remove(symbol);
        _settingSO.windowPlatformSymbols.Remove(symbol);
        _settingSO.macPlatformSymbols.Remove(symbol);
        _settingSO.aosPlatformSymbols.Remove(symbol);
        _settingSO.iosPlatformSymbols.Remove(symbol);
    }

    #endregion

    #region Sync & Apply

    // 모든 플랫폼의 심볼을 수집하여 customAllSymbols에 등록
    private void SyncFromProject()
    {
        var allTargets = new[]
        {
            NamedBuildTarget.Standalone,
            NamedBuildTarget.Android,
            NamedBuildTarget.iOS,
        };

        HashSet<string> allSymbols = new HashSet<string>();

        // PlayerSettings에서 모든 플랫폼 심볼 수집
        foreach (var namedTarget in allTargets)
        {
            PlayerSettings.GetScriptingDefineSymbols(namedTarget, out string[] symbols);
            foreach (var s in symbols)
            {
                if (!string.IsNullOrEmpty(s)) allSymbols.Add(s);
            }
        }

        // SO 내부 플랫폼 리스트에서도 수집
        foreach (var s in _settingSO.allPlatformSymbols) allSymbols.Add(s);
        foreach (var s in _settingSO.windowPlatformSymbols) allSymbols.Add(s);
        foreach (var s in _settingSO.macPlatformSymbols) allSymbols.Add(s);
        foreach (var s in _settingSO.aosPlatformSymbols) allSymbols.Add(s);
        foreach (var s in _settingSO.iosPlatformSymbols) allSymbols.Add(s);

        // customAllSymbols에 없는 심볼 추가
        foreach (var symbol in allSymbols)
        {
            if (_settingSO.customAllSymbols.All(e => e.symbolName != symbol))
            {
                _settingSO.customAllSymbols.Add(new CustomSymbolEntry { symbolName = symbol, includedInBuild = true });
            }
        }

        EditorUtility.SetDirty(_settingSO);
    }

    // 모든 플랫폼에 심볼 일괄 적용
    private void ApplySymbolsToAllPlatforms()
    {
        var targets = new[]
        {
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneOSX,
            BuildTarget.Android,
            BuildTarget.iOS,
        };

        foreach (var target in targets)
        {
            ApplySymbolsToPlatform(target);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Symbols] Applied to all platforms.");
    }

    // 에디터용 심볼 적용 (includedInBuild 무관, 플랫폼에 등록된 모든 심볼 적용)
    private void ApplySymbolsToPlatform(BuildTarget target)
    {
        var namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
        List<string> resultSymbols = _settingSO.GetPlatformSymbols(target);

        PlayerSettings.SetScriptingDefineSymbols(namedTarget, resultSymbols.Distinct().ToArray());
    }

    #endregion
}
