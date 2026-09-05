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
            if (!unit || unit.runtimeStat == null) continue;

            pair.Value.Restore(unit.runtimeStat);
            RefreshUnit(unit);
        }
    }

    public bool ApplyFlatStat(StaticAICore target, ItemEffectDefinition effect)
    {
        if (!target || target.runtimeStat == null) return false;

        int amount = Mathf.RoundToInt(effect.flatValue);
        if (amount == 0) return false;

        CaptureOriginal(target);

        UnitRuntimeStat runtimeStat = target.runtimeStat;
        switch (effect.statType)
        {
            case ItemStatType.MaxHp:
                runtimeStat.MaxHP += amount;
                runtimeStat.CurrentHP = Mathf.Clamp(runtimeStat.CurrentHP + amount, 1, runtimeStat.MaxHP);
                return true;

            case ItemStatType.Attack:
                runtimeStat.AttackDamage += amount;
                return true;

            case ItemStatType.Defense:
                runtimeStat.Defense += amount;
                return true;

            case ItemStatType.Agility:
                runtimeStat.EvasionRate = Mathf.Clamp(runtimeStat.EvasionRate + amount * 0.03f, 0f, 0.35f);
                runtimeStat.AttackSpeed += amount * 1.05f;
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
        if (!target || target.runtimeStat == null) return;

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
        if (!unit || unit.runtimeStat == null) return;

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
        if (!unit || unit.runtimeStat == null) return;
        if (statSnapshots.ContainsKey(unit)) return;

        statSnapshots.Add(unit, new BattleUnitStatSnapshot(unit.runtimeStat));
    }

    private void ApplyMaxHpPercent(StaticAICore target, float percent)
    {
        int increase = Mathf.RoundToInt(target.runtimeStat.MaxHP * percent);
        if (increase == 0) return;

        target.runtimeStat.MaxHP += increase;
        target.runtimeStat.CurrentHP = Mathf.Clamp(target.runtimeStat.CurrentHP + increase, 1, target.runtimeStat.MaxHP);
        RefreshUnit(target);
    }
}
