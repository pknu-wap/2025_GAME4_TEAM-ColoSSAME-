using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic;
using BattleK.Scripts.Data.Type.AIDataType.CC;
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
            // 동일한 로직 인스턴스가 중복 실행되는 것을 방지하거나 
            // 그냥 독립적으로 실행 (우리의 결정대로 독립 실행)
            StartCoroutine(CCRoutine(logic, target));
        }
        
        private IEnumerator CCRoutine(ApplyCC logic, StaticAICore target)
        {
            if (logic.IsHardCC) target.EnterCCState(logic.AnimState);
            target.SetStatMultiplier(logic.StatusType, logic, logic.Multiplier);

            yield return new WaitForSeconds(logic.Duration);
            if (logic.IsHardCC) target.ExitCCState();
            target.RemoveStatMultiplier(logic.StatusType, logic);
        }

        private IEnumerator ProcessCCRoutine(StatusData data)
        {
            foreach (var action in data.Actions ) action.OnStart(_aiCore, data);

            var timer = 0f;
            while (timer < data.duration)
            {
                timer += Time.deltaTime;
                
                foreach(var action in data.Actions) action.OnTick(_aiCore, data);
                yield return null;
            }
            
            foreach(var action in data.Actions) action.OnEnd(_aiCore, data);
        }
    }
}
