using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions; // ← 추가: 표시용 이름 변환에 사용

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TargetWindowController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public Button firstTargetSlot;
    public Button secondTargetSlot;
    public TextMeshProUGUI firstTargetText;
    public TextMeshProUGUI secondTargetText;

    [Header("직업 버튼들 (라벨은 한글: 검사/도끼병/창병/도적/궁수/마법사)")]
    public Button[] jobButtons;

    [Header("캐릭터 이미지")]
    public Image characterImage; // CharacterImg/Image 오브젝트 연결

    // ==== 이미지 경로 설정 ====
    public enum ImagePathMode
    {
        Resources,           // Resources/<folder>/<name>(.png) → Resources.Load 사용
        RelativeToAssets,    // <Project>/Assets/<folder>/<name>.<ext>
        StreamingAssets,     // <Project>/StreamingAssets/<folder>/<name>.<ext>
        PersistentDataPath,  // <App>/persistentData/<folder>/<name>.<ext>
        Absolute             // <folder>/<name>.<ext> (folder에 절대경로 입력)
    }

    [Header("이미지 경로 설정")]
    [Tooltip("어디 기준으로 이미지를 로드할지 선택합니다.")]
    public ImagePathMode pathMode = ImagePathMode.Resources;

    [Tooltip("폴더 경로. 예) Resources 모드: 'ScreenShots', Assets 상대: 'SPUM/ScreenShots', 절대: 'D:/.../ScreenShots'")]
    public string folder = "ScreenShots";

    [Tooltip("파일 확장자(리소스 모드 제외)")]
    public string fileExtension = "png";

#if UNITY_EDITOR
    [SerializeField, TextArea(1, 2)]
    private string _resolvedBasePathPreview;
#endif

    // ==== 내부 상태 ====
    private enum TargetSlot { None, First, Second }
    private TargetSlot currentSelectedSlot = TargetSlot.None;

    // 저장용 키는 원본 그대로 (예: "Astra_Aetherius")
    private string _currentCharacterKey;

    void Start()
    {
        // 슬롯 클릭 시 현재 선택 슬롯 설정
        if (firstTargetSlot)  firstTargetSlot.onClick.AddListener(() => SelectSlot(TargetSlot.First));
        if (secondTargetSlot) secondTargetSlot.onClick.AddListener(() => SelectSlot(TargetSlot.Second));

        // 직업 버튼에 클릭 이벤트 연결 (UI 라벨은 한글, 저장은 enum)
        if (jobButtons != null)
        {
            foreach (Button btn in jobButtons)
            {
                if (!btn) continue;
                var label = btn.GetComponentInChildren<TextMeshProUGUI>()?.text;
                btn.onClick.AddListener(() => SetJobToSelectedSlot(label));
            }
        }
    }

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(_currentCharacterKey))
            SyncUIFromSave();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        _resolvedBasePathPreview = ResolveBaseFolderForPreview();
    }
#endif

    /// <summary>
    /// 캐릭터 이름/이미지 설정 + 저장된 타겟 복원(항상 이 함수로 진입)
    /// </summary>
    public void SetCharacter(string name)
    {
        // 저장용 키는 원본 그대로 보관
        _currentCharacterKey = name?.Trim();

        // 표시는 변환( "_" → " " 등 )된 이름으로
        SetCharacterName(ToDisplayName(_currentCharacterKey));
        LoadCharacterImage(_currentCharacterKey);

        SyncUIFromSave();
    }

    public void SetCharacterName(string nameForDisplay)
    {
        if (characterNameText != null)
        {
            // 혹시 외부에서 원본 키를 넣어도 안전하게 표시용 변환
            characterNameText.text = ToDisplayName(nameForDisplay);
        }
        else
        {
            Debug.LogError("characterNameText가 연결되어 있지 않습니다!");
        }
    }

    /// <summary>
    /// 저장용 키(예: "Astra_Aetherius")를 표시용 문자열("Astra Aetherius")로 변환
    /// 필요 시 여러 언더스코어를 단일 공백으로 정리
    /// </summary>
    private static string ToDisplayName(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return key;
        // 1) 언더스코어를 공백으로
        string s = key.Replace('_', ' ');
        // 2) 공백 여러 개 → 하나로 정리
        s = Regex.Replace(s, @"\s+", " ").Trim();
        return s;
    }

    private void LoadCharacterImage(string characterNameKey)
    {
        if (characterImage == null)
        {
            Debug.LogError("characterImage가 연결되어 있지 않습니다!");
            return;
        }

        if (string.IsNullOrEmpty(characterNameKey))
        {
            characterImage.sprite = null;
            return;
        }

        // ── 1) Resources 모드: Resources.Load 사용 ───────────────────────────────
        if (pathMode == ImagePathMode.Resources)
        {
            // Resources/<folder>/<characterNameKey> (확장자 없이)
            string resPath = CombineUnityPath(folder, characterNameKey);
            Sprite sprite = Resources.Load<Sprite>(resPath);
            if (sprite != null)
            {
                characterImage.sprite = sprite;
                characterImage.color = Color.white; // 투명 방지
            }
            else
            {
                Debug.LogWarning($"[이미지] Resources에서 스프라이트를 찾을 수 없습니다: {resPath} (예상 파일: Assets/Resources/{folder}/{characterNameKey}.png)");
                characterImage.sprite = null;
            }
            return;
        }

        // ── 2) 파일 시스템 모드: Texture2D 로드 후 Sprite 생성 ──────────────────
        string baseFolder = ResolveBaseFolder();
        if (string.IsNullOrEmpty(baseFolder))
        {
            Debug.LogError($"[이미지] 베이스 폴더 해석 실패. mode={pathMode}, folder={folder}");
            characterImage.sprite = null;
            return;
        }

        string fullPath = PathCombineOS(baseFolder, $"{characterNameKey}.{fileExtension}");
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[이미지] 파일을 찾을 수 없습니다: {fullPath}");
            characterImage.sprite = null;
            return;
        }

        try
        {
            byte[] data = File.ReadAllBytes(fullPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(data))
            {
                var sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                characterImage.sprite = sprite;
                characterImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"[이미지] Texture2D.LoadImage 실패: {fullPath}");
                characterImage.sprite = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[이미지] 로드 중 예외: {fullPath}\n{e}");
            characterImage.sprite = null;
        }
    }

    // 저장된 값을 UI에 반영(라벨은 한글로)
    private void SyncUIFromSave()
    {
        var mgr = PlayerCharacterSaveManager.Instance;
        if (mgr == null || string.IsNullOrEmpty(_currentCharacterKey)) return;

        var rec = mgr.GetOrCreate(_currentCharacterKey);

        // 새 포맷: List<UnitClass> 기준
        if (rec.targetClasses != null && rec.targetClasses.Count > 0)
        {
            if (firstTargetText)
            {
                if (rec.targetClasses.Count > 0 && TargetClassMap.EnumToKo.TryGetValue(rec.targetClasses[0], out var ko1))
                    firstTargetText.text = ko1;
                else
                    firstTargetText.text = "타겟1";
            }

            if (secondTargetText)
            {
                if (rec.targetClasses.Count > 1 && TargetClassMap.EnumToKo.TryGetValue(rec.targetClasses[1], out var ko2))
                    secondTargetText.text = ko2;
                else
                    secondTargetText.text = "타겟2";
            }
            return;
        }

        // 구버전 호환(문자열로 저장돼 있을 수 있음)
        if (firstTargetText && !string.IsNullOrEmpty(rec.target1)) firstTargetText.text = rec.target1;
        if (secondTargetText && !string.IsNullOrEmpty(rec.target2)) secondTargetText.text = rec.target2;
    }

    private void SelectSlot(TargetSlot slot)
    {
        currentSelectedSlot = slot;
        HighlightSlot(slot);
    }

    private void HighlightSlot(TargetSlot slot)
    {
        var selectedColor = new Color(1f, 1f, 0.7f);
        var normalColor   = Color.white;

        if (firstTargetSlot)  firstTargetSlot.image.color  = (slot == TargetSlot.First)  ? selectedColor : normalColor;
        if (secondTargetSlot) secondTargetSlot.image.color = (slot == TargetSlot.Second) ? selectedColor : normalColor;
    }

    // 버튼 라벨(한글)을 눌렀을 때: UI는 한글로 표시, 저장은 Enum으로
    private void SetJobToSelectedSlot(string jobName)
    {
        if (currentSelectedSlot == TargetSlot.None)
        {
            Debug.Log("먼저 슬롯을 클릭하세요.");
            return;
        }

        // UI 표시(한글 라벨)
        if (currentSelectedSlot == TargetSlot.First && firstTargetText)        firstTargetText.text  = jobName;
        else if (currentSelectedSlot == TargetSlot.Second && secondTargetText) secondTargetText.text = jobName;

        // 저장(Enum 리스트)
        if (!string.IsNullOrEmpty(_currentCharacterKey) && PlayerCharacterSaveManager.Instance != null)
        {
            var targets = new List<UnitClass>(2);

            if (firstTargetText && TargetClassMap.TryKoToEnum(firstTargetText.text, out var t1))
                targets.Add(t1);
            if (secondTargetText && TargetClassMap.TryKoToEnum(secondTargetText.text, out var t2))
                targets.Add(t2);

            PlayerCharacterSaveManager.Instance.UpdateTargetClasses(_currentCharacterKey, targets);
        }
    }

    // 수동 초기화 버튼 등에만 사용(자동 호출 X)
    public void ResetAll()
    {
        if (characterNameText) characterNameText.text = "캐릭터 이름";
        if (firstTargetText)   firstTargetText.text   = "타겟1";
        if (secondTargetText)  secondTargetText.text  = "타겟2";
        if (characterImage)    characterImage.sprite  = null;

        currentSelectedSlot = TargetSlot.None;
        HighlightSlot(TargetSlot.None);
    }

    // ==========================
    // 경로 처리 유틸리티
    // ==========================

    // Resources 모드에서만 사용 (확장자 없이)
    private static string CombineUnityPath(string a, string b)
    {
        a = (a ?? "").Trim().Replace('\\', '/').Trim('/');
        b = (b ?? "").Trim().Replace('\\', '/').Trim('/');
        if (string.IsNullOrEmpty(a)) return b;
        if (string.IsNullOrEmpty(b)) return a;
        return $"{a}/{b}";
    }

    private string ResolveBaseFolder()
    {
        switch (pathMode)
        {
            case ImagePathMode.Resources:
                // Resources는 baseFolder 개념이 아니라서 사용하지 않음
                return null;

            case ImagePathMode.RelativeToAssets:
                return PathCombineOS(Application.dataPath, folder);

            case ImagePathMode.StreamingAssets:
                return PathCombineOS(Application.streamingAssetsPath, folder);

            case ImagePathMode.PersistentDataPath:
                return PathCombineOS(Application.persistentDataPath, folder);

            case ImagePathMode.Absolute:
                // folder 자체가 절대 경로여야 함
                return NormalizeOSPath(folder);

            default:
                return null;
        }
    }

    private static string PathCombineOS(string basePath, string sub)
    {
        if (string.IsNullOrEmpty(basePath)) return NormalizeOSPath(sub);
        if (string.IsNullOrEmpty(sub)) return NormalizeOSPath(basePath);
        return NormalizeOSPath(Path.Combine(basePath, sub));
    }

    private static string NormalizeOSPath(string p)
    {
        return string.IsNullOrEmpty(p) ? null : p.Replace('\\', '/');
    }

#if UNITY_EDITOR
    private string ResolveBaseFolderForPreview()
    {
        if (pathMode == ImagePathMode.Resources)
            return $"(Resources) Assets/Resources/{folder.Trim().Replace('\\','/')}";
        return ResolveBaseFolder() ?? "(null)";
    }

    [CustomEditor(typeof(TargetWindowController))]
    private class TargetWindowControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var t = (TargetWindowController)target;

            // 해석 경로 미리보기
            EditorGUILayout.Space(6);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUILayout.Label("경로 미리보기", EditorStyles.boldLabel);
                string preview = t.ResolveBaseFolderForPreview();
                EditorGUILayout.HelpBox(preview ?? "(null)", MessageType.Info);
            }

            // 폴더 선택 (Resources 모드는 파일 시스템 폴더 개념이 아니라서 비활성)
            using (new EditorGUI.DisabledScope(t.pathMode == ImagePathMode.Resources))
            {
                if (GUILayout.Button("폴더 선택 (파일 시스템)"))
                {
                    string start = t.ResolveBaseFolder() ?? Application.dataPath;
                    string selected = EditorUtility.OpenFolderPanel("이미지 폴더 선택", start, "");
                    if (!string.IsNullOrEmpty(selected))
                    {
                        // 여기서는 현재 모드 유지하고 folder만 절대경로로 저장
                        t.folder = selected;
                        EditorUtility.SetDirty(t);
                    }
                }
            }

            if (GUILayout.Button("이 캐릭터 이미지 다시 로드 (디버그)"))
            {
                if (!string.IsNullOrEmpty(t._currentCharacterKey))
                    t.LoadCharacterImage(t._currentCharacterKey);
                else
                    Debug.Log("현재 캐릭터 키가 없습니다. SetCharacter 호출 후 테스트하세요.");
            }
        }
    }
#endif
}
