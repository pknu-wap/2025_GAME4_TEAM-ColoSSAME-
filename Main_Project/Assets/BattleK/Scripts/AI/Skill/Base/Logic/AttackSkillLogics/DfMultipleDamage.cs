using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base.Logic.AttackSkillLogics
{
    public class DfMultipleDamage : ISkillLogic
    {
        [SerializeField] private float SkillPointRatio = 0.12f;
        [SerializeField] private float BaseMultiplier = 1.56f;
        [SerializeField] private float DefenseRatio = 0.20f;

        public void Execute(StaticAICore owner, StaticAICore target)
        {
            // 예외 처리
            if (!target || target == owner)
                return;

            // 아군 제외
            if (((1 << target.gameObject.layer) & owner.TargetLayer) == 0)
                return;

            // 공격력 계산
            float attackValue =
                owner.CurrentAttackDamage *
                (BaseMultiplier + SkillPointRatio * owner.Stat.SkillPoint);

            // 방어력 감소 계산
            float defenseReduce =
                target.Stat.Defense * DefenseRatio;

            // 최종 데미지
            int finalDamage =
                Mathf.RoundToInt(attackValue - defenseReduce);

            // 최소 데미지 보정
            finalDamage = Mathf.Max(1, finalDamage);

            // 데미지 적용
            target.OnTakeDamage(finalDamage, true);

            UnityEngine.Debug.Log(
                $"[SPMultipleDamage] " +
                $"{target.name}에게 {finalDamage} 데미지 적용!");
        }
    }
}