using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Battle.Scripts.Value.Data;
using Newtonsoft.Json;
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
        }

        [ContextMenu("사진만 저장")]
        public void CaptureAndSaveAll()
        {
            StartCoroutine(CaptureAllRoutine());
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
            var id = Target.GetComponent<CharacterID>();
            string fileName = $"{id.characterKey}.png";

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

            string folderPath;

            if (id.characterTeamKey == "Team01")
            {
                folderPath = Path.Combine(baseSavePath, "PlayerCharacter");
            }
            else
            {
                string subFolder = id.characterTeamKey;
                folderPath = Path.Combine(baseSavePath, "EnemyCharacter", subFolder);
            }

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);
            byte[] pngData = screenshot.EncodeToPNG();
            File.WriteAllBytes(fullPath, pngData);

            Debug.Log($"저장 완료: {fullPath}");

    #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
    #endif

            yield return null;
        }

        [ContextMenu("출전한 캐릭터 이미지만 불러오기")]
        public void LoadOnlyDeployedSprites()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "PlayerSave.json");

            if (!File.Exists(filePath))
            {
                Debug.LogWarning("저장된 JSON 파일이 없습니다.");
                return;
            }

            string json = File.ReadAllText(filePath);
            CharacterData data = JsonConvert.DeserializeObject<CharacterData>(json);

            HashSet<string> deployedKeys = new HashSet<string>();
            foreach (var pair in data.characters)
            {
                if (pair.Value.IsDeployed)
                    deployedKeys.Add(pair.Key);
            }

            if (deployedKeys.Count == 0)
            {
                Debug.Log("출전한 캐릭터가 없습니다.");
                return;
            }

            foreach (var pannel in Pannels)
            {
                if (pannel == null) continue;
                var id = pannel.GetComponent<CharacterID>();
                if (id == null) continue;

                if (!deployedKeys.Contains($"{id.characterTeamKey}_{id.characterKey}")) continue;

                string folderPath;

                if (id.characterTeamKey == "Team01")
                {
                    folderPath = Path.Combine(baseSavePath, "PlayerCharacter");
                }
                else
                {
                    string subFolder = id.characterTeamKey.Replace("Team", "Enemy");
                    folderPath = Path.Combine(baseSavePath, "EnemyCharacter", subFolder);
                }

                if (!Directory.Exists(folderPath)) continue;

                string[] files = Directory.GetFiles(folderPath, "*.png");
                foreach (var file in files)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    if (fileNameWithoutExt.Equals(id.characterKey, StringComparison.OrdinalIgnoreCase))
                    {
                        ApplySpriteToPannel(pannel, file);
                        Debug.Log($"출전 캐릭터 이미지 적용됨: {file}");
                        break;
                    }
                }
            }
        }

        [ContextMenu("저장된 이미지 불러오기")]
        public void LoadAllSprites()
        {
            foreach (var pannel in Pannels)
            {
                if (pannel == null) continue;

                var id = pannel.GetComponent<CharacterID>();
                if (id == null) continue;

                string folderPath;

                if (id.characterTeamKey == "Team01")
                {
                    folderPath = Path.Combine(baseSavePath, "PlayerCharacter");
                }
                else
                {
                    string subFolder = id.characterTeamKey;
                    folderPath = Path.Combine(baseSavePath, "EnemyCharacter", subFolder);
                }

                if (!Directory.Exists(folderPath)) continue;

                string[] files = Directory.GetFiles(folderPath, "*.png");
                foreach (var file in files)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    if (fileNameWithoutExt.Equals(id.characterKey, StringComparison.OrdinalIgnoreCase))
                    {
                        ApplySpriteToPannel(pannel, file);
                        break;
                    }
                }
            }
        }

        public void LoadSpriteForSingle(GameObject target, string teamKey)
        {
            if (target == null) return;

            var id = target.GetComponent<CharacterID>();
            if (id == null)
            {
                Debug.LogWarning("CharacterID 컴포넌트가 없습니다.");
                return;
            }

            string folderPath;

            if (teamKey == "Team01")
            {
                folderPath = Path.Combine(baseSavePath, "PlayerCharacter");
            }
            else
            {
                string subFolder = id.characterTeamKey;
                folderPath = Path.Combine(baseSavePath, "EnemyCharacter", subFolder);
            }

            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning("지정된 폴더가 존재하지 않습니다: " + folderPath);
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.png");
            foreach (var file in files)
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                if (fileNameWithoutExt.Equals(id.characterKey, StringComparison.OrdinalIgnoreCase))
                {
                    ApplySpriteToPannel(target, file);
                    return;
                }
            }

            Debug.LogWarning($"{id.characterKey}에 해당하는 이미지가 {folderPath}에 없음");
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
            }
        }
    }
}