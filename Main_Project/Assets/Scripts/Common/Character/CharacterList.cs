using System;
using System.Collections.Generic;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using UnityEngine;

namespace Colosseum.Character
{
    public enum CharacterSortMode
    {
        Default,
        InjurySeverity,
        Rarity,
        Level
    }

    public class CharacterList : MonoBehaviour
    {
        [SerializeField] private List<CharacterItem> characterSlots;

        public MonoBehaviour CoroutineHost { get; set; }

        private readonly AddressableAssetLoader<Sprite> _portraitLoader = new();
        private readonly List<(Unit Unit, CharacterData Data)> _ownedUnits = new();

        private string _selectedUnitId;
        private CharacterSortMode _sortMode = CharacterSortMode.Default;

        public event Action<Unit> OnCharacterSelected;

        private void OnDestroy()
        {
            _portraitLoader.ReleaseAll();
        }

        public void Refresh()
        {
            CollectOwnedUnitsInCurrentFamily();

            int slotIndex = 0;
            for (int i = 0; i < _ownedUnits.Count; i++)
            {
                if (slotIndex >= characterSlots.Count)
                {
                    Debug.LogWarning("[CharacterList] \uc2ac\ub86f \uac1c\uc218\uac00 \ubd80\uc871\ud569\ub2c8\ub2e4. \uc778\uc2a4\ud399\ud130\uc5d0\uc11c \uc2ac\ub86f\uc744 \ucd94\uac00\ud558\uc138\uc694.");
                    break;
                }

                (Unit unit, CharacterData data) = _ownedUnits[i];
                characterSlots[slotIndex].SetData(unit, data, _portraitLoader, CoroutineHost, HandleSlotSelected);
                characterSlots[slotIndex].SetSelected(unit.Id == _selectedUnitId);
                slotIndex++;
            }

            for (int i = slotIndex; i < characterSlots.Count; i++)
            {
                characterSlots[i].Hide();
            }
        }

        private void RefreshHighlightOnly()
        {
            foreach (CharacterItem slot in characterSlots)
            {
                if (slot.gameObject.activeSelf)
                {
                    slot.SetSelected(slot.UnitId == _selectedUnitId);
                }
            }
        }

        public void RefreshSelectedSlotStatus()
        {
            foreach (CharacterItem slot in characterSlots)
            {
                if (slot.gameObject.activeSelf && slot.UnitId == _selectedUnitId)
                {
                    slot.RefreshStatus();
                    break;
                }
            }
        }

        public void SortByDefault() => SetSortMode(CharacterSortMode.Default);
        public void SortByInjurySeverity() => SetSortMode(CharacterSortMode.InjurySeverity);
        public void SortByRarity() => SetSortMode(CharacterSortMode.Rarity);
        public void SortByLevel() => SetSortMode(CharacterSortMode.Level);

        private void SetSortMode(CharacterSortMode mode)
        {
            if (_sortMode == mode) return;

            _sortMode = mode;
            Refresh();
        }

        public void Select(Unit unit)
        {
            if (unit == null) return;
            HandleSlotSelected(unit);
        }

        public Unit GetFirstUnit() => _ownedUnits.Count > 0 ? _ownedUnits[0].Unit : null;

        private void HandleSlotSelected(Unit unit)
        {
            _selectedUnitId = unit.Id;
            RefreshHighlightOnly();
            OnCharacterSelected?.Invoke(unit);
        }
        private void CollectOwnedUnitsInCurrentFamily()
        {
            _ownedUnits.Clear();

            string currentFamilyId = FamilyUtility.GetCurrentFamilyId();
            if (string.IsNullOrEmpty(currentFamilyId))
            {
                return;
            }

            List<Unit> myUnits = UserManager.Instance.user.myUnits;
            if (myUnits == null)
            {
                return;
            }

            foreach (Unit unit in myUnits)
            {
                CharacterData data = CharacterInfoProvider.GetCharacterData(unit.Id);
                if (data != null && data.Family_ID == currentFamilyId)
                {
                    _ownedUnits.Add((unit, data));
                }
            }

            SortOwnedUnits();
        }

        private void SortOwnedUnits()
        {
            if (_sortMode == CharacterSortMode.Default)
            {
                return;
            }

            _ownedUnits.Sort(GetComparer());
        }

        private Comparison<(Unit Unit, CharacterData Data)> GetComparer() => _sortMode switch
        {
            CharacterSortMode.Rarity => CompareByRarity,
            CharacterSortMode.Level => CompareByLevel,
            _ => CompareByInjurySeverity
        };

        private static int CompareByInjurySeverity((Unit Unit, CharacterData Data) a, (Unit Unit, CharacterData Data) b)
        {
            int result = ((int)b.Unit.currentInjury).CompareTo((int)a.Unit.currentInjury);
            if (result != 0) return result;

            result = b.Unit.Tier.CompareTo(a.Unit.Tier);
            return result != 0 ? result : b.Unit.Level.CompareTo(a.Unit.Level);
        }

        private static int CompareByRarity((Unit Unit, CharacterData Data) a, (Unit Unit, CharacterData Data) b)
        {
            int result = b.Unit.Tier.CompareTo(a.Unit.Tier);
            if (result != 0) return result;

            result = ((int)b.Unit.currentInjury).CompareTo((int)a.Unit.currentInjury);
            return result != 0 ? result : b.Unit.Level.CompareTo(a.Unit.Level);
        }

        private static int CompareByLevel((Unit Unit, CharacterData Data) a, (Unit Unit, CharacterData Data) b)
        {
            int result = b.Unit.Level.CompareTo(a.Unit.Level);
            if (result != 0) return result;

            result = ((int)b.Unit.currentInjury).CompareTo((int)a.Unit.currentInjury);
            return result != 0 ? result : b.Unit.Tier.CompareTo(a.Unit.Tier);
        }
    }
}
