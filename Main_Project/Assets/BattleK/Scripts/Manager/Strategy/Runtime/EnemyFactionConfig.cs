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

        // ====== 가중 로스터 ======
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

        public enum RosterPickMode
        {
            WeightedWithoutReplacement, // 가중치 비례, 중복 없이
            WeightedWithReplacement     // 가중치 비례, 중복 허용
        }

        [Header("선발 방식")]
        public RosterPickMode rosterPickMode = RosterPickMode.WeightedWithoutReplacement;

        // ====== [하위호환] 기존 로스터 ======
        [Header("출전 유닛 키 (하위호환): weightedEnemyKeys가 비어있을 때 사용")]
        public List<string> enemyKeys = new();

        [Tooltip("하위호환: enemyKeys 사용 시 셔플 후 상위 rosterCount만 사용")]
        public bool shuffleKeys = true;

        public List<string> PickRosterKeys()
        {
            var want = Mathf.Max(0, rosterCount);

            if (weightedEnemyKeys is { Count: > 0 })
            {
                return PickWeighted(weightedEnemyKeys, want, rosterPickMode == RosterPickMode.WeightedWithReplacement);
            }

            var list = new List<string>(enemyKeys ?? new List<string>());
            if (list.Count == 0 || want == 0) return new List<string>();
            if (shuffleKeys) Shuffle(list);
            if (list.Count > want) list = list.GetRange(0, want);
            return list;
        }

        private List<string> PickWeighted(List<WeightedKey> source, int count, bool withReplacement)
        {
            count = Mathf.Clamp(count, 0, Mathf.Max(0, source.Count));
            var result = new List<string>(count);
            if (count == 0) return result;

            if (withReplacement)
            {
                var sum = SumWeights(source);
                if (sum <= 0f) return result;
                for (var i = 0; i < count; i++)
                {
                    var pick = SpinRoulette(source, sum);
                    if (!string.IsNullOrEmpty(pick))
                        result.Add(pick);
                }
            }
            else
            {
                var pool = new List<WeightedKey>(source);
                for (var i = 0; i < count && pool.Count > 0; i++)
                {
                    var sum = SumWeights(pool);
                    if (sum <= 0f) break;
                    var idx = SpinRouletteIndex(pool, sum);
                    if (idx >= 0 && idx < pool.Count)
                    {
                        result.Add(pool[idx].key);
                        pool.RemoveAt(idx);
                    }
                    else break;
                }
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

        private static string SpinRoulette(List<WeightedKey> list, float sum)
        {
            var r = UnityEngine.Random.value * sum;
            for (var i = 0; i < list.Count; i++)
            {
                var w = Mathf.Max(0f, list[i].weight);
                if (r <= w) return list[i].key;
                r -= w;
            }
            return list.Count > 0 ? list[list.Count - 1].key : null;
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
