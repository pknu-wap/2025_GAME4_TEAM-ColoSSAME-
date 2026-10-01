using System;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using Colosseum.HealingCenter;   // TODO: 치료 UI 분리 시 제거 (HealingService 임시 의존)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Colosseum.Character
{
    public class CharacterDetail : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text agilityText;
        [SerializeField] private TMP_Text defenseText;
        [SerializeField] private TMP_Text healthText;
        
        [Header("Optional - HealingCenter 화면용 (비워두면 표시하지 않음)")]
        [SerializeField] private Button healButton;
        [SerializeField] private TMP_Text healingCostText;
        [SerializeField] private TMP_Text injuryStatusText;


        public MonoBehaviour CoroutineHost { get; set; }

        private readonly AddressableAssetLoader<Sprite> _portraitLoader = new();

        private Unit _currentUnit;

        public event Action<Unit> OnHealRequested;

        private void Awake()
        {
            if (healButton != null) healButton.onClick.AddListener(HandleHealClicked);
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
            SetText(nameText, string.Empty);
            SetText(hpText, string.Empty);
            SetText(injuryStatusText, string.Empty);
            SetText(healingCostText, string.Empty);
            SetText(levelText, string.Empty);
            SetText(attackText, string.Empty);
            SetText(agilityText, string.Empty);
            SetText(defenseText, string.Empty);
            SetText(healthText, string.Empty);
            if (portraitImage != null) portraitImage.sprite = null;
            if (healButton != null) healButton.interactable = false;
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
                Debug.LogWarning($"[CharacterDetail] \uce90\ub9ad\ud130 \ub370\uc774\ud130\ub97c \ucc3e\uc744 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4: {_currentUnit.Id}");
                Clear();
                return;
            }

            SetText(nameText, $"{characterData.Unit_Name}");
            RefreshHealingInfo();
            RefreshStats(characterData);

            MonoBehaviour host = CoroutineHost != null ? CoroutineHost : this;
            host.StartCoroutine(CharacterInfoProvider.LoadPortraitAsync(
                _portraitLoader,
                characterData,
                sprite => portraitImage.sprite = sprite,
                () => Debug.LogWarning($"[CharacterDetail] \ud3ec\ud2b8\ub808\uc774\ud2b8 \ub85c\ub4dc \uc2e4\ud328: {characterData.Unit_ID}")
            ));
        }

        // 치료소 전용 표시 (치료 UI가 연결되지 않았거나 HealingService가 없으면 건너뜀)
        private void RefreshHealingInfo()
        {
            if (injuryStatusText == null && healingCostText == null && healButton == null) return;
            if (HealingService.Instance == null) return;

            bool isInjured = HealingService.Instance.IsInjured(_currentUnit);
            SetText(injuryStatusText, $"상태 : {HealingService.Instance.GetInjuryStatusText(_currentUnit)}");

            int cost = HealingService.Instance.GetHealingCost(_currentUnit);
            SetText(healingCostText, isInjured ? $"비용 : {cost} G" : "비용 : -");

            bool hasEnoughGold = HealingService.Instance.HasEnoughGold(_currentUnit);
            if (healButton != null) healButton.interactable = isInjured && hasEnoughGold;
        }
        private void RefreshStats(CharacterData characterData)
        {
            SetText(levelText, $"LV : {_currentUnit.Level}");

            Stat_Distribution stats = characterData.Stat_Distribution;
            if (stats == null) return;

            SetText(attackText, $"공격력 : {stats.ATK}");
            SetText(agilityText, $"민첩 : {stats.AGI}");
            SetText(defenseText, $"방어력 : {stats.DEF}");
            SetText(healthText, $"체력 : {stats.HP}");
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null) target.SetText(value);
        }

        private void HandleHealClicked()
        {
            if (_currentUnit == null)
            {
                Debug.LogWarning("[CharacterDetail] \uc120\ud0dd\ub41c \uce90\ub9ad\ud130\uac00 \uc5c6\uc2b5\ub2c8\ub2e4.");
                return;
            }

            OnHealRequested?.Invoke(_currentUnit);
        }
    }
}
