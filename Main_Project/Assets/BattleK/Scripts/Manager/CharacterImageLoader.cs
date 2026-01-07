using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using BattleK.Scripts.JSON;

namespace BattleK.Scripts.Manager
{
    public class CharacterImageLoader : MonoBehaviour
    {
        [Header("유저 로드 소스")]
        [Tooltip("씬에 UnitLoadManager가 있으면 그걸 우선 사용합니다.")]
        [SerializeField] private bool _preferUnitLoadManager = true;

        [Header("UnitLoadManager")]
        [SerializeField] private UnitLoadManager _unitLoadManager = null;

        [Tooltip("UnitLoadManager를 못 쓰는 경우, 이 경로(비우면 persistentDataPath/UserSave.json)를 사용")]
        [SerializeField] private string _absoluteJsonPath = "";

        [Header("Addressable 키 규칙")]
        [Tooltip("유닛의 Portrait Address 키 Prefix. 예) Portrait/ -> 최종 키: Portrait/Astra_Aetherius\n비워두면 unitId 자체를 키로 사용합니다.")]
        [SerializeField] private string _portraitKeyPrefix = "";

        [Header("Character 슬롯")]
        [SerializeField] private GameObject[] _characterParents;

        [Header("이미지 없으면 슬롯 비활성화")]
        [SerializeField] private bool _deactivateIfMissing = true;

        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private readonly List<AsyncOperationHandle> _handles = new List<AsyncOperationHandle>();

        private void Start()
        {
            StartCoroutine(LoadImagesToCharactersCoroutine());
        }

        private void OnDestroy()
        {
            foreach (var h in _handles)
            {
                try { Addressables.Release(h); }
                catch { Debug.LogWarning("Exception: Handle delete Failed"); }
            }
            _handles.Clear();
            _spriteCache.Clear();
        }

        private IEnumerator LoadImagesToCharactersCoroutine()
        {
            var slotCount = _characterParents?.Length ?? 0;

            if (!TryGetUser(out var user))
            {
                Debug.LogWarning("[CharacterImageLoader] User Load Failed");
                DisableAllSlots();
                yield break;
            }

            var unitIds = user.myUnits?
                .Select(u => u.unitId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToList() ?? new List<string>();

            if (unitIds.Count == 0)
            {
                Debug.LogWarning("[CharacterImageLoader] myUnit is empty");
                DisableAllSlots();
                yield break;
            }

            for (var i = 0; i < slotCount; i++)
            {
                var parent = _characterParents?[i];
                if (!parent) continue;

                if (i >= unitIds.Count)
                {
                    Deactivate(parent);
                    continue;
                }

                var unitId = unitIds[i];
                var key = BuildPortraitKey(unitId);

                yield return LoadAndApplySpriteToSlot(parent, key, unitId);
            }
        }

        private bool TryGetUser(out User user)
        {
            user = null;

            if (_preferUnitLoadManager && _unitLoadManager != null)
            {
                if (_unitLoadManager.LoadedUser != null)
                {
                    user = _unitLoadManager.LoadedUser;
                    return true;
                }

                if (_unitLoadManager.TryLoad(out var msg) && _unitLoadManager.LoadedUser != null)
                {
                    user = _unitLoadManager.LoadedUser;
                    return true;
                }

                Debug.LogWarning($"[CharacterImageLoader] UnitLoadManager 로드 실패/LoadedUser null. msg={msg}");
            }

            var path = _absoluteJsonPath;
            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine(Application.persistentDataPath, "UserSave.json");

            if (!JsonFileLoader.TryLoadJsonFile<User>(path, out var loaded, out var message))
            {
                Debug.LogWarning($"[CharacterImageLoader] 파일 로드 실패: path={path} reason={message}");
                return false;
            }

            UserDefaults.Ensure(loaded);

            user = loaded;
            return true;
        }

        private IEnumerator LoadAndApplySpriteToSlot(GameObject parent, string portraitKey, string unitIdKey)
        {
            if (!parent)
                yield break;

            // Find Image
            var child = parent.transform.Find($"{parent.name}img");
            if (!child) { Deactivate(parent); yield break; }

            if (!child.TryGetComponent(out Image img)) { Deactivate(parent); yield break; }

            // Cache hit
            if (_spriteCache.TryGetValue(portraitKey, out Sprite cached) && cached != null)
            {
                ApplySprite(img, parent, cached, unitIdKey);
                yield break;
            }

            // Load from Addressable
            var handle = Addressables.LoadAssetAsync<Sprite>(portraitKey);
            _handles.Add(handle);

            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogWarning($"[CharacterImageLoader] Sprite 로드 실패: key={portraitKey}");
                Deactivate(parent);
                yield break;
            }

            Sprite sprite = handle.Result;
            _spriteCache[portraitKey] = sprite;

            ApplySprite(img, parent, sprite, unitIdKey);
        }

        private static void ApplySprite(Image img, GameObject parent, Sprite sprite, string unitIdKey)
        {
            img.sprite = sprite;

            var cid = parent.GetComponent<CharacterID>();
            if (cid != null) cid.characterKey = unitIdKey;

            parent.SetActive(true);
        }

        private string BuildPortraitKey(string unitId)
        {
            if (string.IsNullOrWhiteSpace(_portraitKeyPrefix))
                return unitId;

            return _portraitKeyPrefix.EndsWith("/")
                ? (_portraitKeyPrefix + unitId)
                : (_portraitKeyPrefix + "/" + unitId);
        }

        private void DisableAllSlots()
        {
            if (_characterParents == null) return;
            foreach (var go in _characterParents)
                if (go != null) Deactivate(go);
        }

        private void Deactivate(GameObject parent)
        {
            if (_deactivateIfMissing && parent != null)
                parent.SetActive(false);
        }
    }
}
