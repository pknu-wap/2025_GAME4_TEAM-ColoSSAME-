using System;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.CCState;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using UnityEngine;

namespace BattleK.Scripts.Combat
{
    public class UnitStats
    {
        private readonly StaticAICore _owner;
        private readonly Dictionary<StatusType, Dictionary<object, StatModifierEntry>> _multipliers = new();
        private readonly Dictionary<FlatStatusType, Dictionary<object, StatModifierEntry>> _flats = new();
        private int _baseMaxHp;

        public event Action Changed;

        public float MoveSpeed { get; private set; }
        public int AttackDamage { get; private set; }
        public int Defense { get; private set; }
        public int SkillPoint { get; private set; }
        public float EvasionRate { get; private set; }
        public float AttackSpeed { get; private set; }
        public float AttackDelay { get; private set; }
        public int MaxHp { get; private set; }

        public UnitStats(StaticAICore owner)
        {
            _owner = owner;
        }

        public void ResetFromBase()
        {
            _multipliers.Clear();
            _flats.Clear();
            _baseMaxHp = _owner.runtimeStat.MaxHP;
            Recalculate();
        }

        public void SetMultiplier(StatusType type, object source, StatSourceCategory category, string label, float delta)
        {
            if (!_multipliers.TryGetValue(type, out var dict))
                _multipliers[type] = dict = new Dictionary<object, StatModifierEntry>();

            dict[source] = new StatModifierEntry(source, category, label, delta);
            Recalculate();
            Changed?.Invoke();
        }

        public void RemoveMultiplier(StatusType type, object source)
        {
            if (!_multipliers.TryGetValue(type, out var dict)) return;

            dict.Remove(source);
            Recalculate();
            Changed?.Invoke();
        }

        public void SetFlat(FlatStatusType type, object source, StatSourceCategory category, string label, float amount)
        {
            if (!_flats.TryGetValue(type, out var dict))
                _flats[type] = dict = new Dictionary<object, StatModifierEntry>();

            dict[source] = new StatModifierEntry(source, category, label, amount);
            Recalculate();
            Changed?.Invoke();
        }

        public void RemoveFlat(FlatStatusType type, object source)
        {
            if (!_flats.TryGetValue(type, out var dict)) return;

            dict.Remove(source);
            Recalculate();
            Changed?.Invoke();
        }

        public bool Has(StatusType type) => _multipliers.TryGetValue(type, out var dict) && dict.Count > 0;

        public StatBreakdown GetBreakdown(StatusType type, float baseValue)
        {
            var contributions = new List<(string, float)>();
            var totalDelta = 0f;

            if (_multipliers.TryGetValue(type, out var dict))
            {
                foreach (var entry in dict.Values.Where(entry => entry.Delta != 0f))
                {
                    contributions.Add((entry.Label, baseValue * entry.Delta));
                    totalDelta += entry.Delta;
                }
            }

            var finalValue = baseValue * Mathf.Max(0f, 1f + totalDelta);
            return new StatBreakdown(baseValue, contributions, finalValue);
        }

        public StatBreakdown GetFlatBreakdown(FlatStatusType type, float baseValue)
        {
            var contributions = new List<(string, float)>();
            var totalDelta = 0f;

            if (!_flats.TryGetValue(type, out var dict))
                return new StatBreakdown(baseValue, contributions, baseValue + totalDelta);
            foreach (var entry in dict.Values.Where(entry => entry.Delta != 0f))
            {
                contributions.Add((entry.Label, entry.Delta));
                totalDelta += entry.Delta;
            }

            return new StatBreakdown(baseValue, contributions, baseValue + totalDelta);
        }

        public (float Base, float Total, float Delta) GetDisplayValue(
            FlatStatusType flatType, StatusType multiplierType, float baseValue)
        {
            var afterFlat = GetFlatBreakdown(flatType, baseValue).FinalValue;
            var total = GetBreakdown(multiplierType, afterFlat).FinalValue;
            return (baseValue, total, total - baseValue);
        }

        private float Flat(FlatStatusType type)
        {
            var sum = 0f;
            if (_flats.TryGetValue(type, out var dict)) sum += dict.Values.Sum(entry => entry.Delta);
            return sum;
        }

        private float Multiplier(StatusType type)
        {
            var sum = 0f;
            if (_multipliers.TryGetValue(type, out var dict)) sum += dict.Values.Sum(entry => entry.Delta);
            return Mathf.Max(0f, 1f + sum);
        }

        private void Recalculate()
        {
            var s = _owner.runtimeStat;
            if (_baseMaxHp <= 0) _baseMaxHp = s.MaxHP;

            AttackDamage = (int)((s.AttackDamage + Flat(FlatStatusType.AttackDamageFlat))
                                 * Multiplier(StatusType.AttackDamageMultiplier));
            Defense = (int)((s.Defense + Flat(FlatStatusType.DefenseFlat))
                            * Multiplier(StatusType.DefenseMultiplier));
            SkillPoint = (int)(s.SkillPoint * Multiplier(StatusType.SkillPointMultiplier));
            EvasionRate = (s.EvasionRate + Flat(FlatStatusType.EvasionRateFlat))
                          * Multiplier(StatusType.EvasionRateMultiplier);
            MoveSpeed = s.MoveSpeed * Multiplier(StatusType.MoveSpeedMultiplier);

            var speedMul = Mathf.Max(0.01f, Multiplier(StatusType.AttackSpeedMultiplier));
            AttackSpeed = (s.AttackSpeed + Flat(FlatStatusType.AttackSpeedFlat)) * speedMul;
            AttackDelay = s.AttackDelay / speedMul;

            MaxHp = Mathf.Max(1, Mathf.RoundToInt(
                (_baseMaxHp + Flat(FlatStatusType.MaxHpFlat)) * (1f + Flat(FlatStatusType.MaxHpPercent))));
        }
    }
}