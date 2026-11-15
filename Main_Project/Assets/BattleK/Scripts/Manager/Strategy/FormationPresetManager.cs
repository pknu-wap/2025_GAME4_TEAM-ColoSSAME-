using System;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.Data.Type;
using BattleK.Scripts.Manager.Strategy.Runtime.PlayerStrategy;
using BattleK.Scripts.UI;
using UnityEngine;

namespace BattleK.Scripts.Manager.Strategy
{
    public class FormationPresetManager : MonoBehaviour
    {
        [Header("Manager")]
        [SerializeField] private FormationManager formationManager;
        [SerializeField] private SlotManager slotManager;

        [Header("배치 가능한 전체 유닛 UI")]
        [Tooltip("배치된 유닛과 미배치 유닛을 모두 연결하세요.")]
        [SerializeField] private UIDrag[] availableUnits;
        
        [Header("저장 슬롯 선택")]
        [Tooltip("0이면 미선택 상태입니다. 저장 슬롯 번호는 1부터 사용합니다.")]
        [SerializeField] private int selectedSaveSlotNumber;

        public int SelectedSaveSlotNumber => selectedSaveSlotNumber;

        public void SelectSaveSlot(int saveSlotNumber)
        {
            if (saveSlotNumber < 1) return;
            selectedSaveSlotNumber = saveSlotNumber;
        }
        
        public void SaveSelectedPreset()
        {
            if (!ValidateSelectedSaveSlot()) return;
            SavePreset(selectedSaveSlotNumber);
        }
        
        public void LoadSelectedPreset()
        {
            if (!ValidateSelectedSaveSlot()) return;
            LoadPreset(selectedSaveSlotNumber);
        }

        private bool ValidateSelectedSaveSlot()
        {
            return selectedSaveSlotNumber >= 1;
        }
        
        private void SavePreset(int saveSlotNumber)
        {
            if (!TryGetContext(saveSlotNumber, out var user, out var formation)) return;
            if (!TryCollectPlacement(user, out var unitKeys)) return;
            user.formationPresets ??= new List<FormationPresetData>();
            var preset = new FormationPresetData
            {
                formationId = formation.FormationId,
                saveSlotNumber = saveSlotNumber,
                unitKeysBySlot = unitKeys
            };
            var existingIndex = FindPresetIndex(user.formationPresets, formation.FormationId, saveSlotNumber);
            var previousPreset = existingIndex >= 0 ? user.formationPresets[existingIndex] : null;
            if (existingIndex >= 0)
            {
                user.formationPresets[existingIndex] = preset;
            }
            else
            {
                user.formationPresets.Add(preset);
            }
            try
            {
                UserManager.Instance.SaveUser();
            }
            catch (Exception)
            {
                if (existingIndex >= 0)
                {
                    user.formationPresets[existingIndex] = previousPreset;
                }
                else
                {
                    user.formationPresets.Remove(preset);
                }
            }
        }

        private void LoadPreset(int saveSlotNumber)
        {
            if (!TryGetContext(saveSlotNumber, out var user, out var formation)) return;

            var presetIndex = FindPresetIndex(user.formationPresets, formation.FormationId, saveSlotNumber);
            if (presetIndex < 0) return;

            var preset = user.formationPresets[presetIndex];
            if (!TryBuildPlacementPlan(user, preset, out var placementPlan)) return;
            slotManager.ClearAllSlots();
            formationManager.ApplyFormationAsset(formation);
            for (var i = 0; i < placementPlan.Length; i++)
            {
                var unit = placementPlan[i];
                if (!unit) continue;
                if (slotManager.TryGetSlot(i, out var slot) && unit.TryPlaceInSlot(slot)) continue;
                return;
            }
        }

        private bool TryGetContext(int saveSlotNumber, out User user, out FormationAsset formation)
        {
            user = null;
            formation = null;

            if (saveSlotNumber < 1) return false;
            if (UserManager.Instance == null || UserManager.Instance.user == null) return false;
            if (!formationManager || !slotManager) return false;

            formation = formationManager.CurrentFormation;
            if (!formation || string.IsNullOrWhiteSpace(formation.FormationId)) return false;
            if (formation.uiAnchoredPositions == null || formation.uiAnchoredPositions.Length == 0) return false;
            if (slotManager.SlotCount == 0) return false;

            var registeredSlots = new HashSet<Slot>();
            for (var i = 0; i < slotManager.SlotCount; i++)
            {
                if (!slotManager.TryGetSlot(i, out var slot) || !registeredSlots.Add(slot)) return false;
            }
            user = UserManager.Instance.user;
            return true;
        }

        private bool TryCollectPlacement(User user, out string[] unitKeys)
        {
            unitKeys = new string[slotManager.SlotCount];
            var usedKeys = new HashSet<string>();
            for (var i = 0; i < unitKeys.Length; i++)
            {
                if (!slotManager.TryGetSlot(i, out var slot)) return false;
                var unit = slot.Occupant;
                if (!unit)
                {
                    unitKeys[i] = string.Empty;
                    continue;
                }
                var key = unit.UnitKey;
                if (unit.CurrentSlot != slot || string.IsNullOrWhiteSpace(key) || !usedKeys.Add(key)) return false;
                if (!IsOwnedUnit(user, key)) return false;
                unitKeys[i] = key;
            }
            return true;
        }

        private bool TryBuildPlacementPlan(User user, FormationPresetData preset, out UIDrag[] placementPlan)
        {
            placementPlan = null;
            if (preset.unitKeysBySlot == null || preset.unitKeysBySlot.Length != slotManager.SlotCount) return false;
            if (!TryBuildUnitLookup(out var unitLookup)) return false;
            var result = new UIDrag[slotManager.SlotCount];
            var usedKeys = new HashSet<string>();
            for (var i = 0; i < preset.unitKeysBySlot.Length; i++)
            {
                var key = preset.unitKeysBySlot[i];
                if (string.IsNullOrEmpty(key)) continue;
                if (!usedKeys.Add(key)) return false;
                if (!IsOwnedUnit(user, key)) return false;
                if (!unitLookup.TryGetValue(key, out var unit)) return false;
                result[i] = unit;
            }
            placementPlan = result;
            return true;
        }

        private bool TryBuildUnitLookup(out Dictionary<string, UIDrag> unitLookup)
        {
            unitLookup = new Dictionary<string, UIDrag>();
            if (availableUnits == null || availableUnits.Length == 0) return false;
            foreach (var unit in availableUnits)
            {
                if (!unit) continue;
                var key = unit.UnitKey;
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (!unitLookup.TryAdd(key, unit)) return false;
            }
            return true;
        }

        private bool IsOwnedUnit(User user, string unitKey)
        {
            var matchCount = user.myUnits?.Count(unit => unit != null && unit.Id == unitKey);
            return matchCount == 1;
        }

        private int FindPresetIndex(List<FormationPresetData> presets, string formationId, int saveSlotNumber)
        {
            if (presets == null) return -1;
            return presets.FindIndex(preset =>
                preset != null &&
                preset.formationId == formationId &&
                preset.saveSlotNumber == saveSlotNumber);
        }
    }
}