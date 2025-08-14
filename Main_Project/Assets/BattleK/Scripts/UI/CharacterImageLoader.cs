using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterImageLoader : MonoBehaviour
{
    [Header("이미지들이 들어있는 폴더 (절대/상대 경로)")]
    [Tooltip("예시: 'Assets/SPUM/ScreenShots' 또는 절대경로 'D:/.../Assets/SPUM/ScreenShots'")]
    public string imageDirectory = "Assets/SPUM/ScreenShots";

    [Header("Character 오브젝트 (Character1 ~ Character15)")]
    public GameObject[] characterParents;

#if UNITY_EDITOR
    [SerializeField, TextArea(1, 3)]
    private string _resolvedPathPreview;
#endif

    void Start()
    {
        LoadImagesToCharacters();
    }

    void LoadImagesToCharacters()
    {
        var resolvedDir = ResolveImageDirectory(imageDirectory);

        if (string.IsNullOrEmpty(resolvedDir) || !Directory.Exists(resolvedDir))
        {
            Debug.LogError($"[오류] 폴더 없음: {imageDirectory} (해석된 실제 경로: {resolvedDir})");
            return;
        }

        string[] imagePaths = Directory.GetFiles(resolvedDir, "*.png")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        int imageCount = imagePaths.Length;
        int objectCount = characterParents.Length;

        for (int i = 0; i < objectCount; i++)
        {
            if (i < imageCount)
            {
                Transform childImage = characterParents[i].transform.Find($"{characterParents[i].name}img");
                if (childImage != null && childImage.TryGetComponent(out Image img))
                {
                    Texture2D tex = LoadPNG(imagePaths[i]);
                    if (tex == null)
                    {
                        Debug.LogWarning($"[경고] 텍스처 로드 실패: {imagePaths[i]}");
                        characterParents[i].SetActive(false);
                        continue;
                    }

                    Sprite sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );

                    img.sprite = sprite;
                    characterParents[i].SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"{characterParents[i].name}img not found or missing Image component");
                    characterParents[i].SetActive(false);
                }
            }
            else
            {
                characterParents[i].SetActive(false);
            }
        }
    }

    private Texture2D LoadPNG(string path)
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
            Debug.LogError($"[에러] PNG 로드 중 예외: {path}\n{e}");
        }
        return null;
    }

    /// <summary>
    /// Inspector에 입력된 경로를 실제 OS 파일 경로로 해석한다.
    /// - 절대 경로: 그대로 사용
    /// - '프로젝트명/Assets/...' 같은 잘못된 접두어 제거
    /// - 'Assets/...' 또는 'SPUM/...'(자유형) → 프로젝트 루트/Assets 기준으로 결합
    /// - 모든 구분자 '/' 로 표준화
    /// </summary>
    private static string ResolveImageDirectory(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        string normalized = input.Trim().Replace('\\', '/');

        // 절대 경로면 그대로 반환
        if (Path.IsPathRooted(normalized))
            return normalized;

        // 프로젝트 루트와 Assets 경로
        string assetsPath = Application.dataPath.Replace('\\', '/');            // .../<Project>/Assets
        string projectRoot = Directory.GetParent(assetsPath).FullName.Replace('\\', '/'); // .../<Project>

        // "프로젝트명/..." 접두어가 실수로 붙은 경우 제거
        string projectName = Path.GetFileName(projectRoot);
        string prefix = projectName + "/";
        if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring(prefix.Length);
        }

        // "Assets/..." 로 시작하면 프로젝트 루트에 결합
        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return (projectRoot + "/" + normalized).Replace('\\', '/');
        }

        // 그 외 문자열은 Assets 하위 상대 경로로 간주
        return (assetsPath + "/" + normalized).Replace('\\', '/');
    }

#if UNITY_EDITOR
    // Inspector에서 실제 해석 경로 확인용 버튼
    [CustomEditor(typeof(CharacterImageLoader))]
    private class CharacterImageLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var t = (CharacterImageLoader)target;

            if (GUILayout.Button("해석된 실제 경로 미리보기"))
            {
                string resolved = ResolveImageDirectory(t.imageDirectory);
                t._resolvedPathPreview = string.IsNullOrEmpty(resolved) ? "(null)" : resolved;
                EditorUtility.SetDirty(t);
            }

            if (!string.IsNullOrEmpty(t._resolvedPathPreview))
            {
                EditorGUILayout.HelpBox($"Resolved Path:\n{t._resolvedPathPreview}", MessageType.Info);
            }

            if (GUILayout.Button("에디터에서 폴더 선택"))
            {
                string start = Directory.Exists(t._resolvedPathPreview) ? t._resolvedPathPreview : Application.dataPath;
                string selected = EditorUtility.OpenFolderPanel("이미지 폴더 선택", start, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    // 선택값을 그대로 저장(절대경로)하거나, Assets 안이면 Assets 상대 경로로 변환
                    string assetsPath = Application.dataPath.Replace('\\', '/');
                    if (selected.Replace('\\', '/').StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase))
                    {
                        t.imageDirectory = "Assets" + selected.Replace('\\', '/').Substring(assetsPath.Length);
                    }
                    else
                    {
                        t.imageDirectory = selected;
                    }
                    t._resolvedPathPreview = ResolveImageDirectory(t.imageDirectory);
                    EditorUtility.SetDirty(t);
                }
            }

            if (GUILayout.Button("지금 바로 로드 테스트"))
            {
                t.LoadImagesToCharacters();
            }
        }
    }
#endif
}
