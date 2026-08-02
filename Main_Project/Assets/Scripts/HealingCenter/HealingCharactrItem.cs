using System;
using BattleK.Scripts.Data;
using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Colosseum.HealingCenter
{
  
    public class HealingCharacterItem : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedHighlight;

        private string _unitId;
        private Action<string> _onSelected;

        public string UnitId => _unitId;

        private void Awake()
        {
            selectButton.onClick.AddListener(HandleClick);
        }
        
        // curHp 추가 에정
        public void SetData(string unitId, string characterName, string portraitAssetName, AddressableAssetLoader<Sprite> portraitLoader, Action<string> onSelected)
        {
            _unitId = unitId;
            _onSelected = onSelected;

            nameText.SetText(characterName);
            // hpText.SetText($"HP {currentHp}");

            LoadPortrait(portraitAssetName, portraitLoader);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _unitId = null;
            gameObject.SetActive(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(isSelected);
            }
        }

        private void LoadPortrait(string portraitAssetName, AddressableAssetLoader<Sprite> portraitLoader)
        {
            if (string.IsNullOrEmpty(portraitAssetName))
            {
                return;
            }

            StartCoroutine(portraitLoader.LoadAsync(
                AddressableAssetType.Character,
                portraitAssetName,
                sprite => portraitImage.sprite = sprite,
                () => Debug.LogWarning($"[HealingCharacterItem] 포트레이트 로드 실패: {portraitAssetName}")
            ));
        }

        private void HandleClick()
        {
            _onSelected?.Invoke(_unitId);
        }
    }
}