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

    /// <summary>아이템 페이지 한 칸에 표시할 데이터 (보유 수량 + 장착 상태).</summary>
    public readonly struct ItemEntryData
    {
        public readonly ItemData Item;
        public readonly int OwnedCount;
        public readonly int AvailableCount;      // 보유 - (전체 유닛에 장착된 수)
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

    /// <summary>
    /// 스킬/아이템 장착 규칙만 담당하는 상태 없는 static 서비스.
    /// UI/저장은 다루지 않음 (저장은 TeamManageController.CommitEquipment).
    /// </summary>
    public static class EquipmentService
    {
        public const int MaxSkillSlots = 3;

        /// <summary>Unit.equippedItemId가 "장착 없음"일 때의 값 (ClassDataBase.SaveTo와 동일).</summary>
        public const int NoItem = -1;

        // ───────────── Skill ─────────────

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

        /// <summary>장착 중이면 해제, 아니면 빈 슬롯에 장착.</summary>
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

        // 기존 훈련 UI(SkillSelectUI)가 남겨둔 빈 슬롯("")/null 항목 제거
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

        // ───────────── Item ─────────────

        public static int GetOwnedCount(User user, int itemId)
        {
            if (user?.inventory == null) return 0;
            return user.inventory.TryGetValue(itemId.ToString(), out int count) ? count : 0;
        }

        /// <summary>해당 아이템을 장착 중인 유닛 수 (유닛당 슬롯 1개이므로 = 사용 중인 개수).</summary>
        public static int GetEquippedCount(User user, int itemId)
        {
            if (user?.myUnits == null) return 0;

            int count = 0;
            foreach (Unit unit in user.myUnits)
            {
                if (unit != null && unit.equippedItemId == itemId) count++;
            }

            return count;
        }

        public static int GetAvailableCount(User user, int itemId)
        {
            return Math.Max(0, GetOwnedCount(user, itemId) - GetEquippedCount(user, itemId));
        }

        /// <summary>장착 중이면 해제, 아니면 장착 (기존 아이템은 교체). 남은 수량이 없으면 거부.</summary>
        public static EquipResult ToggleItem(User user, Unit unit, int itemId)
        {
            if (user == null || unit == null) return EquipResult.InvalidTarget;

            if (unit.equippedItemId == itemId)
            {
                unit.equippedItemId = NoItem;
                return EquipResult.Unequipped;
            }

            if (GetOwnedCount(user, itemId) <= 0) return EquipResult.NotOwned;
            if (GetAvailableCount(user, itemId) <= 0) return EquipResult.ItemUnavailable;

            unit.equippedItemId = itemId;
            return EquipResult.Equipped;
        }

        public static EquipResult UnequipItem(Unit unit)
        {
            if (unit == null || unit.equippedItemId == NoItem) return EquipResult.InvalidTarget;

            unit.equippedItemId = NoItem;
            return EquipResult.Unequipped;
        }

        /// <summary>
        /// 보유 아이템 목록 (id 오름차순). 장착 여부를 지우지 않고 표시용으로 함께 담는다.
        /// categories가 비어 있으면 전체 카테고리.
        /// </summary>
        public static List<ItemEntryData> BuildItemEntries(
            User user,
            Unit selectedUnit,
            ItemDatabase itemDatabase,
            List<ItemCategory> categories)
        {
            var result = new List<ItemEntryData>();
            if (user?.inventory == null || itemDatabase == null) return result;

            bool filterByCategory = categories != null && categories.Count > 0;

            foreach (KeyValuePair<string, int> pair in user.inventory)
            {
                if (pair.Value <= 0) continue;
                if (!int.TryParse(pair.Key, out int itemId)) continue;

                ItemData item = itemDatabase.GetById(itemId);
                if (item == null) continue;
                if (filterByCategory && !categories.Contains(item.category)) continue;

                bool equippedByCurrent = selectedUnit != null && selectedUnit.equippedItemId == itemId;
                result.Add(new ItemEntryData(item, pair.Value, GetAvailableCount(user, itemId), equippedByCurrent));
            }

            result.Sort((a, b) => a.Item.id.CompareTo(b.Item.id));
            return result;
        }
    }
}
