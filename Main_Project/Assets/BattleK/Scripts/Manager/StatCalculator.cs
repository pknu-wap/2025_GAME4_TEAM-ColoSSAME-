using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public static class StatCalculator
    {
        public static FinalStat Calculate(UnitBaseStat baseStat, StatCorrectionTable table)
        {
            var level = Mathf.Max(1, baseStat.Level);
            var factor = table.GetFactor(baseStat.Rarity);

            var atk = Mathf.RoundToInt(baseStat.BaseAtk + table.AtkBase + level * factor.AtkFactor * table.AtkMultiplier);
            var def = Mathf.RoundToInt(baseStat.BaseDef + table.DefBase + level * factor.DefFactor * table.DefMultiplier);
            var hp  = Mathf.RoundToInt(baseStat.BaseHp  + table.HpBase  + level * factor.HpFactor  * table.HpMultiplier);
            var agi = baseStat.BaseAgi;
            var attackSpeed = baseStat.BaseAttackSpeed;
            var evasionRate = Mathf.Min(agi * table.EvasionRatePerAgi, table.EvasionRateCap);

            return new FinalStat(
                maxHp: hp,
                currentHp: hp,
                attackDamage: atk,
                defense: def,
                skillPoint: baseStat.BaseSkillPoint,
                moveSpeed: baseStat.BaseMoveSpeed,
                evasionRate: evasionRate,
                attackSpeed: attackSpeed,
                attackDelay: baseStat.BaseAttackDelay,
                currentInjury: baseStat.CurrentInjury
            );
        }
        public static void ApplyTo(UnitRuntimeStat target, UnitBaseStat baseStat, StatCorrectionTable table)
        {
            var finalStat = Calculate(baseStat, table);
            finalStat.ApplyTo(target);
        }
    }
}