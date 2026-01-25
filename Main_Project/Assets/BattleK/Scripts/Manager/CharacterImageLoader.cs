using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
namespace BattleK.Scripts.Manager
{
    public class CharacterImageLoader : MonoBehaviour
    {
        [Header("UnitLoadManager")]
        [SerializeField] private UnitLoadManager _unitLoadManager;

        [Header("UI 슬롯")]
        [SerializeField] private GameObject[] _characterParents;
        [SerializeField] private bool _deactivateIfMissing = true;

        private readonly Dictionary<string, Sprite> _spriteCache = new(StringComparer.Ordinal);
        private readonly List<AsyncOperationHandle> _handles = new();

        private void Start()
        {
            StartCoroutine(LoadImagesToCharactersCoroutine());
        }

        private void OnDestroy()
        {
            _handles.ForEach(h => { if(h.IsValid()) Addressables.Release(h); });
            _handles.Clear();
            _spriteCache.Clear();
        }

        private IEnumerator LoadImagesToCharactersCoroutine()
        {
            if (!_unitLoadManager)
            {
                DisableAllSlots();
                yield break;
            }

            if (_unitLoadManager.LoadedUser == null)
            {
                _unitLoadManager.TryLoad(out var _);
            }
            
            var user = _unitLoadManager.LoadedUser;
            if (user?.myUnits == null)
            {
                DisableAllSlots();
                yield break;
            }
            
            var unitIds = user.myUnits?
                .Select(u => u.unitId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToList() ?? new List<string>();

            var slotCount = _characterParents?.Length ?? 0;
            
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
                yield return LoadAndApplySpriteToSlot(parent, unitId);
            }
        }

        private IEnumerator LoadAndApplySpriteToSlot(GameObject parent, string unitId)
        {
            if (!parent) yield break;

            var child = parent.transform.Find($"{parent.name}img");
            if (!child || !child.TryGetComponent(out Image img))
            {
                Deactivate(parent);
                yield break;
            }

            if (_spriteCache.TryGetValue(unitId, out var cached) && cached)
            {
                ApplySprite(img, parent, cached, unitId);
                yield break;
            }
            
            var handle = Addressables.LoadAssetAsync<Sprite>(unitId);
            _handles.Add(handle);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded || handle.Result)
            {
                var sprite = handle.Result;
                _spriteCache[unitId] = sprite;
                ApplySprite(img, parent, sprite, unitId);
            }
            else
            {
                Debug.LogWarning($"[CharacterImageLoader] 로드 실패.\nUnitID: {unitId}");
                Deactivate(parent);
            }
        }

        private static void ApplySprite(Image img, GameObject parent, Sprite sprite, string unitIdKey)
        {
            img.sprite = sprite;
            if(parent.TryGetComponent(out CharacterID cid)) cid.characterKey = unitIdKey;
            parent.SetActive(true);
        }

        private void DisableAllSlots()
        {
            if (_characterParents == null) return;
            foreach (var go in _characterParents) Deactivate(go);
        }

        private void Deactivate(GameObject parent)
        {
            if (_deactivateIfMissing && parent) parent.SetActive(false);
        }
    }
}
