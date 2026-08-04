using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.Manager.Strategy.Runtime;
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
            float playerUiScale, float enemyUiScale,
            Vector3 playerOffset, Vector3 enemyOffset,
            FormationAnimConfig playerAnimConfig, FormationAnimConfig enemyAnimConfig)
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

        public void SetEnemyFormationOverride(FormationAsset asset) => _enemyFormationOverride = asset;

        public void BuildPlayerRequests(Slot[] playerSlots, int playerBookIndex,
            List<UnitSpawnRequest> requests, List<AssetReferenceGameObject> assetRefs)
        {
            var positions = _formationManager.CalculatePlayerPositions(playerSlots, _playerUiScale, _playerOffset);
            _lastPlayerPositions = positions;
            if (positions.Count == 0) return;

            var center = GetCenter(positions);
            var startPosBase = _playerAnimConfig.useAnim ? _playerAnimConfig.startCenter : center;
            var endPosBase = _playerAnimConfig.useAnim ? _playerAnimConfig.endCenter : center;
            var duration = _playerAnimConfig.useAnim ? _playerAnimConfig.travelTime : 0f;
            var book = GetBookOrNull(playerBookIndex);

            var posIndex = 0;
            foreach (var slot in playerSlots)
            {
                if (!slot.IsOccupied) continue;
                if (posIndex >= positions.Count) break;

                var key = slot.Occupant.GetComponent<CharacterID>().characterKey;
                var offset = positions[posIndex++] - center;

                TryAdd(new UnitSpawnRequest
                {
                    logicalKey = key,
                    startPos = startPosBase + offset,
                    endPos = endPosBase + offset,
                    faceLeft = true,
                    duration = duration,
                    isPlayer = true,
                    sourceSlot = slot,
                    sourceOccupant = slot.Occupant
                }, book, requests, assetRefs);
            }
        }

        public void BuildEnemyRequests(int enemyBookIndex, List<EnemyFactionConfig> enemyFaction,
            Transform playerRoot, Transform enemyRoot,
            List<UnitSpawnRequest> requests, List<AssetReferenceGameObject> assetRefs)
        {
            var factionConfig = enemyFaction[enemyBookIndex];
            var keys = factionConfig.PickRosterKeys();
            if (keys == null || keys.Count == 0) return;

            List<Vector3> positions;
            if (_enemyFormationOverride)
            {
                positions = _formationManager.CalculatePositionsFromAsset(
                    _enemyFormationOverride, _enemyUiScale, _enemyOffset, _enemyAnimConfig.endCenter);
            }
            else
            {
                var playerPosInEnemySpace = new List<Vector3>();
                if (enemyRoot && playerRoot)
                {
                    playerPosInEnemySpace.AddRange(_lastPlayerPositions
                        .Select(pLocal => playerRoot.TransformPoint(pLocal))
                        .Select(world => enemyRoot.InverseTransformPoint(world)));
                }

                positions = _formationManager.CalculateEnemyPositions(
                    factionConfig, keys.Count, playerPosInEnemySpace,
                    _enemyUiScale, _enemyOffset, _enemyAnimConfig.endCenter);
            }

            if (positions == null) return;

            var centerEnd = _enemyAnimConfig.endCenter;
            var centerStart = _enemyAnimConfig.useAnim ? _enemyAnimConfig.startCenter : centerEnd;
            var duration = _enemyAnimConfig.useAnim ? _enemyAnimConfig.travelTime : 0f;
            var book = factionConfig.addressBookOverride ? factionConfig.addressBookOverride : GetBookOrNull(enemyBookIndex);

            for (var i = 0; i < Mathf.Min(keys.Count, positions.Count); i++)
            {
                var offset = positions[i] - centerEnd;
                TryAdd(new UnitSpawnRequest
                {
                    logicalKey = keys[i],
                    startPos = centerStart + offset,
                    endPos = centerEnd + offset,
                    faceLeft = false,
                    duration = duration,
                    isPlayer = false
                }, book, requests, assetRefs);
            }
        }

        private void TryAdd(UnitSpawnRequest req, CharacterAddressBook book,
            List<UnitSpawnRequest> requests, List<AssetReferenceGameObject> assetRefs)
        {
            if (!book || !book.TryGet(req.logicalKey, out var ar)) return;
            requests.Add(req);
            assetRefs.Add(ar);
        }

        private CharacterAddressBook GetBookOrNull(int idx) =>
            (idx >= 0 && idx < _addressBooks.Count) ? _addressBooks[idx] : null;

        private Vector3 GetCenter(List<Vector3> list) =>
            list.Aggregate(Vector3.zero, (a, b) => a + b) / list.Count;
    }
}