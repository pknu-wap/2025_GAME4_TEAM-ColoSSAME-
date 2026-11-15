using BattleK.Scripts.AI;
using BattleK.Scripts.HP;
using UnityEngine;

namespace BattleK.Scripts.Combat.Revive
{
    public class ChargeReviveProvider : IReviveProvider
    {
        private readonly float _hpRatio;
        private readonly StatusVisualType _visual;

        public int Priority { get; }
        public bool IsSpent { get; private set; }

        public ChargeReviveProvider(float hpRatio, StatusVisualType visual = StatusVisualType.Normal, int priority = 0)
        {
            _hpRatio = hpRatio;
            _visual = visual;
            Priority = priority;
        }

        public bool CanRevive(StaticAICore unit) => !IsSpent;

        public ReviveResult Revive(StaticAICore unit)
        {
            IsSpent = true;
            return new ReviveResult(Mathf.Max(1, (int)(unit.Health.Max * _hpRatio)), visual: _visual);
        }
    }
}