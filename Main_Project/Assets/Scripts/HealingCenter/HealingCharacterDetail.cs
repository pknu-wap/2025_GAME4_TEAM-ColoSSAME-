using System;
using BattleK.Scripts.Data;
using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Colosseum.HealingCenter
{
    public class HealingCharacterDetail : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text injuryStatusText;
        [SerializeField] private TMP_Text healingCostText;
        [SerializeField] private Button healButton;

        private readonly AddressableAssetLoader<Sprite> _portraitLoader = new();

        private string _currentUnitId;
        
        public event Action<string> OnHealRequested;

        private void Awake()
        {
            healButton.onClick.AddListener(HandleHealClicked);
        }

        private void OnDestroy()
        {
            _portraitLoader.ReleaseAll();
        }

        public void ShowCharacter(string unitId)
        {
            _currentUnitId = unitId;
            Refresh();
        }

        public void Clear()
        {
            _currentUnitId = null;
            nameText.SetText(string.Empty);
            hpText.SetText(string.Empty);
            injuryStatusText.SetText(string.Empty);
            healingCostText.SetText(string.Empty);
            portraitImage.sprite = null;
            healButton.interactable = false;
        }
        
        public void Refresh()
        {
            if (string.IsNullOrEmpty(_currentUnitId))
            {
                Clear();
                return;
            }

            CharacterData characterData = UnitDataManager.Instance.GetCharacterData(_currentUnitId);
            Unit myUnit = UserManager.Instance.GetMyUnitById(_currentUnitId);

            if (characterData == null || myUnit == null)
            {
                Debug.LogWarning($"[HealingCharacterDetail] 캐릭터 데이터를 찾을 수 없습니다: {_currentUnitId}");
                Clear();
                return;
            }

            nameText.SetText(characterData.Unit_Name);
            // hpText.SetText($"HP {myUnit.currentHp}"); // TODO: 실제 HP 필드명 확인 필요

            bool isInjured = HealingService.Instance.IsInjured(_currentUnitId);
            injuryStatusText.SetText(HealingService.Instance.GetInjuryStatusText(_currentUnitId));

            int cost = HealingService.Instance.GetHealingCost(_currentUnitId);
            healingCostText.SetText(isInjured ? $"{cost} G" : "-");

            healButton.interactable = isInjured;

            StartCoroutine(_portraitLoader.LoadAsync(
                AddressableAssetType.Character,
                characterData.Unit_Name, // TODO: 포트레이트 전용 필드가 따로 있다면 그 필드로 교체
                sprite => portraitImage.sprite = sprite,
                () => Debug.LogWarning($"[HealingCharacterDetail] 포트레이트 로드 실패: {characterData.Unit_Name}")
            ));
        }

        private void HandleHealClicked()
        {
            if (string.IsNullOrEmpty(_currentUnitId))
            {
                Debug.LogWarning("[HealingCharacterDetail] 선택된 캐릭터가 없습니다.");
                return;
            }

            OnHealRequested?.Invoke(_currentUnitId);
        }
    }
}
