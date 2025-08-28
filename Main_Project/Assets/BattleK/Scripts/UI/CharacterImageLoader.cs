using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 기존: clanNames/디렉토리 스캔 기반 배치
/// 변경: UserSave.json의 myUnits[*].unitId 를 이용하여
///       Assets/BattleK/Family/{CLAN}/Images/{unitId}.png 만 정확히 매칭해 로드
///
/// - 4개 기존 파일(User, Unit, UserManager, UserSaveManager) 수정 없이 동작
/// - UnitLoadManager 사용/미사용 모두 지원
/// </summary>
public class CharacterImageLoader : MonoBehaviour
{
    [Header("=== UserSave.json 로드 소스 선택 ===")]
    [Tooltip("체크 시, 씬의 UnitLoadManager에서 LoadedUser를 읽어옵니다.\n해제 시, 아래 absoluteJsonPath(또는 basic fallback)에서 직접 읽어옵니다.")]
    public bool useUnitLoadManager = true;

    [Tooltip(@"useUnitLoadManager=false일 때 사용할 절대 경로.
예) C:\Users\User\AppData\LocalLow\DefaultCompany\StateLearning\UserSave.json
비워두면 Application.persistentDataPath\UserSave.json 을 사용합니다.")]
    public string absoluteJsonPath = "";

    [Header("이미지 루트 템플릿 ( {CLAN} 토큰을 가문명으로 치환 )")]
    [Tooltip("예: Assets/BattleK/Family/{CLAN}/Images")]
    public string directoryTemplate = "Assets/BattleK/Family/{CLAN}/Images";

    [Header("Character 오브젝트 (Character1 ~ CharacterN)")]
    public GameObject[] characterParents;

    [Header("허용 확장자(소문자, 점 포함) - 기본: .png 만 사용")]
    public string[] allowedExtensions = new[] { ".png" };

    [Header("로드 실패시 사용: 슬롯 비활성화 여부")]
    public bool deactivateIfMissing = true;

    [Header("파일명 매칭: unitId와 정확히 일치하는 파일만 로드")]
    public bool exactMatchOnly = true;

#if UNITY_EDITOR
    [SerializeField, TextArea(2, 6)]
    private string _jsonPathPreview;

    [SerializeField, TextArea(3, 20)]
    private string _assignPreview;
#endif

    // 내부: 마지막으로 읽은 User 구조
    private User _lastLoadedUser;

    // JSON 역직렬화 옵션(스키마 변경 내성)
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling     = NullValueHandling.Include,
        Formatting            = Formatting.Indented
    };

    // ========= Entry =========
    void Start()
    {
        LoadImagesToCharacters();
    }

    /// <summary>
    /// 메인 엔트리: UserSave.json을 읽어 myUnits 순서대로 이미지를 배치
    /// </summary>
    public void LoadImagesToCharacters()
    {
        int slotCount = characterParents?.Length ?? 0;
        if (slotCount == 0)
        {
            Debug.LogWarning("[CharacterImageLoader] characterParents 배열이 비어있습니다.");
            return;
        }

        // 1) UserSave 읽기
        if (!TryLoadUserSave(out var user, out var msg))
        {
            Debug.LogWarning("[CharacterImageLoader] UserSave 로드 실패: " + msg);
            DisableAllSlots();
            return;
        }

        _lastLoadedUser = user;

        // 2) myUnits의 unitId 순서대로 이미지 적용
        var unitIds = user.myUnits?.Select(mu => mu.unitId).Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                     ?? new List<string>();

        if (unitIds.Count == 0)
        {
            Debug.LogWarning("[CharacterImageLoader] myUnits가 비어있습니다.");
            DisableAllSlots();
            return;
        }

        AssignByUnitIds(unitIds);
    }

    // ========= Core Logic =========

    /// <summary>
    /// unitId 목록을 받아, 각 unitId에 해당하는 png를 찾아 슬롯에 순차 배치
    /// - unitId = 'Astra_Orion' → clan='Astra'
    /// - 폴더 = directoryTemplate.Replace('{CLAN}', clan)
    /// - 파일 = {폴더}/{unitId}.png
    /// </summary>
    private void AssignByUnitIds(List<string> unitIds)
    {
        int slotCount = characterParents.Length;
        int idx = 0;

        for (; idx < slotCount && idx < unitIds.Count; idx++)
        {
            var parent = characterParents[idx];
            if (parent == null) continue;

            string unitId = unitIds[idx].Trim();
            string clan   = ExtractClan(unitId); // "Astra_Orion" → "Astra"

            if (string.IsNullOrEmpty(clan))
            {
                WarnAndDeactivate(parent, $"unitId '{unitId}'에서 가문명 추출 실패");
                continue;
            }

            string imagesDir = ResolveToAbsolute(BuildClanDirectory(clan));
            if (string.IsNullOrEmpty(imagesDir) || !Directory.Exists(imagesDir))
            {
                WarnAndDeactivate(parent, $"폴더 없음: {imagesDir}");
                continue;
            }

            // 파일명 정확 매칭: {unitId}.png
            string picked = PickImageForUnitId(imagesDir, unitId, exactMatchOnly);
            if (string.IsNullOrEmpty(picked))
            {
                WarnAndDeactivate(parent, $"이미지 찾기 실패: {unitId} (폴더: {imagesDir})");
                continue;
            }

            ApplyImageToSlot(parent, picked, overrideCharacterKey: unitId);
        }

        // 남은 슬롯 처리
        for (; idx < slotCount; idx++)
        {
            if (deactivateIfMissing && characterParents[idx] != null)
                characterParents[idx].SetActive(false);
        }
    }

    /// <summary>
    /// unitId 앞부분(언더스코어 이전)을 clan으로 간주
    /// 예: Astra_Orion → Astra
    /// </summary>
    private string ExtractClan(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId)) return null;
        int pos = unitId.IndexOf('_');
        if (pos <= 0) return null;
        return unitId.Substring(0, pos);
    }

    /// <summary>
    /// clan → 템플릿 치환
    /// </summary>
    private string BuildClanDirectory(string clanName)
    {
        if (string.IsNullOrWhiteSpace(directoryTemplate)) return null;
        return NormalizePath(directoryTemplate.Replace("{CLAN}", clanName ?? string.Empty));
    }

    /// <summary>
    /// imagesDir 내부에서 unitId와 일치하는 파일을 선택
    /// exactMatchOnly=true 이면 {unitId}.{확장자} 만 허용
    /// </summary>
    private string PickImageForUnitId(string imagesDir, string unitId, bool exact)
    {
        try
        {
            if (!Directory.Exists(imagesDir)) return null;

            if (exact)
            {
                foreach (var ext in allowedExtensions)
                {
                    string p = Path.Combine(imagesDir, unitId + ext);
                    if (File.Exists(p)) return NormalizePath(p);
                }
                return null;
            }
            else
            {
                // contains / prefix 매칭 등 확장 가능
                string lower = unitId.ToLowerInvariant();
                var files = Directory.GetFiles(imagesDir)
                                     .Where(p => allowedExtensions.Contains(Path.GetExtension(p).ToLowerInvariant()))
                                     .OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
                foreach (var f in files)
                {
                    string name = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                    if (name.Contains(lower)) return NormalizePath(f);
                }
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[CharacterImageLoader] PickImageForUnitId 예외: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 실제 이미지 적용 + CharacterID.characterKey = unitId로 강제
    /// </summary>
    private void ApplyImageToSlot(GameObject parent, string filePath, string overrideCharacterKey = null)
    {
        var childImage = parent.transform.Find($"{parent.name}img");
        if (childImage != null && childImage.TryGetComponent(out Image img))
        {
            Texture2D tex = LoadImageAny(filePath);
            if (tex == null)
            {
                WarnAndDeactivate(parent, $"텍스처 로드 실패: {filePath}");
                return;
            }

            Sprite sprite = Sprite.Create(
                tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f
            );

            img.sprite = sprite;
            parent.SetActive(true);

            var cid = parent.GetComponent<CharacterID>();
            if (cid != null)
            {
                // unitId를 그대로 characterKey로 설정
                string key = overrideCharacterKey ?? Path.GetFileNameWithoutExtension(filePath);
                cid.characterKey = key;
            }
            else
            {
                Debug.LogWarning($"[CharacterImageLoader] {parent.name} 에 CharacterID 컴포넌트가 없습니다.");
            }
        }
        else
        {
            WarnAndDeactivate(parent, $"{parent.name}img 오브젝트를 찾지 못했거나 Image 컴포넌트가 없습니다.");
        }
    }

    private void WarnAndDeactivate(GameObject parent, string reason)
    {
        Debug.LogWarning($"[CharacterImageLoader] 슬롯 처리 실패: {reason}");
        if (deactivateIfMissing && parent != null)
            parent.SetActive(false);
    }

    private void DisableAllSlots()
    {
        if (characterParents == null) return;
        foreach (var go in characterParents)
        {
            if (go != null && deactivateIfMissing) go.SetActive(false);
        }
    }

    // ========= UserSave 로드 =========

    /// <summary>
    /// UserSave.json 로드(두 가지 루트)
    /// 1) useUnitLoadManager=true: 씬의 UnitLoadManager에서 LoadedUser 읽기
    /// 2) false: absoluteJsonPath(또는 persistentDataPath)에서 직접 파일 로드
    /// </summary>
    private bool TryLoadUserSave(out User user, out string message)
    {
        user = null;
        message = string.Empty;

        if (useUnitLoadManager)
        {
            var loader = FindObjectOfType<UnitLoadManager>();
            if (loader == null)
            {
                message = "UnitLoadManager를 찾지 못했습니다.";
                return false;
            }

            if (!loader.HasUser())
            {
                // 로더에 아직 로드 안 됐으면 시도
                if (!loader.LoadFromAbsolutePath(out var msg))
                {
                    message = "UnitLoadManager.LoadFromAbsolutePath 실패: " + msg;
                    return false;
                }
            }

            user = loader.LoadedUser;
            if (user == null)
            {
                message = "UnitLoadManager.LoadedUser 가 null 입니다.";
                return false;
            }

            return true;
        }
        else
        {
            string jsonPath = absoluteJsonPath;
            if (string.IsNullOrWhiteSpace(jsonPath))
                jsonPath = Path.Combine(Application.persistentDataPath, "UserSave.json");

            if (!File.Exists(jsonPath))
            {
                message = "세이브 파일이 없습니다: " + jsonPath;
                return false;
            }

            try
            {
                string json = File.ReadAllText(jsonPath);
                var data = JsonConvert.DeserializeObject<User>(json, jsonSettings);
                if (data == null)
                {
                    message = "역직렬화 결과가 null입니다.";
                    return false;
                }
                if (data.myUnits == null) data.myUnits = new List<Unit>();
                if (data.inventory == null) data.inventory = new Dictionary<string, int>();
                user = data;
                return true;
            }
            catch (Exception ex)
            {
                message = "예외: " + ex.Message;
                return false;
            }
        }
    }

    // ========= Path/IO 유틸 =========

    public static string ResolveToAbsolute(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        string normalized = NormalizePath(input);

        if (Path.IsPathRooted(normalized))
            return normalized;

        // ProjectRoot/Assets
        string assetsPath = NormalizePath(Application.dataPath);
        string projectRoot = NormalizePath(Directory.GetParent(assetsPath).FullName);

        // "Assets/..." → ProjectRoot + "/Assets/..."
        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            return NormalizePath(projectRoot + "/" + normalized);

        // 상대경로면 Assets 기준으로 처리
        return NormalizePath(assetsPath + "/" + normalized);
    }

    public static string NormalizePath(string p)
    {
        return string.IsNullOrEmpty(p) ? p : p.Replace('\\', '/');
    }

    private Texture2D LoadImageAny(string path)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(fileData))
                return tex;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CharacterImageLoader] 이미지 로드 예외: {path}\n{e}");
        }
        return null;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterImageLoader))]
    private class CharacterImageLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var t = (CharacterImageLoader)target;

            GUILayout.Space(6);

            if (GUILayout.Button("UserSave 경로 미리보기"))
            {
                string p = t.useUnitLoadManager ? "(UnitLoadManager 사용 중)"
                                                : (string.IsNullOrWhiteSpace(t.absoluteJsonPath)
                                                    ? Path.Combine(Application.persistentDataPath, "UserSave.json")
                                                    : t.absoluteJsonPath);
                t._jsonPathPreview = p;
                EditorUtility.SetDirty(t);
            }
            if (!string.IsNullOrEmpty(t._jsonPathPreview))
            {
                EditorGUILayout.HelpBox(t._jsonPathPreview, MessageType.Info);
            }

            if (GUILayout.Button("배치 미리보기 (UserSave 기반)"))
            {
                t._assignPreview = BuildAssignPreview(t);
                EditorUtility.SetDirty(t);
            }
            if (!string.IsNullOrEmpty(t._assignPreview))
            {
                EditorGUILayout.HelpBox(t._assignPreview, MessageType.Info);
            }

            GUILayout.Space(6);
            if (GUILayout.Button("지금 바로 로드"))
            {
                t.LoadImagesToCharacters();
            }
        }

        private static string BuildAssignPreview(CharacterImageLoader t)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // 간이 로드(실패해도 미리보기 문자열 생성)
            if (!t.TryLoadUserSave(out var user, out var msg))
            {
                sb.AppendLine("[UserSave 로드 실패]");
                sb.AppendLine(msg);
                return sb.ToString();
            }

            var unitIds = user.myUnits?.Select(mu => mu.unitId).Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                          ?? new List<string>();

            sb.AppendLine($"[myUnits] {unitIds.Count}");
            for (int i = 0; i < t.characterParents.Length; i++)
            {
                string slotName = t.characterParents[i] ? t.characterParents[i].name : "(null)";
                string unitId = (i < unitIds.Count) ? unitIds[i] : "(none)";
                string clan   = (unitId != "(none)") ? t.ExtractClan(unitId) : "(none)";

                string dir = string.IsNullOrEmpty(clan) ? "(invalid clan)" : t.BuildClanDirectory(clan);
                string abs = string.IsNullOrEmpty(clan) ? "(invalid clan)" : ResolveToAbsolute(dir);
                string pick = "(none)";

                if (!string.IsNullOrEmpty(clan) && Directory.Exists(abs))
                {
                    pick = t.PickImageForUnitId(abs, unitId, t.exactMatchOnly) ?? "(none)";
                }

                sb.AppendLine($"Slot {i + 1} ({slotName})");
                sb.AppendLine($"  unitId : {unitId}");
                sb.AppendLine($"  clan   : {clan}");
                sb.AppendLine($"  dir    : {dir}");
                sb.AppendLine($"  abs    : {abs}");
                sb.AppendLine($"  file   : {pick}");
            }

            return sb.ToString();
        }
    }
#endif
}