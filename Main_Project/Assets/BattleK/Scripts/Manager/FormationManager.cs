using System;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Manager.Strategy.Runtime;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class FormationManager : MonoBehaviour
    {
        [Header("미리보기 슬롯(좌표 적용 대상)")]
        public RectTransform[] unitImages;

        [Header("옵션")]
        [Tooltip("positions 길이 < unitImages 길이일 때 남은 슬롯을 (0,0)으로 초기화할지")]
        public bool resetUnusedSlotsToZero;

        public event Action<Vector2[]> FormationApplied;

        public void ApplyFormationAsset(FormationAsset asset)
        {
            if (!asset)
            {
                Debug.LogWarning("[FormationManager] FormationAsset이 null입니다.");
                return;
            }

            var positions = asset.uiAnchoredPositions;
            if (!ApplyPositionsInternal(positions))
            {
                Debug.LogWarning("[FormationManager] FormationAsset에 좌표가 없거나 unitImages 비어있음.");
                return;
            }

            FormationApplied?.Invoke(positions);
        }
        
        private bool ApplyPositionsInternal(Vector2[] positions)
        {
            if (positions == null || positions.Length == 0) return false;
            if (unitImages == null || unitImages.Length == 0) return false;

            var count = Mathf.Min(unitImages.Length, positions.Length);
            for (var i = 0; i < count; i++)
            {
                if (!unitImages[i]) continue;
                unitImages[i].anchoredPosition = positions[i];
            }

            if (!resetUnusedSlotsToZero || unitImages.Length <= count) return true;
            {
                for (var i = count; i < unitImages.Length; i++)
                {
                    if (unitImages[i] == null) continue;
                    unitImages[i].anchoredPosition = Vector2.zero;
                }
            }

            return true;
        }
        public List<Vector3> CalculatePlayerPositions(Slot[] slots, float uiScale, Vector3 offset)
        {
            var result = new List<Vector3>();
            if (slots == null) return result;

            foreach (var slot in slots)
            {
                if (!slot || !slot.IsOccupied || !slot.Occupant) continue;

                var uiPos = Vector3.zero;
                var rt = slot.GetComponent<RectTransform>();
                if (rt) 
                {
                    uiPos = new Vector3(rt.anchoredPosition.x, rt.anchoredPosition.y, 0f);
                }
            
                var finalLocalPos = (uiPos * uiScale) + offset;
                result.Add(finalLocalPos);
            }

            return result;
        }
        private Vector3[] TryBuildWithFallbacks(EnemyStrategySet set, EnemyStrategyRequest req, int tryCount)
        {
            var first = set.PickRandom();
            var tried = new HashSet<EnemyStrategyBase>();
            var attempt = BuildSafe(first, req);
            if (attempt is { Length: > 0 }) return attempt;
            tried.Add(first);

            foreach (var w in set.strategies.Where(w => w.strategy && !tried.Contains(w.strategy)))
            {
                attempt = BuildSafe(w.strategy, req);
                if (attempt is { Length: > 0 }) return attempt;
                tried.Add(w.strategy);
                if (tried.Count >= tryCount) break;
            }
            return Array.Empty<Vector3>();
        }

        private Vector3[] BuildCenteredLine(int count, float scale, Vector3 baseOffset, float cellPxX, Vector3 center)
        {
            var arr = new Vector3[count];
            var cell = Mathf.Max(1f, cellPxX) * scale;
            var startX = -(count - 1) * 0.5f * cell;
            for (var i = 0; i < count; i++)
            {
                var x = startX + i * cell;
                arr[i] = new Vector3(x, 0f, 0f) + baseOffset + center;
            }
            return arr;
        }
        private Vector3[] BuildSafe(EnemyStrategyBase strategy, EnemyStrategyRequest req)
        {
            if (!strategy) return Array.Empty<Vector3>();
            Vector3[] arr;
            try { arr = strategy.BuildLocalPositions(req); }
            catch (Exception e)
            {
                Debug.LogWarning($"[BattleStart] 전략 실행 예외: {strategy.name} - {e.Message}");
                return Array.Empty<Vector3>();
            }
            if (arr == null || arr.Length == 0) return Array.Empty<Vector3>();
            return arr;
        }
        public List<Vector3> CalculateEnemyPositions(
            EnemyFactionConfig faction, 
            int count, 
            List<Vector3> playerTargets, 
            float scale, 
            Vector3 offset, 
            Vector3 defaultCenter)
        {
            if (!faction || !faction.strategySet) return new List<Vector3>();

            var req = new EnemyStrategyRequest
            {
                unitCount = count,
                playerLocalTargetsInEnemySpace = playerTargets.ToArray(),
                uiToWorldScale = scale,
                baseOffset = offset,
                formationEndCenter = defaultCenter
            };

            var targets = TryBuildWithFallbacks(faction.strategySet, req, 5);

            if (targets == null || targets.Length == 0)
            {
                targets = BuildCenteredLine(count, scale, offset, 120f, defaultCenter);
            }

            return targets.ToList();
        }
        
        public List<Vector3> CalculatePositionsFromAsset(FormationAsset asset,
            float scale,
            Vector3 offset,
            Vector3 centerCorrection)
        {
            if (!asset || asset.uiAnchoredPositions == null) return null;

            return asset.uiAnchoredPositions.Select(uiPos => new Vector3(uiPos.x * scale, uiPos.y * scale, 0f) + offset + centerCorrection).ToList();
        }
    }
}
