using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public class RuntimeSkill
    {
        public SkillSO Data;
        public float LastUsedTime = -999f;

        public RuntimeSkill(SkillSO data) => Data = data;

        public bool IsReady => Time.time >= LastUsedTime + Data.Cooldown;
        public void ResetCooldown() => LastUsedTime = Time.time;
    }
}