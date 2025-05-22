using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Battle.Scripts.ImageManager
{
    public class TransparentScreenshot : MonoBehaviour
    {
        public Camera captureCamera;
        public int width = 1920;
        public int height = 1080;
        public LayerMask visibleLayers;
        public List<GameObject> Targets; // 캡처할 대상
        public List<GameObject> Pannels; // SpriteRenderer가 붙은 대상

        private string baseSavePath;

        private void Awake()
        {
            baseSavePath = Path.Combine(Application.dataPath, "Battle/Resources/Images");
            LoadAllSprites(); // 실행 시 자동 로드
        }

        [ContextMenu("사진만 저장")]
        public void CaptureAndSaveAll()
        {
            StartCoroutine(CaptureAllRoutine());
        }

        [ContextMenu("저장된 이미지 불러오기")]
        public void LoadAllSprites()
        {
            foreach (var pannel in Pannels)
            {
                if (pannel == null)
                {
                    Debug.LogWarning("Pannel 리스트에 null이 있음.");
                    continue;
                }

                string tag = pannel.tag;
                string folderPath = Path.Combine(baseSavePath, tag);
                Debug.Log($"tag : {tag} path : {baseSavePath}");
                if (!Directory.Exists(folderPath))
                {
                    Debug.LogWarning($"폴더가 존재하지 않음: {folderPath}");
                    continue;
                }

                string[] files = Directory.GetFiles(folderPath, "*.png");
                bool matched = false;

                foreach (var file in files)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    if (fileNameWithoutExt.Equals(pannel.name, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"✅ 일치하는 이미지 찾음: {file} → {pannel.name}");
                        ApplySpriteToPannel(pannel, file);
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    Debug.LogWarning($"❌ {pannel.name}에 대응하는 PNG 파일이 {folderPath}에 없음");
                }
            }
        }

        private void ApplySpriteToPannel(GameObject pannel, string filePath)
        {
            byte[] data = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(data);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            if (pannel.TryGetComponent(out SpriteRenderer renderer))
            {
                renderer.sprite = sprite;
                Debug.Log($"🖼️ 적용 완료: {Path.GetFileName(filePath)}");
            }
        }

        private IEnumerator CaptureAllRoutine()
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                var target = Targets[i];
                yield return CaptureAndSaveSingle(target);
            }
        }

        private IEnumerator CaptureAndSaveSingle(GameObject Target)
        {
            string fileName = $"{Target.name}.png";

            captureCamera.orthographic = true;
            captureCamera.orthographicSize = 0.8f;
            captureCamera.transform.position = new Vector3(Target.transform.position.x, Target.transform.position.y + 0.3f, -1f);

            var originalClearFlags = captureCamera.clearFlags;
            var originalBackgroundColor = captureCamera.backgroundColor;
            var originalCullingMask = captureCamera.cullingMask;

            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);
            captureCamera.cullingMask = visibleLayers;

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            captureCamera.targetTexture = rt;
            RenderTexture.active = rt;

            var screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
            captureCamera.Render();
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();

            captureCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            captureCamera.clearFlags = originalClearFlags;
            captureCamera.backgroundColor = originalBackgroundColor;
            captureCamera.cullingMask = originalCullingMask;

            // 👉 Pannel에서 Target.name과 일치하는 객체의 tag를 가져와 저장 폴더 결정
            string matchingTag = GetMatchingPannelTag(Target.name);
            Debug.Log(matchingTag);
            string folderPath = Path.Combine(baseSavePath, matchingTag);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);
            byte[] pngData = screenshot.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngData);

            Debug.Log($"📸 저장 완료: {fullPath}");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            yield return null;
        }

        /// <summary>
        /// Target.name과 일치하거나 포함된 Pannel의 tag를 찾아 반환합니다.
        /// 없으면 Untagged 반환
        /// </summary>
        private string GetMatchingPannelTag(string targetName)
        {
            foreach (var pannel in Pannels)
            {
                if (pannel == null) continue;
                if (pannel.name.Equals(targetName, StringComparison.OrdinalIgnoreCase)
                    || targetName.Contains(pannel.name)
                    || pannel.name.Contains(targetName))
                {
                    return pannel.tag;
                }
            }

            return "Untagged";
        }
    }
}