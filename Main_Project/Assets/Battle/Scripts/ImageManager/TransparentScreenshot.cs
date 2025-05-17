using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D.Sprites;
#endif

namespace Battle.Scripts.ImageManager
{
    public class TransparentScreenshot : MonoBehaviour
    {
        public Camera captureCamera;
        public int width = 1920;
        public int height = 1080;
        public string savePath;
        public LayerMask visibleLayers;
        public List<GameObject> Targets;
        public List<GameObject> Pannels;

        [ContextMenu("투명 스크린샷 찍기 (다수)")]
        public void TakeTransparentScreenshots()
        {
            StartCoroutine(ProcessAllTargets());
        }

        private void Awake()
        {
            savePath = $"Battle/Images";
        }

        private IEnumerator ProcessAllTargets()
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                var target = Targets[i];
                var pannel = i < Pannels.Count ? Pannels[i] : null;
                yield return CaptureScreenshot(target, pannel);
            }
        }
        private IEnumerator CaptureScreenshot(GameObject Target, GameObject Pannel)
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

            string folderPath = Path.Combine(Application.dataPath, savePath, Target.tag);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);
            File.WriteAllBytes(fullPath, screenshot.EncodeToPNG());

            Debug.Log($"✅ 스크린샷 저장 완료: {fullPath}");

    #if UNITY_EDITOR
            string relativePath = $"Assets/{savePath}/{Target.tag}/{fileName}";
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                string relativePath = $"Assets/{savePath}/{Target.tag}/{fileName}";
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();

                    TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.spritePivot = new Vector2(0.58f, 0.5f); // ✅ Pivot 중심으로 설정
                        importer.SaveAndReimport();
                    }

                    EditorApplication.delayCall += () =>
                    {
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
                        if (sprite != null && Pannel != null && Pannel.TryGetComponent(out SpriteRenderer renderer))
                        {
                            renderer.sprite = sprite;
                            Debug.Log($"✅ SpriteRenderer에 '{sprite.name}' 적용 완료 (Pivot Center)");
                        }
                        else
                        {
                            Debug.LogWarning("❌ Sprite를 불러오지 못했거나 SpriteRenderer가 없습니다.");
                        }
                    };
                };

            };
    #endif
            yield return null;
        }
    }
}