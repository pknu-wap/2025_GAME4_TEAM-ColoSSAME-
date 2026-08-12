using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

internal sealed class BattleItemStatApplier
{
    private readonly Dictionary<StaticAICore, BattleUnitStatSnapshot> statSnapshots = new();

    public void ClearSnapshots()
    {
        statSnapshots.Clear();
    }

    public void RestoreOriginalStats()
    {
        foreach (KeyValuePair<StaticAICore, BattleUnitStatSnapshot> pair in statSnapshots)
        {
            StaticAICore unit = pair.Key;
            if (!unit || unit.Stat == null) continue;

            pair.Value.Restore(unit.Stat);
            RefreshUnit(unit);
        }
    }

    public bool ApplyFlatStat(StaticAICore target, ItemEffectDefinition effect)
    {
        if (!target || target.Stat == null) return false;

        int amount = Mathf.RoundToInt(effect.flatValue);
        if (amount == 0) return false;

        CaptureOriginal(target);

        UnitStat stat = target.Stat;
        switch (effect.statType)
        {
            case ItemStatType.MaxHp:
                stat.MaxHP += amount;
                stat.CurrentHP = Mathf.Clamp(stat.CurrentHP + amount, 1, stat.MaxHP);
                return true;

            case ItemStatType.Attack:
                stat.AttackDamage += amount;
                return true;

            case ItemStatType.Defense:
                stat.Defense += amount;
                return true;

            case ItemStatType.Agility:
                stat.EvasionRate = Mathf.Clamp(stat.EvasionRate + amount * 0.03f, 0f, 0.35f);
                stat.AttackSpeed += amount * 1.05f;
                return true;
        }

        return false;
    }

    public void ApplyPercentStat(
        StaticAICore target,
        BattleItemEffectRuntime runtime,
        float percent,
        int stackCount)
    {
        if (!target || target.Stat == null) return;

        CaptureOriginal(target);

        int safeStackCount = Mathf.Max(1, stackCount);
        float multiplier = 1f + percent * safeStackCount;

        switch (runtime.Effect.statType)
        {
            case ItemStatType.MaxHp:
                ApplyMaxHpPercent(target, percent * safeStackCount);
                break;

            case ItemStatType.Attack:
                target.SetStatMultiplier(StatusType.AttackDamageMultiplier, runtime, multiplier);
                break;

            case ItemStatType.Defense:
                target.SetStatMultiplier(StatusType.DefenseMultiplier, runtime, multiplier);
                break;

            case ItemStatType.Agility:
                target.SetStatMultiplier(StatusType.AttackSpeedMultiplier, runtime, multiplier);
                target.SetStatMultiplier(StatusType.EvasionRateMultiplier, runtime, multiplier);
                break;
        }
    }

    public void RemovePercentStat(StaticAICore target, BattleItemEffectRuntime runtime)
    {
        if (!target) return;

        switch (runtime.Effect.statType)
        {
            case ItemStatType.Attack:
                target.RemoveStatMultiplier(StatusType.AttackDamageMultiplier, runtime);
                break;

            case ItemStatType.Defense:
                target.RemoveStatMultiplier(StatusType.DefenseMultiplier, runtime);
                break;

            case ItemStatType.Agility:
                target.RemoveStatMultiplier(StatusType.AttackSpeedMultiplier, runtime);
                target.RemoveStatMultiplier(StatusType.EvasionRateMultiplier, runtime);
                break;
        }
    }

    public void RefreshUnit(StaticAICore unit)
    {
        if (!unit || unit.Stat == null) return;

        unit.SetInitialStats();
        UpdateHpBar(unit);
    }

    public static void UpdateHpBar(StaticAICore unit)
    {
        if (unit && unit.HPBar)
        {
            unit.HPBar.UpdateHPBar();
        }
    }

    private void CaptureOriginal(StaticAICore unit)
    {
        if (!unit || unit.Stat == null) return;
        if (statSnapshots.ContainsKey(unit)) return;

        statSnapshots.Add(unit, new BattleUnitStatSnapshot(unit.Stat));
    }

    private void ApplyMaxHpPercent(StaticAICore target, float percent)
    {
        int increase = Mathf.RoundToInt(target.Stat.MaxHP * percent);
        if (increase == 0) return;

        target.Stat.MaxHP += increase;
        target.Stat.CurrentHP = Mathf.Clamp(target.Stat.CurrentHP + increase, 1, target.Stat.MaxHP);
        RefreshUnit(target);
    }
}
