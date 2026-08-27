using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleK.Scripts.Data.Stat
{
    [CreateAssetMenu(menuName = "BattleK/StatCorrectionTable")]
    public class StatCorrectionTable : ScriptableObject
    {
        [Serializable]
        public class RarityStatFactor
        {
            public int Rarity;
            [Tooltip("공격력 공식의 '성급스탯' 값")]
            public float AtkFactor = 1f;
            [Tooltip("방어력 공식의 '방어스탯' 값")]
            public float DefFactor = 1f;
            [Tooltip("체력 공식의 '체력스탯' 값")]
            public float HpFactor = 1f;
        }
        
        [Header("공통 기본값")]
        [Tooltip("공격력/방어력 공식의 기본값 (20)")]
        public float AtkBase = 20f;
        public float DefBase = 20f;
        [Tooltip("체력 공식의 기본값 (200)")]
        public float HpBase = 200f;

        [Header("공통 배율")]
        public float AtkMultiplier = 2f;
        public float DefMultiplier = 1.5f;
        public float HpMultiplier = 10f;

        [Header("성급별 성장 계수")]
        [Tooltip("Rarity 값별 AtkFactor/DefFactor/HpFactor 설정. 여기 없는 Rarity는 기본값(1)로 처리됩니다.")]
        public List<RarityStatFactor> RarityFactors = new();

        [Header("AGI 파생 공식 설정")]
        [Tooltip("공격속도 배율 = (100 + AGI * AttackSpeedPerAgi) / 100. 기본값 5 (AGI 1당 5%).")]
        public float AttackSpeedPerAgi = 5f;

        [Tooltip("회피율 = AGI * EvasionRatePerAgi. 기본값 0.03 (AGI 1당 3%).")]
        public float EvasionRatePerAgi = 0.03f;

        [Tooltip("회피율 상한. 기본값 0.35 (35%).")]
        public float EvasionRateCap = 0.35f;

        private Dictionary<int, RarityStatFactor> _lookup;

        public RarityStatFactor GetFactor(int rarity)
        {
            _lookup ??= BuildLookup();
            return _lookup.TryGetValue(rarity, out var factor) ? factor : new RarityStatFactor { Rarity = rarity };
        }

        private Dictionary<int, RarityStatFactor> BuildLookup()
        {
            var dict = new Dictionary<int, RarityStatFactor>();
            foreach (var f in RarityFactors)
            {
                dict[f.Rarity] = f;
            }
            return dict;
        }

        private void OnValidate()
        {
            _lookup = null;
        }
    }
}