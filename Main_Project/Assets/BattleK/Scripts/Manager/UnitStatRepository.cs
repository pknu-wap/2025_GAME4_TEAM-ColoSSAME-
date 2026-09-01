using System;
using System.Collections.Generic;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public readonly struct UnitDisplayInfo
    {
        public readonly string UnitId;
        public readonly string UnitName;
        public readonly int Tier;
        public readonly int Level;
        public readonly UnitClass UnitClass;
        public readonly Sprite CharacterImage;
        public readonly FinalStat Stat;

        public UnitDisplayInfo(string unitId, string unitName, int tier, int level, UnitClass unitClass, Sprite characterImage, FinalStat stat)
        {
            UnitId = unitId;
            UnitName = unitName;
            Tier = tier;
            Level = level;
            UnitClass = unitClass;
            CharacterImage = characterImage;
            Stat = stat;
        }
    }

    public static class UnitStatRepository
    {
        private static readonly Dictionary<string, UnitDisplayInfo> _units = new();
        public static event Action<string, UnitDisplayInfo> OnUnitChanged;

        public static void Set(string unitId, string unitName, int tier, int level, UnitClass unitClass, Sprite characterImage, FinalStat stat)
        {
            if (string.IsNullOrEmpty(unitId)) return;
            var info = new UnitDisplayInfo(unitId, unitName, tier, level, unitClass, characterImage, stat);
            _units[unitId] = info;
            OnUnitChanged?.Invoke(unitId, info);
        }

        public static void SetStat(string unitId, FinalStat stat)
        {
            if (string.IsNullOrEmpty(unitId)) return;
            if (_units.TryGetValue(unitId, out var existing))
            {
                Set(unitId, existing.UnitName, existing.Tier, existing.Level, existing.UnitClass, existing.CharacterImage, stat);
            }
        }

        public static bool TryGet(string unitId, out UnitDisplayInfo info)
        {
            if (string.IsNullOrEmpty(unitId))
            {
                info = default;
                return false;
            }
            return _units.TryGetValue(unitId, out info);
        }

        public static bool Contains(string unitId) =>
            !string.IsNullOrEmpty(unitId) && _units.ContainsKey(unitId);

        public static void Remove(string unitId)
        {
            if (string.IsNullOrEmpty(unitId)) return;
            _units.Remove(unitId);
        }

        public static void ClearAll()
        {
            _units.Clear();
        }
    }
}