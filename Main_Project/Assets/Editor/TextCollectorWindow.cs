using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public class TextCollectorWindow : EditorWindow
{
    static readonly Regex Hangul = new("[가-힣]");

    class SceneEntry { public string path; public bool inBuild; public bool selected; }

    class Item
    {
        public string assetPath;
        public bool isScene;
        public int[] indexPath;
        public string objectPath;
        public string text;
        public string key;
        public bool codeRef;
        public bool selected;
        public string Id => assetPath + "|" + string.Join("/", indexPath);
    }

    readonly List<SceneEntry> scenes = new();
    List<Item> items = new();
    Vector2 sceneScroll, itemScroll;
    bool showCodeRefOnly;
    string filter = "";

    [MenuItem("Tools/언어/문구 수집 (씬·프리팹)", priority = 0)]
    static void Open()
    {
        var w = GetWindow<TextCollectorWindow>("문구 수집");
        w.minSize = new Vector2(760, 560);
        w.Show();
    }

    void OnEnable() => RefreshSceneList();

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "선택한 씬과 그 씬이 쓰는 프리팹 원본에서 한글이 들어간 TMP 텍스트를 찾습니다.\n" +
            "스크립트가 참조하는 텍스트(코드가 실행 중에 글자를 바꿀 수 있는 것)는 기본으로 체크가 꺼져 있습니다. " +
            "여기에 번역을 붙이면 코드가 넣은 값이 덮어써질 수 있으니 확인 후 켜세요.\n" +
            "드롭다운 안 글자와 이미 번역이 연결된 텍스트는 목록에 나오지 않습니다.", MessageType.None);

        DrawScenes();
        EditorGUILayout.Space();
        DrawItems();
    }

    void DrawScenes()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("씬", EditorStyles.boldLabel);
            if (GUILayout.Button("빌드 씬만", GUILayout.Width(80))) scenes.ForEach(s => s.selected = s.inBuild);
            if (GUILayout.Button("해제", GUILayout.Width(50))) scenes.ForEach(s => s.selected = false);
            if (GUILayout.Button("새로고침", GUILayout.Width(70))) RefreshSceneList();
        }
        sceneScroll = EditorGUILayout.BeginScrollView(sceneScroll, GUILayout.Height(110));
        foreach (var s in scenes)
            s.selected = EditorGUILayout.ToggleLeft((s.inBuild ? "[빌드] " : "") + s.path.Substring("Assets/".Length), s.selected);
        EditorGUILayout.EndScrollView();

        using (new EditorGUI.DisabledScope(!scenes.Any(s => s.selected)))
            if (GUILayout.Button("스캔", GUILayout.Height(26))) Scan();
    }

    void RefreshSceneList()
    {
        var build = new HashSet<string>(EditorBuildSettings.scenes.Select(s => s.path));
        var previous = scenes.Where(s => s.selected).Select(s => s.path).ToHashSet();
        scenes.Clear();
        foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/TextMesh Pro/") || path.Contains("/Samples/")) continue;
            scenes.Add(new SceneEntry { path = path, inBuild = build.Contains(path), selected = previous.Contains(path) });
        }
        scenes.Sort((a, b) => a.inBuild != b.inBuild ? (a.inBuild ? -1 : 1) : string.CompareOrdinal(a.path, b.path));
    }

    void DrawItems()
    {
        if (items.Count == 0) { EditorGUILayout.LabelField("스캔 결과가 여기에 표시됩니다."); return; }

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"찾은 텍스트 {items.Count}개 (선택 {items.Count(i => i.selected)}개, 코드 참조 {items.Count(i => i.codeRef)}개)", EditorStyles.boldLabel);
            if (GUILayout.Button("전체 선택", GUILayout.Width(70))) items.ForEach(i => i.selected = true);
            if (GUILayout.Button("코드 참조 제외", GUILayout.Width(95))) items.ForEach(i => i.selected = !i.codeRef);
            if (GUILayout.Button("전체 해제", GUILayout.Width(70))) items.ForEach(i => i.selected = false);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            filter = EditorGUILayout.TextField("검색", filter);
            showCodeRefOnly = EditorGUILayout.ToggleLeft("코드 참조만 보기", showCodeRefOnly, GUILayout.Width(120));
        }

        itemScroll = EditorGUILayout.BeginScrollView(itemScroll);
        string lastAsset = null;
        foreach (var it in items)
        {
            if (showCodeRefOnly && !it.codeRef) continue;
            if (!string.IsNullOrEmpty(filter) && !(it.text.Contains(filter) || it.objectPath.Contains(filter) || it.assetPath.Contains(filter))) continue;

            if (it.assetPath != lastAsset)
            {
                lastAsset = it.assetPath;
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField((it.isScene ? "씬  " : "프리팹  ") + it.assetPath, EditorStyles.miniBoldLabel);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                it.selected = EditorGUILayout.Toggle(it.selected, GUILayout.Width(16));
                var oneLine = it.text.Replace("\n", " ⏎ ");
                var style = it.codeRef ? EditorStyles.boldLabel : EditorStyles.label;
                EditorGUILayout.LabelField(new GUIContent((it.codeRef ? "[코드] " : "") + oneLine, it.objectPath), style, GUILayout.MinWidth(260));
                it.key = EditorGUILayout.TextField(it.key, GUILayout.Width(200));
            }
        }
        EditorGUILayout.EndScrollView();

        using (new EditorGUI.DisabledScope(!items.Any(i => i.selected)))
        {
            if (GUILayout.Button($"선택한 {items.Count(i => i.selected)}개 문구 표에 추가하고 연결", GUILayout.Height(30)) &&
                EditorUtility.DisplayDialog("문구 연결", "선택한 텍스트를 'UI' 문구 표에 넣고 번역을 연결합니다. 씬과 프리팹이 저장됩니다.", "적용", "취소"))
                Apply();
        }
    }

    void Scan()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var setup = EditorSceneManager.GetSceneManagerSetup();
        var found = new List<Item>();
        var codeRefs = new HashSet<string>();
        var selectedScenes = scenes.Where(s => s.selected).Select(s => s.path).ToList();

        try
        {
            var prefabQueue = new Queue<string>();
            for (int i = 0; i < selectedScenes.Count; i++)
            {
                EditorUtility.DisplayProgressBar("문구 수집", selectedScenes[i], (float)i / selectedScenes.Count * 0.5f);
                var scene = EditorSceneManager.OpenScene(selectedScenes[i], OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                    ScanRoot(roots[r], selectedScenes[i], true, new[] { r }, found, codeRefs, prefabQueue);
            }

            var done = new HashSet<string>();
            while (prefabQueue.Count > 0)
            {
                var path = prefabQueue.Dequeue();
                if (!done.Add(path)) continue;
                EditorUtility.DisplayProgressBar("문구 수집", path, 0.5f + 0.5f * done.Count / Mathf.Max(1, done.Count + prefabQueue.Count));
                var root = PrefabUtility.LoadPrefabContents(path);
                try { ScanRoot(root, path, false, new int[0], found, codeRefs, prefabQueue); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (setup.Length > 0 && setup.All(s => !string.IsNullOrEmpty(s.path))) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        foreach (var it in found) { it.codeRef = codeRefs.Contains(it.Id); it.selected = !it.codeRef; }
        AssignKeys(found);
        items = found.OrderBy(i => i.isScene ? 0 : 1).ThenBy(i => i.assetPath).ToList();
        Debug.Log($"[TextCollector] 스캔 완료: 텍스트 {items.Count}개 (코드 참조 {items.Count(i => i.codeRef)}개)");
    }

    static void ScanRoot(GameObject root, string assetPath, bool isScene, int[] rootPath,
                         List<Item> found, HashSet<string> codeRefs, Queue<string> prefabQueue)
    {
        foreach (var text in LocalizationSetup.TextsIn(root))
        {
            if (PrefabUtility.IsPartOfPrefabInstance(text))
            {
                var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(text);
                var path = AssetDatabase.GetAssetPath(source);
                if (path.EndsWith(".prefab")) prefabQueue.Enqueue(path);
                continue;
            }
            var value = LocalizationSetup.GetText(text);
            if (value == null || !Hangul.IsMatch(value)) continue;
            if (LocalizationSetup.InDropdown(text)) continue;
            var ev = text.GetComponent<LocalizeStringEvent>();
            if (ev != null && !ev.StringReference.IsEmpty) continue;

            found.Add(new Item
            {
                assetPath = assetPath,
                isScene = isScene,
                indexPath = rootPath.Concat(IndexPath(text.transform, root.transform)).ToArray(),
                objectPath = NamePath(text.transform, root.transform),
                text = value,
            });
        }

        // 스크립트가 참조하는 텍스트는 코드가 글자를 바꿀 수 있음
        foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null || mb is LocalizeStringEvent) continue;
            if (!mb.GetType().Assembly.GetName().Name.StartsWith("Assembly-CSharp")) continue;

            var so = new SerializedObject(mb);
            var prop = so.GetIterator();
            while (prop.Next(true))
            {
                if (prop.propertyType != SerializedPropertyType.ObjectReference || prop.objectReferenceValue == null) continue;
                Component target = prop.objectReferenceValue as TMP_Text;
                if (target == null) target = prop.objectReferenceValue as Text;
                if (target == null && prop.objectReferenceValue is GameObject go)
                {
                    target = go.GetComponent<TMP_Text>();
                    if (target == null) target = go.GetComponent<Text>();
                }
                if (target == null) continue;
                var id = RefId(target, assetPath, isScene, root.transform);
                if (id != null) codeRefs.Add(id);
            }
        }
    }

    static string RefId(Component text, string assetPath, bool isScene, Transform prefabRoot)
    {
        if (PrefabUtility.IsPartOfPrefabInstance(text))
        {
            var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(text);
            var path = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrEmpty(path)) return null;
            return path + "|" + string.Join("/", IndexPath(source.transform, source.transform.root));
        }
        if (isScene)
        {
            var root = text.transform.root;
            var p = new List<int> { root.GetSiblingIndex() };
            p.AddRange(IndexPath(text.transform, root));
            return assetPath + "|" + string.Join("/", p);
        }
        return assetPath + "|" + string.Join("/", IndexPath(text.transform, prefabRoot));
    }

    static IEnumerable<int> IndexPath(Transform t, Transform root)
    {
        var stack = new Stack<int>();
        for (var c = t; c != null && c != root; c = c.parent) stack.Push(c.GetSiblingIndex());
        return stack;
    }

    static string NamePath(Transform t, Transform root)
    {
        var names = new Stack<string>();
        for (var c = t; c != null; c = c.parent) { names.Push(c.name); if (c == root) break; }
        return string.Join("/", names);
    }

    static void AssignKeys(List<Item> list)
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationSetup.TableName);
        var byText = new Dictionary<string, string>();
        var used = new HashSet<string>();
        if (collection != null)
        {
            var ko = collection.GetTable("ko") as StringTable;
            foreach (var e in collection.SharedData.Entries)
            {
                used.Add(e.Key);
                var v = ko?.GetEntry(e.Id)?.Value;
                if (!string.IsNullOrEmpty(v) && !byText.ContainsKey(v)) byText[v] = e.Key;
            }
        }

        var counters = new Dictionary<string, int>();
        foreach (var it in list)
        {
            if (byText.TryGetValue(it.text, out var existing)) { it.key = existing; continue; }

            var prefix = Regex.Replace(Path.GetFileNameWithoutExtension(it.assetPath).ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
            if (prefix.Length == 0) prefix = "text";
            counters.TryGetValue(prefix, out int n);
            string key;
            do { n++; key = $"{prefix}_{n:000}"; } while (used.Contains(key));
            counters[prefix] = n;
            used.Add(key);
            byText[it.text] = key;
            it.key = key;
        }
    }

    void Apply()
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationSetup.TableName);
        if (collection == null) { Debug.LogError("[TextCollector] 먼저 'Tools/언어/처음 설정'을 실행하세요."); return; }
        var ko = collection.GetTable("ko") as StringTable;

        var chosen = items.Where(i => i.selected && !string.IsNullOrWhiteSpace(i.key)).ToList();

        foreach (var it in chosen)
        {
            if (collection.SharedData.GetEntry(it.key) == null) collection.SharedData.AddKey(it.key);
            var entry = ko.GetEntry(it.key);
            if (entry == null) ko.AddEntry(it.key, it.text);
            else if (string.IsNullOrEmpty(entry.Value)) entry.Value = it.text;
        }
        EditorUtility.SetDirty(collection.SharedData);
        EditorUtility.SetDirty(ko);
        AssetDatabase.SaveAssets();

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var setup = EditorSceneManager.GetSceneManagerSetup();
        int linked = 0, skipped = 0;
        try
        {
            foreach (var group in chosen.GroupBy(i => i.assetPath))
            {
                EditorUtility.DisplayProgressBar("문구 연결", group.Key, 0.5f);
                if (group.First().isScene)
                {
                    var scene = EditorSceneManager.OpenScene(group.Key, OpenSceneMode.Single);
                    var roots = scene.GetRootGameObjects();
                    foreach (var it in group)
                    {
                        var t = it.indexPath[0] < roots.Length ? Resolve(roots[it.indexPath[0]].transform, it.indexPath.Skip(1)) : null;
                        if (LinkIfSame(t, it)) linked++; else skipped++;
                    }
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                else
                {
                    var root = PrefabUtility.LoadPrefabContents(group.Key);
                    try
                    {
                        foreach (var it in group)
                            if (LinkIfSame(Resolve(root.transform, it.indexPath), it)) linked++; else skipped++;
                        PrefabUtility.SaveAsPrefabAsset(root, group.Key);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (setup.Length > 0 && setup.All(s => !string.IsNullOrEmpty(s.path))) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        items.RemoveAll(i => chosen.Contains(i));
        Debug.Log($"[TextCollector] 연결 {linked}개{(skipped > 0 ? $", 건너뜀 {skipped}개 (스캔 후 글자나 계층이 바뀜, 다시 스캔하세요)" : "")}");
    }

    static Transform Resolve(Transform root, IEnumerable<int> path)
    {
        var t = root;
        foreach (var i in path)
        {
            if (t == null || i >= t.childCount) return null;
            t = t.GetChild(i);
        }
        return t;
    }

    static bool LinkIfSame(Transform t, Item it)
    {
        Component text = t != null ? t.GetComponent<TMP_Text>() : null;
        if (text == null && t != null) text = t.GetComponent<Text>();
        if (text == null || LocalizationSetup.GetText(text) != it.text) return false;
        LocalizationSetup.Link(text, it.key);
        return true;
    }
}
