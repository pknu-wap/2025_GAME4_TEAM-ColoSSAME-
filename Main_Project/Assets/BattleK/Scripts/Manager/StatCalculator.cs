using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public static class StatCalculator
    {
        public static FinalStat Calculate(UnitBaseStat baseStat, StatCorrectionTable table, ClassBaseStatTable classTable)
        {
            var level = Mathf.Max(1, baseStat.Level);
            var factor = table.GetFactor(baseStat.Rarity);

            var atk = Mathf.RoundToInt(baseStat.BaseAtk + table.AtkBase + level * factor.AtkFactor * table.AtkMultiplier);
            var def = Mathf.RoundToInt(baseStat.BaseDef + table.DefBase + level * factor.DefFactor * table.DefMultiplier);
            var hp  = Mathf.RoundToInt(baseStat.BaseHp  + table.HpBase  + level * factor.HpFactor  * table.HpMultiplier);
            var agi = baseStat.BaseAgi;
            var evasionRate = Mathf.Min(agi * table.EvasionRatePerAgi, table.EvasionRateCap);

            if (classTable == null)
            {
                Debug.LogWarning($"[StatCalculator] {baseStat.UnitName}({baseStat.UnitClass}): ClassBaseStatTable이 없어 MoveSpeed/AttackSpeed/AttackDelay가 0으로 계산됩니다.");
            }
            var classEntry = classTable != null
                ? classTable.GetEntryOrDefault(baseStat.UnitClass)
                : default;

            return new FinalStat(
                maxHp: hp,
                currentHp: hp,
                attackDamage: atk,
                defense: def,
                moveSpeed: classEntry.MoveSpeed,
                evasionRate: evasionRate,
                attackSpeed: classEntry.AttackSpeed,
                attackDelay: classEntry.AttackDelay,
                currentInjury: baseStat.CurrentInjury
            );
        }

        public static void ApplyTo(UnitRuntimeStat target, UnitBaseStat baseStat, StatCorrectionTable table, ClassBaseStatTable classTable)
        {
            var finalStat = Calculate(baseStat, table, classTable);
            finalStat.ApplyTo(target);
        }
    }
}