using System.Collections;
using System.Linq;
using BattleK.Scripts.Data;
using UnityEngine;
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

        private readonly AddressableAssetLoader<Sprite> _spriteLoader = new();

        private void Start()
        {
            StartCoroutine(LoadImagesToCharactersCoroutine());
        }

        private void OnDestroy()
        {
            _spriteLoader.ReleaseAll();
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
                _unitLoadManager.TryLoad(out var message);
                if (_unitLoadManager.LoadedUser == null)
                    Debug.LogWarning($"[CharacterImageLoader] 유저 로드 실패: {message}");
            }

            var user = _unitLoadManager.LoadedUser;
            if (user?.myUnits == null)
            {
                DisableAllSlots();
                yield break;
            }

            var unitIds = user.myUnits
                .Select(u => u.unitId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .ToList();

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

                yield return ApplyUnitToSlot(parent, unitIds[i]);
            }
        }

        private IEnumerator ApplyUnitToSlot(GameObject parent, string unitId)
        {
            var child = parent.transform.Find($"{parent.name}img");
            if (!child || !child.TryGetComponent(out Image img))
            {
                Deactivate(parent);
                yield break;
            }

            yield return _spriteLoader.LoadAsync(
                AddressableAssetType.Character,
                unitId,
                sprite => ApplySprite(img, parent, sprite, unitId),
                () => Deactivate(parent));
        }

        private static void ApplySprite(Image img, GameObject parent, Sprite sprite, string unitIdKey)
        {
            img.sprite = sprite;
            if (parent.TryGetComponent(out CharacterID cid)) cid.characterKey = unitIdKey;
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