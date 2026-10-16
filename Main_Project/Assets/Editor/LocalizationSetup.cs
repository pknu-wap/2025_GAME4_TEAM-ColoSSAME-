using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Localization;
using UnityEditor.Localization.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class LocalizationSetup
{
    const string Root = "Assets/Localization";
    public const string TableName = "UI";

    static readonly string[] SettingPrefabs =
    {
        "Assets/Scenes/Prefab/Setting(Book).prefab",
        "Assets/Scenes/MainMenu_Prefab/Setting(Main).prefab",
    };
    const string MainMenuScene = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Tools/언어/처음 설정 (한 번만)", priority = 100)]
    static void FirstSetup()
    {
        CreateTables();
        ApplyToUI();
    }

    [MenuItem("Tools/언어/문구 표 열기", priority = 1)]
    static void OpenTable()
    {
        var collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
        if (collection == null) LocalizationTablesWindow.ShowWindow();
        else LocalizationTablesWindow.ShowWindow(collection);
    }

    [MenuItem("Tools/언어/미리보기 언어 바꾸기", priority = 2)]
    static void OpenPreview() => EditorApplication.ExecuteMenuItem("Window/Asset Management/Localization Scene Controls");

    public static void CreateTables()
    {
        Directory.CreateDirectory(Root + "/Locales");
        Directory.CreateDirectory(Root + "/Tables");
        AssetDatabase.Refresh();

        var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            settings.name = "LocalizationSettings";
            AssetDatabase.CreateAsset(settings, Root + "/LocalizationSettings.asset");
            LocalizationEditorSettings.ActiveLocalizationSettings = settings;
        }

        var ko = GetOrCreateLocale("ko");
        var en = GetOrCreateLocale("en");

        if (en.Metadata.GetMetadata<FallbackLocale>() == null)
        {
            en.Metadata.AddMetadata(new FallbackLocale(ko));
            EditorUtility.SetDirty(en);
        }
        settings.GetStringDatabase().UseFallback = true;

        var selectors = settings.GetStartupLocaleSelectors();
        if (!selectors.OfType<SpecificLocaleSelector>().Any())
            selectors.Add(new SpecificLocaleSelector { LocaleId = ko.Identifier });
        EditorUtility.SetDirty(settings);

        var collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
        if (collection == null)
            collection = LocalizationEditorSettings.CreateStringTableCollection(TableName, Root + "/Tables");

        AssetDatabase.SaveAssets();
        Debug.Log($"[LocalizationSetup] 문구 표 '{TableName}' 준비 완료 (키 {collection.SharedData.Entries.Count}개)");
    }

    static Locale GetOrCreateLocale(string code)
    {
        var existing = LocalizationEditorSettings.GetLocales().FirstOrDefault(l => l.Identifier.Code == code);
        if (existing != null) return existing;

        var locale = Locale.CreateLocale(new LocaleIdentifier(code));
        // CreateLocale은 에셋 이름을 안 채워줌
        var display = locale.Identifier.CultureInfo != null ? locale.Identifier.CultureInfo.EnglishName : code;
        locale.name = $"{display} ({code})";
        AssetDatabase.CreateAsset(locale, $"{Root}/Locales/{locale.name}.asset");
        LocalizationEditorSettings.AddLocale(locale);
        return locale;
    }

    public static void ApplyToUI()
    {
        if (LocalizationEditorSettings.GetStringTableCollection(TableName) == null)
        {
            Debug.LogError("[LocalizationSetup] 먼저 '처음 설정'을 실행하세요.");
            return;
        }
        foreach (var path in SettingPrefabs)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                AddLanguageRow(root);
                int n = LocalizeTexts(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[LocalizationSetup] {path}: 문구 {n}개 연결, 언어 드롭다운 추가");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
        int count = 0;
        foreach (var go in scene.GetRootGameObjects())
            count += LocalizeTexts(go, skipPrefabInstances: true);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LocalizationSetup] {MainMenuScene}: 문구 {count}개 연결");

        AssetDatabase.SaveAssets();
    }

    static void AddLanguageRow(GameObject root)
    {
        var sound = FindChild(root.transform, "Sound");
        var screenMode = FindChild(root.transform, "ScreenModeDropdown");
        var resolution = FindChild(root.transform, "ResolutionDropdown");
        if (sound == null || screenMode == null || resolution == null) return;

        var language = FindChild(root.transform, "LanguageDropdown");
        if (language == null)
        {
            language = UnityEngine.Object.Instantiate(screenMode.gameObject, screenMode.parent).transform;
            language.name = "LanguageDropdown";
            language.SetSiblingIndex(screenMode.GetSiblingIndex());
            var label = language.GetComponentsInChildren<TMP_Text>(true)
                .FirstOrDefault(t => t.text.Trim() == "화면 모드");
            if (label != null) label.text = "언어";
        }

        float top = 345f, step = 74f, groupGap = 90f;
        SetRowY(sound, language, top);
        SetRowY(sound, screenMode, top - step);
        SetRowY(sound, resolution, top - step * 2);
        string[] sliders = { "Slider", "Slider (1)", "Slider (2)", "Slider (3)" };
        for (int i = 0; i < sliders.Length; i++)
        {
            var s = sound.Find(sliders[i]);
            if (s != null) SetRowY(sound, (RectTransform)s, top - step * 2 - groupGap - step * i);
        }

        var ui = root.GetComponent<SettingsUI>();
        if (ui != null)
        {
            var so = new SerializedObject(ui);
            so.FindProperty("languageDropdown").objectReferenceValue = language.GetComponent<TMP_Dropdown>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void SetRowY(Transform sound, Transform row, float soundLocalY)
    {
        var rt = (RectTransform)row;
        float parentY = 0f;
        for (var p = rt.parent; p != null && p != sound; p = p.parent) parentY += ((RectTransform)p).anchoredPosition.y;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, soundLocalY - parentY);
    }

    static int LocalizeTexts(GameObject root, bool skipPrefabInstances = false)
    {
        var byKorean = KoreanToKey();
        int count = 0;

        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (skipPrefabInstances && PrefabUtility.IsPartOfPrefabInstance(text)) continue;

            var ev = text.GetComponent<LocalizeStringEvent>();
            var plain = Regex.Replace(text.text, "<.*?>", "").Trim();
            bool linked = ev != null && !ev.StringReference.IsEmpty;
            if (!linked && !byKorean.ContainsKey(plain)) continue;

            if (linked) Link(text, null);
            else Link(text, byKorean[plain]);
            count++;
        }

        return count;
    }

    public static void Link(Component text, string key)
    {
        var ev = text.GetComponent<LocalizeStringEvent>();
        if (ev == null) ev = text.gameObject.AddComponent<LocalizeStringEvent>();
        if (key != null) ev.StringReference = new LocalizedString(TableName, key);
        if (ev.OnUpdateString.GetPersistentEventCount() == 0)
        {
            var setter = text.GetType().GetProperty("text").GetSetMethod();
            var action = (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), text, setter);
            UnityEventTools.AddPersistentListener(ev.OnUpdateString, action);
        }
        EditorUtility.SetDirty(ev);
        EditorUtility.SetDirty(text);
    }

    // TMP 텍스트와 예전 UI Text
    public static IEnumerable<Component> TextsIn(GameObject root) =>
        root.GetComponentsInChildren<TMP_Text>(true).Cast<Component>()
            .Concat(root.GetComponentsInChildren<Text>(true));

    public static string GetText(Component c) => c is TMP_Text t ? t.text : c is Text l ? l.text : null;

    public static bool InDropdown(Component c) =>
        c.GetComponentInParent<TMP_Dropdown>(true) != null || c.GetComponentInParent<Dropdown>(true) != null;

    // 문구 표의 한국어 → 키 (같은 문구가 여러 키면 먼저 나온 키)
    static Dictionary<string, string> KoreanToKey()
    {
        var map = new Dictionary<string, string>();
        var collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
        if (collection == null || collection.GetTable("ko") is not StringTable ko) return map;
        foreach (var entry in collection.SharedData.Entries)
        {
            var value = ko.GetEntry(entry.Id)?.Value;
            if (!string.IsNullOrEmpty(value) && !map.ContainsKey(value.Trim())) map[value.Trim()] = entry.Key;
        }
        return map;
    }

    static Transform FindChild(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            var r = FindChild(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
