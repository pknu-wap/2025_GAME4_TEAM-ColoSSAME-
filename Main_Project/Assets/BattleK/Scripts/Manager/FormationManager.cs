using System;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Manager.Strategy.Runtime;
using BattleK.Scripts.Manager.Strategy.Runtime.PlayerStrategy;
using BattleK.Scripts.UI;
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

        public FormationAsset CurrentFormation { get; private set; }

        public event Action<Vector2[]> FormationApplied;

        public void ApplyFormationAsset(FormationAsset asset)
        {
            if (!asset) return;
            var positions = asset.uiAnchoredPositions;
            if (!ApplyPositionsInternal(positions)) return;
            CurrentFormation = asset;
            RefreshOccupiedUnitPositions();
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

            if (resetUnusedSlotsToZero)
            {
                for (var i = count; i < unitImages.Length; i++)
                {
                    if (!unitImages[i]) continue;
                    unitImages[i].anchoredPosition = Vector2.zero;
                }
            }

            return true;
        }

        private void RefreshOccupiedUnitPositions()
        {
            if (unitImages == null) return;
            foreach (var image in unitImages)
            {
                if (!image) continue;
                if (!image.TryGetComponent<Slot>(out var slot)) continue;
                if (!slot.Occupant) continue;
                slot.Occupant.RefreshSlotPosition();
            }
        }

        public List<Vector3> CalculatePlayerPositions(Slot[] slots, float uiScale, Vector3 offset)
        {
            var result = new List<Vector3>();
            if (slots == null) return result;

            foreach (var slot in slots)
            {
                if (!slot || !slot.IsOccupied || !slot.Occupant) continue;
                var uiPos = Vector3.zero;
                if (slot.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    uiPos = new Vector3(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, 0f);
                }
                result.Add(uiPos * uiScale + offset);
            }
            return result;
        }

        public List<Vector3> CalculateEnemyPositions(EnemyFactionConfig faction, int count, List<Vector3> playerTargets, float scale, Vector3 offset, Vector3 defaultCenter)
        {
            if (!faction || !faction.strategySet || count <= 0) return new List<Vector3>();
            var request = new EnemyStrategyRequest
            {
                unitCount = count,
                playerLocalTargetsInEnemySpace = playerTargets?.ToArray() ?? Array.Empty<Vector3>(),
                uiToWorldScale = scale,
                baseOffset = offset,
                formationEndCenter = defaultCenter
            };
            var targets = TryBuildWithFallbacks(faction.strategySet, request, 5);
            if (targets.Length == 0)
            {
                targets = BuildCenteredLine(count, scale, offset, 120f, defaultCenter);
            }
            return targets.ToList();
        }

        public List<Vector3> CalculatePositionsFromAsset(FormationAsset asset, float scale, Vector3 offset, Vector3 centerCorrection)
        {
            if (!asset || asset.uiAnchoredPositions == null) return null;
            return asset.uiAnchoredPositions.Select(uiPos => new Vector3(uiPos.x * scale, uiPos.y * scale, 0f) + offset + centerCorrection).ToList();
        }

        private Vector3[] TryBuildWithFallbacks(EnemyStrategySet set, EnemyStrategyRequest request, int tryCount)
        {
            if (!set || tryCount <= 0) return Array.Empty<Vector3>();
            var tried = new HashSet<EnemyStrategyBase>();
            var first = set.PickRandom();
            if (first)
            {
                tried.Add(first);
                var firstAttempt = BuildSafe(first, request);
                if (firstAttempt.Length > 0) return firstAttempt;
            }
            if (set.strategies == null) return Array.Empty<Vector3>();
            foreach (var attempt in from weightedStrategy in set.strategies.TakeWhile(weightedStrategy => tried.Count < tryCount) select weightedStrategy.strategy into strategy where strategy && tried.Add(strategy) select BuildSafe(strategy, request) into attempt where attempt.Length > 0 select attempt)
            {
                return attempt;
            }
            return Array.Empty<Vector3>();
        }

        private Vector3[] BuildSafe(EnemyStrategyBase strategy, EnemyStrategyRequest request)
        {
            if (!strategy) return Array.Empty<Vector3>();
            try
            {
                return strategy.BuildLocalPositions(request) ?? Array.Empty<Vector3>();
            }
            catch (Exception e)
            {
                return Array.Empty<Vector3>();
            }
        }

        private Vector3[] BuildCenteredLine(int count, float scale, Vector3 baseOffset, float cellPxX, Vector3 center)
        {
            var positions = new Vector3[count];
            var cell = Mathf.Max(1f, cellPxX) * scale;
            var startX = -(count - 1) * 0.5f * cell;
            for (var i = 0; i < count; i++)
            {
                var x = startX + i * cell;
                positions[i] = new Vector3(x, 0f, 0f) + baseOffset + center;
            }
            return positions;
        }
    }
}