using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic
{
    public class TauntLogic : ISkillLogic
    {
        [SerializeField] private float Duration = 2f;
        [SerializeField] private bool RestorePreviousTarget = true;
        [SerializeField] private bool RefreshDuration = true;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            if (!owner || !target) return;
            if (owner == target || owner.IsDead || target.IsDead) return;
            if (owner.gameObject.layer == target.gameObject.layer) return;

            var controller = target.GetComponent<TauntTargetController>();
            if (!controller)
            {
                controller = target.gameObject.AddComponent<TauntTargetController>();
            }

            controller.Apply(owner, Duration, RestorePreviousTarget, RefreshDuration);
        }
    }

}
