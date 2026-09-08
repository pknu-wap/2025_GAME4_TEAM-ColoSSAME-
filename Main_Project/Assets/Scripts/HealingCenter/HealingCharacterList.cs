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

        private readonly AddressableAssetLoader<Sprite> _portraitLoader = new();

        private string _selectedUnitId;
        
        public event Action<string> OnCharacterSelected;

        private void OnDestroy()
        {
            _portraitLoader.ReleaseAll();
        }

        public void Refresh()
        {
            List<CharacterData> ownedCharacters = GetOwnedCharacters();

            int slotIndex = 0;

            for (int i = 0; i < ownedCharacters.Count; i++)
            {
                if (slotIndex >= characterSlots.Count)
                {
                    Debug.LogWarning("[HealingCharacterList] 슬롯 개수가 부족합니다. 인스펙터에서 슬롯을 추가하세요.");
                    break;
                }

                CharacterData characterData = ownedCharacters[i];

                Unit myUnit = UserManager.Instance.GetMyUnitById(characterData.Unit_ID);
                if (myUnit == null)
                {
                    continue;
                }

                HealingCharacterItem slot = characterSlots[slotIndex];
                slot.SetData(
                    characterData.Unit_ID,
                    characterData.Unit_Name,
                    // myUnit.currentHp,
                    characterData.Unit_Name, // TODO: 포트레이트 전용 필드가 따로 있다면 그 필드로 교체
                    _portraitLoader,
                    HandleSlotSelected
                );
                slot.SetSelected(characterData.Unit_ID == _selectedUnitId);

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

        private void HandleSlotSelected(string unitId)
        {
            _selectedUnitId = unitId;
            RefreshHighlightOnly();
            OnCharacterSelected?.Invoke(unitId);
        }

        private List<CharacterData> GetOwnedCharacters()
        {
            string currentFamilyId = FamilyUtility.GetCurrentFamilyId();

            if (string.IsNullOrEmpty(currentFamilyId))
            {
                return new List<CharacterData>();
            }

            List<CharacterData> familyUnits = UnitDataManager.Instance.GetFamilyUnits(currentFamilyId);

            if (familyUnits == null)
            {
                return new List<CharacterData>();
            }

            List<CharacterData> owned = new List<CharacterData>();

            foreach (CharacterData characterData in familyUnits)
            {
                if (UserManager.Instance.GetMyUnitById(characterData.Unit_ID) != null)
                {
                    owned.Add(characterData);
                }
            }

            return owned;
        }
    }
}