using System;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Combat.Damage.Formulas;
using BattleK.Scripts.Combat.Status;
using BattleK.Scripts.Manager.Battle;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    [Serializable]
    public sealed class ApplyStatusLogic : ISkillLogic
    {
        [SerializeField] private StatusSpec _spec = new();

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!owner) return;
            var status = owner.Combat?.Status;
            status?.Apply(new StatusRequest(owner, target, _spec));
        }
    }
}