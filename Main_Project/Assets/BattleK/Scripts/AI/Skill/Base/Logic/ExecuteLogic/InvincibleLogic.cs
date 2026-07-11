using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class InvincibleLogic : ISkillLogic
    {
        [SerializeField] private float Duration = 2f;
        [SerializeField] private bool RefreshDuration = true;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!owner || !target) return;
            if (target.IsDead) return;

            var controller = target.GetComponent<InvincibleTargetController>();
            if (!controller)
            {
                controller = target.gameObject.AddComponent<InvincibleTargetController>();
            }

            controller.Apply(Duration, RefreshDuration);
        }
    }
}
