using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.Manager.Strategy.Runtime;
using BattleK.Scripts.Manager.Strategy.Runtime.PlayerStrategy;
using BattleK.Scripts.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BattleK.Scripts.Manager
{
    public class BattleFormationRequestBuilder
    {
        private readonly FormationManager _formationManager;
        private readonly List<CharacterAddressBook> _addressBooks;
        private readonly float _playerUiScale;
        private readonly float _enemyUiScale;
        private readonly Vector3 _playerOffset;
        private readonly Vector3 _enemyOffset;
        private readonly FormationAnimConfig _playerAnimConfig;
        private readonly FormationAnimConfig _enemyAnimConfig;

        private List<Vector3> _lastPlayerPositions = new();
        private FormationAsset _enemyFormationOverride;

        public BattleFormationRequestBuilder(
            FormationManager formationManager,
            List<CharacterAddressBook> addressBooks,
            float playerUiScale,
            float enemyUiScale,
            Vector3 playerOffset,
            Vector3 enemyOffset,
            FormationAnimConfig playerAnimConfig,
            FormationAnimConfig enemyAnimConfig)
        {
            _formationManager = formationManager;
            _addressBooks = addressBooks;
            _playerUiScale = playerUiScale;
            _enemyUiScale = enemyUiScale;
            _playerOffset = playerOffset;
            _enemyOffset = enemyOffset;
            _playerAnimConfig = playerAnimConfig;
            _enemyAnimConfig = enemyAnimConfig;
        }

        public void SetEnemyFormationOverride(FormationAsset asset)
        {
            _enemyFormationOverride = asset;
        }

        public void BuildPlayerRequests(Slot[] playerSlots, int playerBookIndex, List<UnitSpawnRequest> requests, List<AssetReferenceGameObject> assetRefs)
        {
            _lastPlayerPositions.Clear();
            if (!_formationManager || playerSlots == null) return;

            var book = GetBookOrNull(playerBookIndex);
            if (!book) return;

            var validSlots = GetValidPlayerSlots(playerSlots);
            if (validSlots.Count == 0) return;

            var positions = _formationManager.CalculatePlayerPositions(validSlots.ToArray(), _playerUiScale, _playerOffset);
            if (positions.Count == 0) return;

            _lastPlayerPositions = positions;

            var center = GetCenter(positions);
            var startCenter = _playerAnimConfig.useAnim ? _playerAnimConfig.startCenter : center;
            var endCenter = _playerAnimConfig.useAnim ? _playerAnimConfig.endCenter : center;
            var duration = _playerAnimConfig.useAnim ? _playerAnimConfig.travelTime : 0f;

            for (var i = 0; i < validSlots.Count; i++)
            {
                var slot = validSlots[i];
                var occupant = slot.Occupant;
                var offset = positions[i] - center;

                TryAdd(new UnitSpawnRequest
                {
                    logicalKey = occupant.UnitKey,
                    startPos = startCenter + offset,
                    endPos = endCenter + offset,
                    faceLeft = true,
                    duration = duration,
                    isPlayer = true,
                    sourceSlot = slot,
                    sourceOccupant = occupant
                }, book, requests, assetRefs);
            }
        }
        public void BuildEnemyRequests(
            int enemyBookIndex,
            List<EnemyFactionConfig> enemyFaction,
            Transform playerRoot,
            Transform enemyRoot,
            List<UnitSpawnRequest> requests,
            List<AssetReferenceGameObject> assetRefs,
            IReadOnlyCollection<string> allowedEnemyIds = null)
        {
            if (!_formationManager) return;
            if (enemyFaction == null || enemyBookIndex < 0 || enemyBookIndex >= enemyFaction.Count) return;

            var factionConfig = enemyFaction[enemyBookIndex];
            if (!factionConfig) return;

            var keys = factionConfig.PickRosterKeys(allowedEnemyIds);
            if (keys == null || keys.Count == 0) return;

            var book = factionConfig.addressBookOverride ? factionConfig.addressBookOverride : GetBookOrNull(enemyBookIndex);
            if (!book)
            {
                Debug.LogWarning($"[BattleFormationRequestBuilder] 적 AddressBook을 찾을 수 없습니다. Index: {enemyBookIndex}");
                return;
            }

            List<Vector3> positions;
            if (_enemyFormationOverride)
            {
                positions = _formationManager.CalculatePositionsFromAsset(_enemyFormationOverride, _enemyUiScale, _enemyOffset, _enemyAnimConfig.endCenter);
            }
            else
            {
                var playerPositionsInEnemySpace = BuildPlayerPositionsInEnemySpace(playerRoot, enemyRoot);
                positions = _formationManager.CalculateEnemyPositions(factionConfig, keys.Count, playerPositionsInEnemySpace, _enemyUiScale, _enemyOffset, _enemyAnimConfig.endCenter);
            }

            if (positions == null || positions.Count == 0) return;

            var endCenter = _enemyAnimConfig.endCenter;
            var startCenter = _enemyAnimConfig.useAnim ? _enemyAnimConfig.startCenter : endCenter;
            var duration = _enemyAnimConfig.useAnim ? _enemyAnimConfig.travelTime : 0f;
            var count = Mathf.Min(keys.Count, positions.Count);

            for (var i = 0; i < count; i++)
            {
                var offset = positions[i] - endCenter;

                TryAdd(new UnitSpawnRequest
                {
                    logicalKey = keys[i],
                    startPos = startCenter + offset,
                    endPos = endCenter + offset,
                    faceLeft = false,
                    duration = duration,
                    isPlayer = false
                }, book, requests, assetRefs);
            }
        }

        private List<Slot> GetValidPlayerSlots(Slot[] playerSlots)
        {
            return playerSlots.Where(slot => slot && slot.Occupant).Where(slot => !string.IsNullOrWhiteSpace(slot.Occupant.UnitKey)).ToList();
        }

        private List<Vector3> BuildPlayerPositionsInEnemySpace(Transform playerRoot, Transform enemyRoot)
        {
            var result = new List<Vector3>();
            if (!playerRoot || !enemyRoot) return result;
            result.AddRange(_lastPlayerPositions.Select(localPosition => playerRoot.TransformPoint(localPosition)).Select(worldPosition => enemyRoot.InverseTransformPoint(worldPosition)));
            return result;
        }

        private void TryAdd(UnitSpawnRequest request, CharacterAddressBook book, List<UnitSpawnRequest> requests, List<AssetReferenceGameObject> assetRefs)
        {
            if (!book) return;
            if (string.IsNullOrWhiteSpace(request.logicalKey)) return;
            if (!book.TryGet(request.logicalKey, out var assetReference)) return;
            requests.Add(request);
            assetRefs.Add(assetReference);
        }

        private CharacterAddressBook GetBookOrNull(int index)
        {
            if (_addressBooks == null || index < 0 || index >= _addressBooks.Count) return null;
            return _addressBooks[index];
        }

        private Vector3 GetCenter(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0) return Vector3.zero;
            return positions.Aggregate(Vector3.zero, (sum, position) => sum + position) / positions.Count;
        }
    }
}