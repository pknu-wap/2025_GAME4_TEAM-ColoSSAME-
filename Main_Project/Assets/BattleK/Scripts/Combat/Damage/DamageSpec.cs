using System;
using BattleK.Scripts.AI;
using BattleK.Scripts.Combat.Damage.Formulas;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.Combat.Damage
{
    [Serializable]
    public class DamageSpec
    {
        [SerializeReference, SelectableReference]
        public IDamageFormula Formula;

        public DamageType Type = DamageType.Physical;
        public DamageFlags Flags = DamageFlags.None;

        [Min(0), Tooltip("계산 결과가 이 값보다 작으면 이 값으로 올린다")]
        public int MinDamage;

        [Tooltip("비워 두면 기본 'Skill' 프로필을 쓴다")]
        public DamageSourceProfileSO Profile;

        private static readonly IDamageSourceProfile Fallback = new SimpleDamageSourceProfile("Skill", "Skill");
        public IDamageSourceProfile GetProfileOrDefault(IDamageSourceProfile fallback) => Profile ? (IDamageSourceProfile)Profile : fallback;

        public int Evaluate(StaticAICore owner, StaticAICore target)
        {
            if (Formula == null) return 0;
            var raw = Formula.Evaluate(new DamageFormulaContext(owner, target));
            return Mathf.Max(MinDamage, Mathf.RoundToInt(raw));
        }

        public DamageRequest CreateRequest(StaticAICore owner, StaticAICore target, object origin = null)
        {
            var profile = Profile ? (IDamageSourceProfile)Profile : Fallback;
            return profile.CreateRequest(owner, target, Evaluate(owner, target), Type, Flags, origin);
        }
    }
}