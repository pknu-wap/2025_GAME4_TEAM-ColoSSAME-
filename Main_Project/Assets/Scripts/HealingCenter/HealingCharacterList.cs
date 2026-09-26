using System;
using System.Collections.Generic;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using UnityEngine;

namespace Colosseum.HealingCenter
{
    public class HealingCharacterList : MonoBehaviour
    {
        [SerializeField] private List<HealingCharacterItem> characterSlots;

        public MonoBehaviour CoroutineHost { get; set; }

        private readonly AddressableAssetLoader<Sprite> _portraitLoader = new();
        private readonly List<(Unit Unit, CharacterData Data)> _ownedUnits = new();

        private string _selectedUnitId;

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
                    Debug.LogWarning("[HealingCharacterList] \uc2ac\ub86f \uac1c\uc218\uac00 \ubd80\uc871\ud569\ub2c8\ub2e4. \uc778\uc2a4\ud399\ud130\uc5d0\uc11c \uc2ac\ub86f\uc744 \ucd94\uac00\ud558\uc138\uc694.");
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

        public void RefreshHighlightOnly()
        {
            foreach (HealingCharacterItem slot in characterSlots)
            {
                if (slot.gameObject.activeSelf)
                {
                    slot.SetSelected(slot.UnitId == _selectedUnitId);
                }
            }
        }

        // \ud798 \ud6c4 \ud574\ub2f9 \uc720\ub2db \uc2ac\ub86f\uc758 \ubd80\uc0c1 \ud14d\uc2a4\ud2b8\ub9cc \uac31\uc2e0 (\uc804\uccb4 Refresh \ubcf4\ub2e4 \uac00\ubcbc\uc74c)
        public void RefreshSelectedSlotStatus()
        {
            foreach (HealingCharacterItem slot in characterSlots)
            {
                if (slot.gameObject.activeSelf && slot.UnitId == _selectedUnitId)
                {
                    slot.RefreshStatus();
                    break;
                }
            }
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
        }
    }
}
