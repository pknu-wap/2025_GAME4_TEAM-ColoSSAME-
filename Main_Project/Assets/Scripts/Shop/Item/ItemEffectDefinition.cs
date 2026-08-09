using System;
using UnityEngine;

public enum ItemUseType
{
    OneBattleConsumable,
    PermanentAccessory
}

public enum ItemEffectDomain
{
    Battle,
    Book
}

public enum ItemEffectTrigger
{
    BattleStart,
    BattleEnd,
    Death,
    Kill,
    LowHp,
    Tick
}

public enum ItemEffectKind
{
    None,
    StatFlatBonus,
    StatPercentBonus,
    ReviveOnce,
    PeriodicHealMaxHpPercent,
    LowHpStatPercentBonus,
    TeamDeathStackingStatPercentBonus,
    KillStackingStatPercentBonus,
    DeathExplosionMaxHpPercent,
    MatchWinGoldPercentBonus,
    MatchLoseGoldPenalty
}

public enum ItemStatType
{
    None,
    MaxHp,
    Attack,
    Defense,
    Agility
}

public enum ItemEffectTarget
{
    Owner,
    Team,
    Enemies,
    Meta
}

[Serializable]
public class ItemEffectDefinition
{
    [Header("Effect Identity")]
    public ItemEffectDomain domain = ItemEffectDomain.Battle;
    public ItemEffectTrigger trigger = ItemEffectTrigger.BattleStart;
    public ItemEffectKind effectKind = ItemEffectKind.None;
    public ItemEffectTarget target = ItemEffectTarget.Owner;

    [Header("Stat")]
    public ItemStatType statType = ItemStatType.None;

    [Tooltip("Fixed stat value. Example: Attack +20, MaxHp +50.")]
    public float flatValue;

    [Tooltip("Percent value as a ratio. Example: 0.25 means +25%.")]
    public float percentValue;

    [Tooltip("Optional upper percent for random ranges. Example: regen 0.01 to 0.02.")]
    public float maxPercentValue;

    [Header("Condition")]
    [Range(0f, 1f)]
    public float hpThresholdRatio;

    [Min(0f)]
    public float tickInterval;

    [Min(0f)]
    public float radius;

    [Tooltip("0 means unlimited unless the runner gives the effect a natural limit.")]
    public int maxTriggerCount;
}
