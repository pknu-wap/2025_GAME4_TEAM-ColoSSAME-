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

        public void ApplyCustomCC(ApplyCC logic, StaticAICore target, float multiplier)
        {
            _runningRoutines.Add(StartCoroutine(CCRoutine(logic, target, multiplier)));
        }
        
        private IEnumerator CCRoutine(ApplyCC logic, StaticAICore target, float multiplier)
        {
            if (logic.IsHardCC) target.EnterCCState(logic.AnimState);
            target.SetStatMultiplier(logic.StatusType, logic, multiplier);

            yield return new WaitForSeconds(logic.Duration);
            if (logic.IsHardCC) target.ExitCCState();
            target.RemoveStatMultiplier(logic.StatusType, logic);
        }
        
        public void ApplyDotDamage(DotDamageLogic logic, float damagePerTick, bool isPenetrating)
        {
            _runningRoutines.Add(StartCoroutine(DotDamageRoutine(logic, damagePerTick, isPenetrating)));
        }
        
        private IEnumerator DotDamageRoutine(DotDamageLogic logic, float tickDamage, bool isPenetrating)
        {
            var timer = 0f;
            var tickTimer = 0f;

            while (timer < logic.Duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= logic.TickInterval)
                {
                    _aiCore.OnTakeDamage((int)tickDamage, isPenetrating);
                    Debug.Log($"[HealthPerDamage] {_aiCore.name}에게 {tickDamage} 데미지 적용!");
                    tickTimer = 0f;
                }
                yield return null;
            }
        }
    }
}
