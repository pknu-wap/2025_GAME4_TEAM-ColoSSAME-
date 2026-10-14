using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;

public class FontToolWindow : EditorWindow
{
    const string FontFolder = "Assets/Ui/Font";
    const string DefaultCharsetPath = FontFolder + "/charset_ko.txt";
    const string UnderlaySuffix = "_UNDERLAY";

    enum Tab { Install, Apply }
    Tab tab;
    Vector2 scroll;

    Font sourceFont;
    TextAsset charset;
    int samplingSize = 64;
    int atlasSize = 4096;
    TMP_FontAsset fallback;

    TMP_FontAsset targetFont;
    bool onlyLocalized;
    bool setAsDefault;
    readonly List<SceneEntry> scenes = new();

    class SceneEntry { public string path; public bool inBuild; public bool selected; }

    [MenuItem("Tools/폰트/폰트 설치", priority = 0)]
    static void OpenInstall() => Open(Tab.Install);

    [MenuItem("Tools/폰트/폰트 적용", priority = 1)]
    static void OpenApply() => Open(Tab.Apply);

    static void Open(Tab t)
    {
        var w = GetWindow<FontToolWindow>("폰트 도구");
        w.tab = t;
        w.minSize = new Vector2(420, 480);
        w.Show();
    }

    void OnEnable()
    {
        if (charset == null) charset = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultCharsetPath);
        if (fallback == null) fallback = FindFontAsset("DNFBitBitv2 SDF");
        if (targetFont == null) targetFont = TMP_Settings.defaultFontAsset;
        RefreshSceneList();
    }

    void OnGUI()
    {
        tab = (Tab)GUILayout.Toolbar((int)tab, new[] { "폰트 설치", "폰트 적용" });
        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (tab == Tab.Install) DrawInstall(); else DrawApply();
        EditorGUILayout.EndScrollView();
    }

    void DrawInstall()
    {
        EditorGUILayout.HelpBox("TTF/OTF 폰트로 TMP 폰트 에셋을 만듭니다. 글자 목록에 있는 글자만 아틀라스에 고정으로 들어갑니다.", MessageType.None);

        using (new EditorGUILayout.HorizontalScope())
        {
            sourceFont = (Font)EditorGUILayout.ObjectField("폰트 파일", sourceFont, typeof(Font), false);
            if (GUILayout.Button("PC에서 가져오기", GUILayout.Width(110))) ImportFontFile();
        }
        charset = (TextAsset)EditorGUILayout.ObjectField("글자 목록", charset, typeof(TextAsset), false);
        samplingSize = EditorGUILayout.IntPopup("글자 크기(샘플링)", samplingSize,
            new[] { "48 (작게, 아틀라스 여유)", "64 (기본)", "80 (선명, 아틀라스 꽉 참)" }, new[] { 48, 64, 80 });
        atlasSize = EditorGUILayout.IntPopup("아틀라스 크기", atlasSize, new[] { "2048", "4096" }, new[] { 2048, 4096 });
        fallback = (TMP_FontAsset)EditorGUILayout.ObjectField(new GUIContent("없는 글자 대체 폰트", "이 폰트에 없는 기호 등을 가져올 폰트"),
            fallback, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(sourceFont == null || charset == null))
        {
            if (GUILayout.Button("폰트 에셋 만들기", GUILayout.Height(30)))
            {
                var created = CreateFontAsset(sourceFont, charset.text, samplingSize, atlasSize, fallback);
                if (created != null) { targetFont = created; EditorGUIUtility.PingObject(created); }
            }
        }
    }

    void ImportFontFile()
    {
        var file = EditorUtility.OpenFilePanel("폰트 파일 선택", "", "ttf,otf");
        if (string.IsNullOrEmpty(file)) return;

        Directory.CreateDirectory(FontFolder);
        var dest = $"{FontFolder}/{Path.GetFileName(file)}";
        var fullDest = Path.GetFullPath(dest);
        if (Path.GetFullPath(file) != fullDest)
        {
            if (File.Exists(fullDest) && !EditorUtility.DisplayDialog("폰트 가져오기", $"{dest}가 이미 있습니다. 덮어쓸까요?", "덮어쓰기", "취소"))
                return;
            File.Copy(file, fullDest, true);
        }
        AssetDatabase.ImportAsset(dest);
        sourceFont = AssetDatabase.LoadAssetAtPath<Font>(dest);
    }

    public static TMP_FontAsset CreateFontAsset(Font font, string chars, int sampling, int atlas, TMP_FontAsset fallbackFont)
    {
        var fontPath = AssetDatabase.GetAssetPath(font);
        var assetPath = $"{Path.GetDirectoryName(fontPath).Replace('\\', '/')}/{Path.GetFileNameWithoutExtension(fontPath)} SDF.asset";

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
        {
            if (!EditorUtility.DisplayDialog("폰트 에셋 만들기", $"{assetPath}가 이미 있습니다. 새로 만들까요?\n(이 폰트를 쓰던 텍스트는 연결이 끊길 수 있습니다)", "새로 만들기", "취소"))
                return null;
            AssetDatabase.DeleteAsset(assetPath);
        }

        // Static으로 저장해야 실행 중에 에셋이 안 바뀜
        var fontAsset = TMP_FontAsset.CreateFontAsset(font, sampling, Mathf.Max(4, sampling / 10), GlyphRenderMode.SDFAA, atlas, atlas, AtlasPopulationMode.Dynamic, true);
        fontAsset.name = Path.GetFileNameWithoutExtension(assetPath);
        AssetDatabase.CreateAsset(fontAsset, assetPath);

        fontAsset.TryAddCharacters(chars, out string missing);
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

        for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
        {
            var tex = fontAsset.atlasTextures[i];
            tex.name = $"{fontAsset.name} Atlas{(i == 0 ? "" : " " + i)}";
            AssetDatabase.AddObjectToAsset(tex, fontAsset);
        }
        fontAsset.material.name = fontAsset.name + " Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        if (fallbackFont != null && fallbackFont != fontAsset)
            fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallbackFont };

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        CreateUnderlayMaterial(fontAsset, assetPath);

        var missingChars = string.IsNullOrEmpty(missing) ? "" : new string(missing.Distinct().ToArray());
        Debug.Log($"[FontTool] 폰트 에셋 생성: {assetPath} (아틀라스 {fontAsset.atlasTextures.Length}장, 폰트에 없는 글자 {missingChars.Length}개{(missingChars.Length > 0 ? ": " + missingChars : "")})");
        if (fontAsset.atlasTextures.Length > 1)
            Debug.LogWarning("[FontTool] 글자가 아틀라스 한 장에 다 안 들어가서 여러 장이 됐습니다. 글자 크기를 줄이거나 아틀라스를 키우세요.");
        return fontAsset;
    }

    static void CreateUnderlayMaterial(TMP_FontAsset fontAsset, string assetPath)
    {
        var source = FindFontAsset("DNFBitBitv2 SDF" + UnderlaySuffix);
        if (source == null) return;

        var matPath = UnderlayMaterialPath(assetPath);
        var mat = new Material(source.material) { name = Path.GetFileNameWithoutExtension(matPath) };
        mat.SetTexture(ShaderUtilities.ID_MainTex, fontAsset.atlasTexture);
        mat.SetFloat(ShaderUtilities.ID_TextureWidth, fontAsset.atlasWidth);
        mat.SetFloat(ShaderUtilities.ID_TextureHeight, fontAsset.atlasHeight);
        mat.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding + 1);

        AssetDatabase.DeleteAsset(matPath);
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
    }

    static string UnderlayMaterialPath(string fontAssetPath) =>
        fontAssetPath.Substring(0, fontAssetPath.Length - ".asset".Length) + UnderlaySuffix + ".mat";

    static TMP_FontAsset FindFontAsset(string name) =>
        AssetDatabase.FindAssets($"{name} t:TMP_FontAsset")
            .Select(g => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g)))
            .FirstOrDefault(f => f != null && f.name == name);

    void DrawApply()
    {
        EditorGUILayout.HelpBox("선택한 씬의 TMP 텍스트 폰트를 바꿉니다. 씬에 들어 있는 프리팹은 원본 프리팹을 고치므로, 그 프리팹을 쓰는 다른 씬에도 같이 적용됩니다.", MessageType.None);

        targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("적용할 폰트", targetFont, typeof(TMP_FontAsset), false);
        onlyLocalized = EditorGUILayout.ToggleLeft("번역이 연결된 텍스트만 (Localize String Event가 붙은 것 + 드롭다운 글자)", onlyLocalized);
        setAsDefault = EditorGUILayout.ToggleLeft("새로 만드는 TMP 텍스트의 기본 폰트로도 설정", setAsDefault);

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("씬", EditorStyles.boldLabel);
            if (GUILayout.Button("빌드 씬만", GUILayout.Width(80))) scenes.ForEach(s => s.selected = s.inBuild);
            if (GUILayout.Button("전체", GUILayout.Width(50))) scenes.ForEach(s => s.selected = true);
            if (GUILayout.Button("해제", GUILayout.Width(50))) scenes.ForEach(s => s.selected = false);
            if (GUILayout.Button("새로고침", GUILayout.Width(70))) RefreshSceneList();
        }
        foreach (var s in scenes)
            s.selected = EditorGUILayout.ToggleLeft((s.inBuild ? "[빌드] " : "") + s.path.Substring("Assets/".Length), s.selected);

        EditorGUILayout.Space();
        int count = scenes.Count(s => s.selected);
        using (new EditorGUI.DisabledScope(targetFont == null || count == 0))
        {
            if (GUILayout.Button($"선택한 씬 {count}개에 적용", GUILayout.Height(30)) &&
                EditorUtility.DisplayDialog("폰트 적용", $"'{(targetFont ? targetFont.name : "")}'를 씬 {count}개와 그 안의 프리팹에 적용합니다.\n열려 있는 씬에 저장 안 한 변경이 있으면 먼저 저장 여부를 묻습니다.", "적용", "취소"))
            {
                Apply(targetFont, scenes.Where(s => s.selected).Select(s => s.path).ToList(), onlyLocalized, setAsDefault);
            }
        }
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

    static void Apply(TMP_FontAsset font, List<string> scenePaths, bool onlyLocalized, bool setAsDefault)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var setup = EditorSceneManager.GetSceneManagerSetup();
        var underlay = AssetDatabase.LoadAssetAtPath<Material>(UnderlayMaterialPath(AssetDatabase.GetAssetPath(font)));
        var legacyFont = new SerializedObject(font).FindProperty("m_SourceFontFile_EditorRef")?.objectReferenceValue as Font;

        try
        {
            var prefabPaths = new HashSet<string>();
            for (int i = 0; i < scenePaths.Count; i++)
            {
                EditorUtility.DisplayProgressBar("폰트 적용", $"프리팹 찾는 중: {scenePaths[i]}", (float)i / scenePaths.Count * 0.3f);
                var scene = EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Single);
                foreach (var go in scene.GetRootGameObjects())
                    CollectPrefabSources(go, prefabPaths, onlyLocalized);
            }

            var done = new HashSet<string>();
            var queue = new Queue<string>(prefabPaths);
            int prefabTexts = 0;
            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                if (!done.Add(path)) continue;
                EditorUtility.DisplayProgressBar("폰트 적용", $"프리팹: {path}", 0.3f + 0.4f * done.Count / Mathf.Max(1, done.Count + queue.Count));

                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var nested = new HashSet<string>();
                    CollectPrefabSources(root, nested, onlyLocalized);
                    foreach (var n in nested) if (!done.Contains(n)) queue.Enqueue(n);

                    prefabTexts += ApplyToOwnedTexts(root, font, underlay, legacyFont, onlyLocalized);
                    RevertFontOverrides(root, onlyLocalized);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }

            int sceneTexts = 0;
            for (int i = 0; i < scenePaths.Count; i++)
            {
                EditorUtility.DisplayProgressBar("폰트 적용", $"씬: {scenePaths[i]}", 0.7f + 0.3f * i / scenePaths.Count);
                var scene = EditorSceneManager.OpenScene(scenePaths[i], OpenSceneMode.Single);
                foreach (var go in scene.GetRootGameObjects())
                {
                    sceneTexts += ApplyToOwnedTexts(go, font, underlay, legacyFont, onlyLocalized);
                    RevertFontOverrides(go, onlyLocalized);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (setAsDefault)
            {
                var settings = Resources.Load<TMP_Settings>("TMP Settings");
                if (settings != null)
                {
                    var so = new SerializedObject(settings);
                    so.FindProperty("m_defaultFontAsset").objectReferenceValue = font;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[FontTool] '{font.name}' 적용 완료: 씬 {scenePaths.Count}개 (텍스트 {sceneTexts}개), 프리팹 {done.Count}개 (텍스트 {prefabTexts}개)");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (setup.Length > 0 && setup.All(s => !string.IsNullOrEmpty(s.path))) EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }

    static bool IsTarget(Component text, bool onlyLocalized) =>
        !onlyLocalized
        || text.GetComponent<LocalizeStringEvent>() != null
        || LocalizationSetup.InDropdown(text);

    static void CollectPrefabSources(GameObject root, HashSet<string> paths, bool onlyLocalized)
    {
        foreach (var text in LocalizationSetup.TextsIn(root))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(text) || !IsTarget(text, onlyLocalized)) continue;
            var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(text);
            var path = AssetDatabase.GetAssetPath(source);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab")) paths.Add(path);
        }
    }

    static int ApplyToOwnedTexts(GameObject root, TMP_FontAsset font, Material underlay, Font legacyFont, bool onlyLocalized)
    {
        int count = 0;
        if (legacyFont != null)
        {
            foreach (var legacy in root.GetComponentsInChildren<Text>(true))
            {
                if (PrefabUtility.IsPartOfPrefabInstance(legacy) || !IsTarget(legacy, onlyLocalized)) continue;
                legacy.font = legacyFont;
                EditorUtility.SetDirty(legacy);
                count++;
            }
        }
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (PrefabUtility.IsPartOfPrefabInstance(text) || !IsTarget(text, onlyLocalized)) continue;

            bool hadUnderlay = text.fontSharedMaterial != null && text.fontSharedMaterial.name.Contains(UnderlaySuffix);
            text.font = font;
            text.fontSharedMaterial = hadUnderlay && underlay != null ? underlay : font.material;
            EditorUtility.SetDirty(text);
            count++;
        }
        return count;
    }

    static void RevertFontOverrides(GameObject root, bool onlyLocalized)
    {
        foreach (var text in LocalizationSetup.TextsIn(root))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(text) || !IsTarget(text, onlyLocalized)) continue;
            var so = new SerializedObject(text);
            foreach (var name in new[] { "m_fontAsset", "m_sharedMaterial", "m_FontData.m_Font" })
            {
                var prop = so.FindProperty(name);
                if (prop != null && prop.prefabOverride)
                    PrefabUtility.RevertPropertyOverride(prop, InteractionMode.AutomatedAction);
            }
        }
    }
}
