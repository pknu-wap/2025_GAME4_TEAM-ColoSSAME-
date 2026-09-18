using System;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
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

        public MonoBehaviour CoroutineHost { get; set; }

        private readonly AddressableAssetLoader<Sprite> _portraitLoader = new();

        private Unit _currentUnit;

        public event Action<Unit> OnHealRequested;

        private void Awake()
        {
            healButton.onClick.AddListener(HandleHealClicked);
        }

        private void OnDestroy()
        {
            _portraitLoader.ReleaseAll();
        }

        public void ShowCharacter(Unit unit)
        {
            _currentUnit = unit;
            Refresh();
        }

        public void Clear()
        {
            _currentUnit = null;
            nameText.SetText(string.Empty);
            hpText.SetText(string.Empty);
            injuryStatusText.SetText(string.Empty);
            healingCostText.SetText(string.Empty);
            portraitImage.sprite = null;
            healButton.interactable = false;
        }
        public void Refresh()
        {
            if (_currentUnit == null)
            {
                Clear();
                return;
            }

            CharacterData characterData = CharacterInfoProvider.GetCharacterData(_currentUnit.Id);
            if (characterData == null)
            {
                Debug.LogWarning($"[HealingCharacterDetail] \uce90\ub9ad\ud130 \ub370\uc774\ud130\ub97c \ucc3e\uc744 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4: {_currentUnit.Id}");
                Clear();
                return;
            }

            nameText.SetText($"이름 : {characterData.Unit_Name}");
            bool isInjured = HealingService.Instance.IsInjured(_currentUnit);
            injuryStatusText.SetText($"상태 : {HealingService.Instance.GetInjuryStatusText(_currentUnit)}");

            int cost = HealingService.Instance.GetHealingCost(_currentUnit);
            healingCostText.SetText(isInjured ? $"비용 : {cost} G" : "비용 : -");

            bool hasEnoughGold = HealingService.Instance.HasEnoughGold(_currentUnit);
            healButton.interactable = isInjured && hasEnoughGold;

            MonoBehaviour host = CoroutineHost != null ? CoroutineHost : this;
            host.StartCoroutine(CharacterInfoProvider.LoadPortraitAsync(
                _portraitLoader,
                characterData,
                sprite => portraitImage.sprite = sprite,
                () => Debug.LogWarning($"[HealingCharacterDetail] \ud3ec\ud2b8\ub808\uc774\ud2b8 \ub85c\ub4dc \uc2e4\ud328: {characterData.Unit_ID}")
            ));
        }

        private void HandleHealClicked()
        {
            if (_currentUnit == null)
            {
                Debug.LogWarning("[HealingCharacterDetail] \uc120\ud0dd\ub41c \uce90\ub9ad\ud130\uac00 \uc5c6\uc2b5\ub2c8\ub2e4.");
                return;
            }

            OnHealRequested?.Invoke(_currentUnit);
        }
    }
}
