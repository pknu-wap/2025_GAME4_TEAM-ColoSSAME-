using System;
using System.Collections.Generic;
using BattleK.Scripts.Data;
using UnityEngine;

namespace BattleK.Scripts.Manager.Strategy.Runtime
{
    [CreateAssetMenu(menuName = "Battle/Enemy/Faction Config", fileName = "EF_Faction")]
    public class EnemyFactionConfig : ScriptableObject
    {
        [Header("가문 이름(표시용)")]
        public string FactionName;

        [Header("가문 Addressable Book (선택, 지정 시 Battle의 enemyBookIndex보다 우선)")]
        public CharacterAddressBook addressBookOverride;

        [Header("가문 전형 세트 (가중 랜덤으로 1개 선택)")]
        public EnemyStrategySet strategySet;

        [Serializable]
        public struct WeightedKey
        {
            public string key;
            [Min(0f)] public float weight;
        }

        [Header("출전 후보(가중치 부여) — 비어있으면 아래 enemyKeys를 사용")]
        public List<WeightedKey> weightedEnemyKeys = new();

        [Header("출전선수 수")]
        [Min(0)] public int rosterCount = 4;

        [Header("출전 유닛 키 (하위호환): weightedEnemyKeys가 비어있을 때 사용")]
        public List<string> enemyKeys = new();

        [Tooltip("하위호환: enemyKeys 사용 시 셔플 후 상위 rosterCount만 사용")]
        public bool shuffleKeys = true;

        [Header("저장 로스터 교차 검증")]
        [Tooltip("true면 allowedIds(현재 저장된 팀 로스터)에 없는 키는 후보에서 자동 제외됩니다. " +
                 "PickRosterKeys에 allowedIds를 넘기지 않으면 이 옵션은 무시됩니다.")]
        public bool restrictToAllowedIds = true;

        [Tooltip("weightedEnemyKeys/enemyKeys에 등록되지 않았지만 allowedIds(저장 로스터)에는 존재하는 유닛에게 부여할 기본 가중치")]
        [Min(0f)] public float defaultWeightForUnlisted = 1f;

        public List<string> PickRosterKeys(IReadOnlyCollection<string> allowedIds = null)
        {
            var want = Mathf.Max(0, rosterCount);
            if (want == 0) return new List<string>();

            var useWeighted = weightedEnemyKeys is { Count: > 0 } || (allowedIds != null && restrictToAllowedIds);

            if (useWeighted)
            {
                var candidates = BuildWeightedCandidates(allowedIds);
                if (candidates.Count == 0) return new List<string>();
                return PickWeightedKeys(candidates, want);
            }

            var list = new List<string>(enemyKeys ?? new List<string>());
            if (list.Count == 0) return new List<string>();
            if (shuffleKeys) Shuffle(list);
            if (list.Count > want) list = list.GetRange(0, want);
            return list;
        }

        private List<WeightedKey> BuildWeightedCandidates(IReadOnlyCollection<string> allowedIds)
        {
            var source = weightedEnemyKeys is { Count: > 0 }
                ? weightedEnemyKeys
                : new List<WeightedKey>();

            if (allowedIds == null || !restrictToAllowedIds)
                return new List<WeightedKey>(source);

            var allowedSet = new HashSet<string>(allowedIds, StringComparer.OrdinalIgnoreCase);
            var result = new List<WeightedKey>();

            foreach (var w in source)
            {
                if (!string.IsNullOrEmpty(w.key) && allowedSet.Contains(w.key))
                    result.Add(w);
            }

            foreach (var id in allowedSet)
            {
                var alreadyListed = source.Exists(w => string.Equals(w.key, id, StringComparison.OrdinalIgnoreCase));
                if (!alreadyListed)
                    result.Add(new WeightedKey { key = id, weight = defaultWeightForUnlisted });
            }

            return result;
        }

        private List<string> PickWeightedKeys(List<WeightedKey> source, int count)
        {
            count = Mathf.Clamp(count, 0, source.Count);
            var result = new List<string>(count);
            if (count == 0) return result;

            var pool = new List<WeightedKey>(source);
            for (var i = 0; i < count && pool.Count > 0; i++)
            {
                var sum = SumWeights(pool);
                if (sum <= 0f) break;
                var idx = SpinRouletteIndex(pool, sum);
                if (idx < 0 || idx >= pool.Count) break;

                result.Add(pool[idx].key);
                pool.RemoveAt(idx);
            }
            return result;
        }

        private static float SumWeights(List<WeightedKey> list)
        {
            var s = 0f;
            for (var i = 0; i < list.Count; i++)
                s += Mathf.Max(0f, list[i].weight);
            return s;
        }

        private static int SpinRouletteIndex(List<WeightedKey> list, float sum)
        {
            var r = UnityEngine.Random.value * sum;
            for (var i = 0; i < list.Count; i++)
            {
                var w = Mathf.Max(0f, list[i].weight);
                if (r <= w) return i;
                r -= w;
            }
            return list.Count - 1;
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}