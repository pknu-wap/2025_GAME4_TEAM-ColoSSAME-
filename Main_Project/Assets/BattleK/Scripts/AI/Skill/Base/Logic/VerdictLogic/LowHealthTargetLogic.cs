using System;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.VerdictLogic
{
    [Serializable]
    public class LowHealthTargetLogic : IConditionLogic
    {
        private readonly Collider2D[] _results = new Collider2D[4];

        public bool Evaluate(StaticAICore owner, LayerMask targetLayer, Vector2 areaSize, out Transform bestTarget)
        {
            bestTarget = null;

            int count = Physics2D.OverlapBoxNonAlloc(
                owner.transform.position,
                areaSize,
                0,
                _results,
                targetLayer
            );

            float lowestHp = float.MaxValue;
            float lowestMaxHp = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_results[i].TryGetComponent<StaticAICore>(out var core)) continue;
                if (core.IsDead) continue;

                // 아군
                if (core.gameObject.layer != owner.gameObject.layer) continue;

                float hp = core.Stat.CurrentHP;
                float maxHp = core.Stat.MaxHP;

                if (hp < lowestHp)
                {
                    lowestHp = hp;
                    lowestMaxHp = maxHp;
                    bestTarget = core.transform;
                }
                else if (Mathf.Approximately(hp, lowestHp))
                {
                    if (maxHp < lowestMaxHp)
                    {
                        lowestMaxHp = maxHp;
                        bestTarget = core.transform;
                    }
                }
            }

            return bestTarget != null;
        }
    }
}