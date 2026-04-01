using System;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.VerdictLogic
{
    [Serializable]
    public class HighHealthTargetLogic : IConditionLogic
    {
        private readonly Collider2D[] _results = new Collider2D[4];

        public bool Evaluate(StaticAICore owner, LayerMask targetLayer, Vector2 areaSize, out Transform bestTarget)
        {
            bestTarget = null;

            var size = Physics2D.OverlapBoxNonAlloc(
                owner.transform.position,
                areaSize,
                0,
                _results,
                targetLayer
            );

            float maxHp = -1f;

            for (int i = 0; i < size; i++)
            {
                if (!_results[i].TryGetComponent<StaticAICore>(out var core)) continue;
                if (core.IsDead) continue;

                float hp = core.Stat.CurrentHP;

                if (hp > maxHp)
                {
                    maxHp = hp;
                    bestTarget = core.transform;
                }
            }

            return bestTarget != null;
        }
    }
}