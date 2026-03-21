using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic;
using UnityEngine;

namespace BattleK.Scripts.Manager.Battle
{
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("References")]
        public StaticAICore _aiCore;
        private readonly List<Coroutine> _runningRoutines = new();

        public void ApplyCustomCC(ApplyCC logic, StaticAICore target)
        {
            _runningRoutines.Add(StartCoroutine(CCRoutine(logic, target)));
        }
        
        private IEnumerator CCRoutine(ApplyCC logic, StaticAICore target)
        {
            if (logic.IsHardCC) target.EnterCCState(logic.AnimState);
            target.SetStatMultiplier(logic.StatusType, logic, logic.Multiplier);

            yield return new WaitForSeconds(logic.Duration);
            if (logic.IsHardCC) target.ExitCCState();
            target.RemoveStatMultiplier(logic.StatusType, logic);
        }
        
        public void ApplyDotDamage(DotDamageLogic logic)
        {
            _runningRoutines.Add(StartCoroutine(DotDamageRoutine(logic)));
        }
        
        private IEnumerator DotDamageRoutine(DotDamageLogic logic)
        {
            var timer = 0f;
            var tickTimer = 0f;

            while (timer < logic.Duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= logic.TickInterval)
                {
                    _aiCore.OnTakeDamage((int)logic.DamagePerTick);
                    tickTimer = 0f;
                }
                yield return null;
            }
        }
    }
}
