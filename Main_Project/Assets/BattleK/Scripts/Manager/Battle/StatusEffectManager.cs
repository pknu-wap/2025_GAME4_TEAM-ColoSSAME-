using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.CCState;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics;
using BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.HP;
using Shop.Item.Runner.Battle.Core;
using UnityEngine;

namespace BattleK.Scripts.Manager.Battle
{
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("References")]
        public StaticAICore _aiCore;
        private readonly List<Coroutine> _runningRoutines = new();

        private readonly Dictionary<StatusVisualType, int> _visualRefCounts = new();

        private void AddVisual(StatusVisualType type)
        {
            _visualRefCounts.TryGetValue(type, out var count);
            _visualRefCounts[type] = count + 1;
            _aiCore.SetVisualStatus(type, true);
        }

        private void RemoveVisual(StatusVisualType type)
        {
            if (!_visualRefCounts.TryGetValue(type, out var count)) return;
            count = Mathf.Max(0, count - 1);
            _visualRefCounts[type] = count;
            if (count == 0) _aiCore.SetVisualStatus(type, false);
        }

        public void ApplyCustomCC(ApplyCC logic, StaticAICore target, float multiplierDelta)
        {
            _runningRoutines.Add(StartCoroutine(CCRoutine(logic, target, multiplierDelta)));
        }

        private IEnumerator CCRoutine(ApplyCC logic, StaticAICore target, float multiplierDelta)
        {
            if (logic.IsHardCC) target.EnterCCState(logic.AnimState);
            if (logic.StatusType != StatusType.None)
            {
                target.SetStatMultiplier(logic.StatusType, logic, StatSourceCategory.Debuff, logic.DisplayName, multiplierDelta);
            }
            if (logic.VisualType != StatusVisualType.Normal) AddVisual(logic.VisualType);

            yield return new WaitForSeconds(logic.Duration);

            if (logic.IsHardCC) target.ExitCCState();
            if (logic.StatusType != StatusType.None)
            {
                target.RemoveStatMultiplier(logic.StatusType, logic);
            }
            if (logic.VisualType != StatusVisualType.Normal) RemoveVisual(logic.VisualType);
        }

        public void ApplyDotDamage(DotDamageLogic logic, StaticAICore attacker, float damagePerTick, bool isPenetrating)
        {
            _runningRoutines.Add(StartCoroutine(DotDamageRoutine(logic, attacker, damagePerTick, isPenetrating)));
        }

        private IEnumerator DotDamageRoutine(DotDamageLogic logic, StaticAICore attacker, float tickDamage, bool isPenetrating)
        {
            if (logic.VisualType != StatusVisualType.Normal) AddVisual(logic.VisualType);

            var timer = 0f;
            var tickTimer = 0f;

            while (timer < logic.Duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= logic.TickInterval)
                {
                    _aiCore.OnTakeDamage((int)tickDamage, attacker, isPenetrating);
                    tickTimer = 0f;
                }
                yield return null;
            }

            if (logic.VisualType != StatusVisualType.Normal) RemoveVisual(logic.VisualType);
        }
    }
}