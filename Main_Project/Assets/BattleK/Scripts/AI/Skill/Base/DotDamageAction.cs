using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [System.Serializable]
    public class DotDamageAction : ICCAction
    {
        public float DamagePerTick;
        private float _timer;

        public void OnStart(StaticAICore target, CCData data) => _timer = 0;
        public void OnTick(StaticAICore target, CCData data)
        {
            _timer += Time.deltaTime;
            if (!(_timer >= data.tickInterval)) return;
            target.OnTakeDamage((int)DamagePerTick);
            _timer = 0;
        }
        public void OnEnd(StaticAICore target, CCData data) { }
    }
}