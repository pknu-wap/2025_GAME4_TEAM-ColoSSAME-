using System;
using System.Collections.Generic;
using BattleK.Scripts.Data.Stat;

namespace TeamManage
{
    public enum EquipResult
    {
        Equipped,
        Unequipped,
        InvalidTarget,
        NotOwned,
        NoFreeSlot,
        ItemUnavailable
    }

    public readonly struct ItemEntryData
    {
        public readonly ItemData Item;
        public readonly int OwnedCount;
        public readonly int AvailableCount;
        public readonly bool EquippedByCurrentUnit;

        public bool Selectable => EquippedByCurrentUnit || AvailableCount > 0;

        public ItemEntryData(ItemData item, int ownedCount, int availableCount, bool equippedByCurrentUnit)
        {
            Item = item;
            OwnedCount = ownedCount;
            AvailableCount = availableCount;
            EquippedByCurrentUnit = equippedByCurrentUnit;
        }
    }

    public static class EquipmentService
    {
        public const int MaxSkillSlots = 3;

        public const int NoItem = Unit.NoEquippedItemId;


        public static List<string> GetEquippedSkillNames(Unit unit)
        {
            var result = new List<string>();
            if (unit?.EquippedSkills == null) return result;

            foreach (UnitSkill skill in unit.EquippedSkills)
            {
                if (skill != null && !string.IsNullOrEmpty(skill.skillName))
                    result.Add(skill.skillName);
            }

            return result;
        }

        public static EquipResult ToggleSkill(Unit unit, string skillName)
        {
            if (unit == null || string.IsNullOrEmpty(skillName)) return EquipResult.InvalidTarget;

            NormalizeEquippedSkills(unit);

            int equippedIndex = unit.EquippedSkills.FindIndex(s => s.skillName == skillName);
            if (equippedIndex >= 0)
            {
                unit.EquippedSkills.RemoveAt(equippedIndex);
                return EquipResult.Unequipped;
            }

            UnitSkill owned = FindOwnedSkill(unit, skillName);
            if (owned == null) return EquipResult.NotOwned;

            if (unit.EquippedSkills.Count >= MaxSkillSlots) return EquipResult.NoFreeSlot;

            unit.EquippedSkills.Add(new UnitSkill(skillName, owned.level));
            return EquipResult.Equipped;
        }

        public static EquipResult UnequipSkill(Unit unit, string skillName)
        {
            if (unit == null || string.IsNullOrEmpty(skillName)) return EquipResult.InvalidTarget;

            NormalizeEquippedSkills(unit);
            int removed = unit.EquippedSkills.RemoveAll(s => s.skillName == skillName);
            return removed > 0 ? EquipResult.Unequipped : EquipResult.InvalidTarget;
        }

        private static void NormalizeEquippedSkills(Unit unit)
        {
            unit.EquippedSkills ??= new List<UnitSkill>();
            unit.EquippedSkills.RemoveAll(s => s == null || string.IsNullOrEmpty(s.skillName));
        }

        private static UnitSkill FindOwnedSkill(Unit unit, string skillName)
        {
            if (unit.OwnedSkills == null) return null;

            foreach (UnitSkill skill in unit.OwnedSkills)
            {
                if (skill != null && skill.skillName == skillName) return skill;
            }

            return null;
        }


        private static int GetOwnedCount(User user, int itemId)
        {
            if (user?.inventory == null) return 0;
            return user.inventory.TryGetValue(itemId.ToString(), out int count) ? count : 0;
        }

        private static int GetEquippedCount(User user, int itemId)
        {
            if (user?.myUnits == null) return 0;

            int count = 0;
            foreach (Unit unit in user.myUnits)
            {
                if (unit != null && unit.equippedItemId == itemId) count++;
            }

            return count;
        }

        public static EquipResult ToggleItem(User user, Unit unit, int itemId)
        {
            if (user == null || unit == null) return EquipResult.InvalidTarget;

            if (unit.equippedItemId == itemId)
            {
                unit.equippedItemId = NoItem;
                return EquipResult.Unequipped;
            }

            int ownedCount = GetOwnedCount(user, itemId);
            if (ownedCount <= 0) return EquipResult.NotOwned;
            if (ownedCount <= GetEquippedCount(user, itemId)) return EquipResult.ItemUnavailable;

            unit.equippedItemId = itemId;
            return EquipResult.Equipped;
        }

        public static EquipResult UnequipItem(Unit unit)
        {
            if (unit == null || unit.equippedItemId == NoItem) return EquipResult.InvalidTarget;

            unit.equippedItemId = NoItem;
            return EquipResult.Unequipped;
        }

        public static List<ItemEntryData> BuildItemEntries(
            User user,
            Unit selectedUnit,
            ItemDatabase itemDatabase,
            List<ItemCategory> categories)
        {
            var result = new List<ItemEntryData>();
            if (user?.inventory == null || itemDatabase == null) return result;

            bool filterByCategory = categories != null && categories.Count > 0;
            Dictionary<int, int> equippedCounts = null;

            foreach (KeyValuePair<string, int> pair in user.inventory)
            {
                if (pair.Value <= 0) continue;
                if (!int.TryParse(pair.Key, out int itemId)) continue;

                ItemData item = itemDatabase.GetById(itemId);
                if (item == null) continue;
                if (filterByCategory && !categories.Contains(item.category)) continue;

                equippedCounts ??= BuildEquippedCounts(user);
                equippedCounts.TryGetValue(itemId, out int equippedCount);
                int availableCount = Math.Max(0, GetOwnedCount(user, itemId) - equippedCount);
                bool equippedByCurrent = selectedUnit != null && selectedUnit.equippedItemId == itemId;
                result.Add(new ItemEntryData(item, pair.Value, availableCount, equippedByCurrent));
            }

            result.Sort((a, b) => a.Item.id.CompareTo(b.Item.id));
            return result;
        }

        private static Dictionary<int, int> BuildEquippedCounts(User user)
        {
            var counts = new Dictionary<int, int>();
            if (user.myUnits == null) return counts;

            foreach (Unit unit in user.myUnits)
            {
                if (unit == null) continue;

                int itemId = unit.equippedItemId;
                counts.TryGetValue(itemId, out int count);
                counts[itemId] = count + 1;
            }

            return counts;
        }
    }
}
