using System;
using System.Collections.Generic;
using BattleK.Scripts.Combat.Damage.Formulas;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.HP;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.Combat.Status
{
    [Flags]
    public enum StatusTags
    {
        None = 0,
        Buff = 1,
        Debuff = 2,
        HardCC = 4,
        Dispellable = 8
    }
    
    [Flags]
    public enum UnitStatusFlags
    {
        None = 0,
        Invincible = 1,
        Stunned = 2,
        Silenced = 3,
    }

    [Serializable]
    public sealed class StatModifierSpec
    {
        public StatusType Stat;
        [SerializeReference, SelectableReference] public IDamageFormula Delta;
    }

    [Serializable]
    public sealed class StatusSpec
    {
        public UnitStatusFlags Flags;
        public string DisplayName;
        public StatusTags Tags = StatusTags.Debuff;
        public float Duration = 2f;
        public StatusStackPolicy StackPolicy = StatusStackPolicy.Refresh;
        public StatusVisualType Visual = StatusVisualType.Normal;
        public PlayerState HardCcAnim = PlayerState.DEBUFF;
        public List<StatModifierSpec> Modifiers = new();
    }
}