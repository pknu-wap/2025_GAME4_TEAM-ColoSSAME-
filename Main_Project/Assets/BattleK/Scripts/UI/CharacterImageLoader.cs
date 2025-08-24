using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterImageLoader : MonoBehaviour
{
    [Header("경로 템플릿 ( {CLAN} 토큰을 가문명으로 치환 )")]
    [Tooltip("예: Assets/BattleK/Family/{CLAN}/Images")]
    public string directoryTemplate = "Assets/BattleK/Family/{CLAN}/Images";

    [Header("가문명 리스트 (인덱스로 선택)")]
    [Tooltip("예: Astra, Boreas, Caelum ...")]
    public string[] clanNames = new[] { "Astra" };

    [Header("Character 오브젝트 (Character1 ~ CharacterN)")]
    public GameObject[] characterParents;

    public enum AssignmentMode
    {
        SingleClanDistribute,     // 하나의 가문 폴더에서 모두 수집 후 순차 배치
        PerSlotClanFirstImage     // 슬롯별 선택 가문 폴더의 첫 이미지 배치
    }

    [Header("배치 모드")]
    public AssignmentMode assignmentMode = AssignmentMode.SingleClanDistribute;

    [Header("SingleClanDistribute 모드에서 사용할 가문 인덱스")]
    [Tooltip("clanNames[selectedClanIndex]가 {CLAN} 치환에 사용됨")]
    public int selectedClanIndex = 0;

    [Header("PerSlotClanFirstImage 모드에서 슬롯별 가문 인덱스 (characterParents와 길이 일치 권장)")]
    [Tooltip("비어있거나 범위를 벗어나면 selectedClanIndex로 대체")]
    public int[] clanIndexPerSlot;

    [Header("허용 이미지 확장자(소문자, 점 포함)")]
    public string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

#if UNITY_EDITOR
    [SerializeField, TextArea(1, 6)]
    private string _resolvedPathPreview;     // 미리보기: SingleClanDistribute 기준 경로

    [SerializeField, TextArea(3, 20)]
    private string _assignPreview;           // 미리보기: 어떤 파일이 어느 슬롯에 들어가는지
#endif

    void Start()
    {
        LoadImagesToCharacters();
    }

    public void LoadImagesToCharacters()
    {
        int slotCount = characterParents?.Length ?? 0;
        if (slotCount == 0)
        {
            Debug.LogWarning("[경고] characterParents 배열이 비어있습니다.");
            return;
        }

        switch (assignmentMode)
        {
            case AssignmentMode.SingleClanDistribute:
                LoadSingleClanDistribute();
                break;
            case AssignmentMode.PerSlotClanFirstImage:
                LoadPerSlotClanFirstImage();
                break;
        }
    }

    private void LoadSingleClanDistribute()
    {
        string clan = GetClanByIndexSafe(selectedClanIndex);
        if (string.IsNullOrEmpty(clan))
        {
            Debug.LogError("[오류] 선택된 가문 인덱스가 유효하지 않습니다.");
            DisableAll();
            return;
        }

        string dir = BuildClanDirectory(clan);
        string resolvedDir = ResolveToAbsolute(dir);

        if (string.IsNullOrEmpty(resolvedDir) || !Directory.Exists(resolvedDir))
        {
            Debug.LogError($"[오류] 폴더 없음: {dir} (해석된 실제 경로: {resolvedDir})");
            DisableAll();
            return;
        }

        List<string> images = GetImagesInDirectory(resolvedDir)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        AssignSequential(images);
    }

    private void LoadPerSlotClanFirstImage()
    {
        int slotCount = characterParents.Length;

        for (int i = 0; i < slotCount; i++)
        {
            var parent = characterParents[i];
            if (parent == null) continue;

            int clanIdx = GetClanIndexForSlot(i);
            string clan = GetClanByIndexSafe(clanIdx);
            if (string.IsNullOrEmpty(clan))
            {
                Debug.LogWarning($"[경고] 슬롯 {i + 1}: 유효하지 않은 가문 인덱스 {clanIdx} (대체: selectedClanIndex={selectedClanIndex})");
                clan = GetClanByIndexSafe(selectedClanIndex);
                if (string.IsNullOrEmpty(clan))
                {
                    parent.SetActive(false);
                    continue;
                }
            }

            string dir = BuildClanDirectory(clan);
            string resolvedDir = ResolveToAbsolute(dir);

            if (string.IsNullOrEmpty(resolvedDir) || !Directory.Exists(resolvedDir))
            {
                Debug.LogWarning($"[경고] 슬롯 {i + 1}: 폴더 없음: {dir} (해석: {resolvedDir})");
                parent.SetActive(false);
                continue;
            }

            string firstImage = GetImagesInDirectory(resolvedDir)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(firstImage))
            {
                Debug.LogWarning($"[경고] 슬롯 {i + 1}: '{resolvedDir}'에 이미지가 없습니다.");
                parent.SetActive(false);
                continue;
            }

            ApplyImageToSlot(parent, firstImage);
        }
    }

    private void AssignSequential(List<string> imagePaths)
    {
        int slotCount = characterParents.Length;

        if (imagePaths == null || imagePaths.Count == 0)
        {
            Debug.LogWarning("[경고] 배치할 이미지가 없습니다.");
            DisableAll();
            return;
        }

        int i = 0;
        for (; i < slotCount && i < imagePaths.Count; i++)
        {
            var parent = characterParents[i];
            if (parent == null) continue;

            ApplyImageToSlot(parent, imagePaths[i]);
        }

        for (; i < slotCount; i++)
        {
            if (characterParents[i] != null)
                characterParents[i].SetActive(false);
        }
    }

    private void ApplyImageToSlot(GameObject parent, string filePath)
    {
        var childImage = parent.transform.Find($"{parent.name}img");
        if (childImage != null && childImage.TryGetComponent(out Image img))
        {
            Texture2D tex = LoadImageAny(filePath);
            if (tex == null)
            {
                Debug.LogWarning($"[경고] 텍스처 로드 실패: {filePath}");
                parent.SetActive(false);
                return;
            }

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            img.sprite = sprite;
            parent.SetActive(true);

            // ✅ CharacterID.characterKey 를 이미지 파일명(확장자 제외)으로 설정
            var cid = parent.GetComponent<CharacterID>();
            if (cid != null)
            {
                string key = Path.GetFileNameWithoutExtension(filePath);
                cid.characterKey = key;
                // (선택) 객체 이름도 맞추고 싶다면:
                // parent.name = key;
            }
            else
            {
                Debug.LogWarning($"[경고] {parent.name} 에 CharacterID 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"{parent.name}img 오브젝트를 찾지 못했거나 Image 컴포넌트가 없습니다.");
            parent.SetActive(false);
        }
    }

    private void DisableAll()
    {
        if (characterParents == null) return;
        foreach (var go in characterParents)
        {
            if (go != null) go.SetActive(false);
        }
    }

    // ---------- 유틸 ----------

    private string GetClanByIndexSafe(int index)
    {
        if (clanNames == null || clanNames.Length == 0) return null;
        if (index < 0 || index >= clanNames.Length) return null;
        string name = clanNames[index];
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    private int GetClanIndexForSlot(int slot)
    {
        if (clanIndexPerSlot == null || clanIndexPerSlot.Length == 0) return selectedClanIndex;
        if (slot < 0 || slot >= clanIndexPerSlot.Length) return selectedClanIndex;
        return clanIndexPerSlot[slot];
    }

    private string BuildClanDirectory(string clanName)
    {
        if (string.IsNullOrEmpty(directoryTemplate))
            return null;

        string p = directoryTemplate.Replace("{CLAN}", clanName ?? string.Empty);
        return NormalizePath(p);
    }

    public static string ResolveToAbsolute(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        string normalized = NormalizePath(input);

        if (Path.IsPathRooted(normalized))
            return normalized;

        string assetsPath = NormalizePath(Application.dataPath); // .../<Project>/Assets
        string projectRoot = NormalizePath(Directory.GetParent(assetsPath).FullName); // .../<Project>

        string projectName = Path.GetFileName(projectRoot);
        string prefix = projectName + "/";
        if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            normalized = normalized.Substring(prefix.Length);

        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            return NormalizePath(projectRoot + "/" + normalized);

        return NormalizePath(assetsPath + "/" + normalized);
    }

    public static string NormalizePath(string p)
    {
        return string.IsNullOrEmpty(p) ? p : p.Replace('\\', '/');
    }

    private List<string> GetImagesInDirectory(string dirAbs)
    {
        bool Allowed(string path)
        {
            string ext = Path.GetExtension(path)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && allowedExtensions.Contains(ext);
        }

        if (!Directory.Exists(dirAbs)) return new List<string>();
        return Directory.GetFiles(dirAbs)
                        .Where(Allowed)
                        .ToList();
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
            Debug.LogError($"[에러] 이미지 로드 중 예외: {path}\n{e}");
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

            if (GUILayout.Button("경로 미리보기 (SingleClan 기준)"))
            {
                string clan = t.GetClanByIndexSafe(t.selectedClanIndex) ?? "(invalid)";
                string dir = t.BuildClanDirectory(clan);
                string resolved = CharacterImageLoader.ResolveToAbsolute(dir);
                t._resolvedPathPreview = $"CLAN='{clan}'\nTemplate : {dir}\nResolved : {resolved}";
                EditorUtility.SetDirty(t);
            }

            if (!string.IsNullOrEmpty(t._resolvedPathPreview))
            {
                EditorGUILayout.HelpBox(t._resolvedPathPreview, MessageType.Info);
            }

            if (GUILayout.Button("배치 미리보기"))
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
            int slotCount = t.characterParents?.Length ?? 0;

            sb.AppendLine($"[Mode] {t.assignmentMode}");
            sb.AppendLine($"[Slots] {slotCount}");
            sb.AppendLine("");

            if (t.assignmentMode == AssignmentMode.SingleClanDistribute)
            {
                string clan = t.GetClanByIndexSafe(t.selectedClanIndex) ?? "(invalid)";
                string dir = t.BuildClanDirectory(clan);
                string resolved = CharacterImageLoader.ResolveToAbsolute(dir);
                sb.AppendLine($"[SingleClan] clan='{clan}'");
                sb.AppendLine($"Dir : {dir}");
                sb.AppendLine($"Abs : {resolved}");

                var images = t.GetImagesInDirectory(resolved)
                              .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                              .ToList();

                sb.AppendLine($"Found: {images.Count}");
                sb.AppendLine("");

                for (int i = 0; i < slotCount; i++)
                {
                    string slotName = t.characterParents[i] ? t.characterParents[i].name : "(null)";
                    string pick = (i < images.Count) ? images[i] : "(none)";
                    sb.AppendLine($"Slot {i + 1} ({slotName}) <= {pick}");
                }
            }
            else // PerSlotClanFirstImage
            {
                for (int i = 0; i < slotCount; i++)
                {
                    int clanIdx = t.GetClanIndexForSlot(i);
                    string clan = t.GetClanByIndexSafe(clanIdx) ?? "(invalid)";
                    string dir = t.BuildClanDirectory(clan);
                    string resolved = CharacterImageLoader.ResolveToAbsolute(dir);
                    string first = t.GetImagesInDirectory(resolved)
                                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                    .FirstOrDefault();

                    string slotName = t.characterParents[i] ? t.characterParents[i].name : "(null)";
                    sb.AppendLine($"Slot {i + 1} ({slotName})  clan='{clan}'");
                    sb.AppendLine($"  Dir : {dir}");
                    sb.AppendLine($"  Abs : {resolved}");
                    sb.AppendLine($"  Img : {(string.IsNullOrEmpty(first) ? "(none)" : first)}");
                }
            }

            return sb.ToString();
        }
    }
#endif
}
