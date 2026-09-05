using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.CCState;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace Shop.Item.Runner.Battle.Stat
{
    internal sealed class BattleItemStatApplier
    {
        private readonly struct FlatRecord
        {
            public readonly StaticAICore Target;
            public readonly ItemEffectDefinition Effect;
            public readonly object Source;

            public FlatRecord(StaticAICore target, ItemEffectDefinition effect, object source)
            {
                Target = target;
                Effect = effect;
                Source = source;
            }
        }

        private readonly List<FlatRecord> _appliedFlatRecords = new();

        public void ClearRecords()
        {
            _appliedFlatRecords.Clear();
        }

        public void RemoveAllAppliedEffects()
        {
            foreach (var record in _appliedFlatRecords)
            {
                if (!record.Target) continue;
                RemoveFlatStat(record.Target, record.Effect, record.Source);
            }
            _appliedFlatRecords.Clear();
        }

        public bool ApplyFlatStat(StaticAICore target, ItemEffectDefinition effect, object source)
        {
            if (!target || target.runtimeStat == null) return false;

            var amount = effect.flatValue;
            if (amount == 0f) return false;

            var label = effect.DisplayName;

            switch (effect.statType)
            {
                case ItemStatType.MaxHp:
                    target.SetFlatModifier(FlatStatusType.MaxHpFlat, source, StatSourceCategory.Item, label, amount);
                    break;

                case ItemStatType.Attack:
                    target.SetFlatModifier(FlatStatusType.AttackDamageFlat, source, StatSourceCategory.Item, label, amount);
                    break;

                case ItemStatType.Defense:
                    target.SetFlatModifier(FlatStatusType.DefenseFlat, source, StatSourceCategory.Item, label, amount);
                    break;

                case ItemStatType.Agility:
                    target.SetFlatModifier(FlatStatusType.EvasionRateFlat, source, StatSourceCategory.Item, label, amount * 0.03f);
                    target.SetFlatModifier(FlatStatusType.AttackSpeedFlat, source, StatSourceCategory.Item, label, amount * 1.05f);
                    break;

                default:
                    return false;
            }

            _appliedFlatRecords.Add(new FlatRecord(target, effect, source));
            return true;
        }

        public void RemoveFlatStat(StaticAICore target, ItemEffectDefinition effect, object source)
        {
            if (!target) return;

            switch (effect.statType)
            {
                case ItemStatType.MaxHp:
                    target.RemoveFlatModifier(FlatStatusType.MaxHpFlat, source);
                    break;

                case ItemStatType.Attack:
                    target.RemoveFlatModifier(FlatStatusType.AttackDamageFlat, source);
                    break;

                case ItemStatType.Defense:
                    target.RemoveFlatModifier(FlatStatusType.DefenseFlat, source);
                    break;

                case ItemStatType.Agility:
                    target.RemoveFlatModifier(FlatStatusType.EvasionRateFlat, source);
                    target.RemoveFlatModifier(FlatStatusType.AttackSpeedFlat, source);
                    break;
            }
        }

        public void ApplyPercentStat(
            StaticAICore target,
            BattleItemEffectRuntime runtime,
            float percent,
            int stackCount)
        {
            if (!target || target.runtimeStat == null) return;

            var safeStackCount = Mathf.Max(1, stackCount);
            var delta = percent * safeStackCount;
            var label = runtime.Effect.DisplayName;

            switch (runtime.Effect.statType)
            {
                case ItemStatType.MaxHp:
                    target.SetFlatModifier(FlatStatusType.MaxHpPercent, runtime, StatSourceCategory.Item, label, delta);
                    break;

                case ItemStatType.Attack:
                    target.SetStatMultiplier(StatusType.AttackDamageMultiplier, runtime, StatSourceCategory.Item, label, delta);
                    break;

                case ItemStatType.Defense:
                    target.SetStatMultiplier(StatusType.DefenseMultiplier, runtime, StatSourceCategory.Item, label, delta);
                    break;

                case ItemStatType.Agility:
                    target.SetStatMultiplier(StatusType.AttackSpeedMultiplier, runtime, StatSourceCategory.Item, label, delta);
                    target.SetStatMultiplier(StatusType.EvasionRateMultiplier, runtime, StatSourceCategory.Item, label, delta);
                    break;
            }
        }

        public void RemovePercentStat(StaticAICore target, BattleItemEffectRuntime runtime)
        {
            if (!target) return;

            switch (runtime.Effect.statType)
            {
                case ItemStatType.MaxHp:
                    target.RemoveFlatModifier(FlatStatusType.MaxHpPercent, runtime);
                    break;

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

        public static void UpdateHpBar(StaticAICore unit)
        {
            if (unit && unit.HPBar)
            {
                unit.HPBar.UpdateHPBar();
            }
        }
    }
}