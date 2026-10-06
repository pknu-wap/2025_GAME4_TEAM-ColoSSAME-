using System;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using Colosseum.HealingCenter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Colosseum.Character
{
    public class CharacterDetail : MonoBehaviour
    {
        [Header("캐릭터 기본 정보")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [Header("이전 씬의 체력 텍스트 (초기화 전용)")]
        [FormerlySerializedAs("hpText")]
        [SerializeField] private TMP_Text legacyHpText;

        [Header("레벨 및 기본 능력치")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text agilityText;
        [SerializeField] private TMP_Text defenseText;
        [FormerlySerializedAs("healthText")]
        [SerializeField] private TMP_Text baseHealthText;
        
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
            if (healButton != null) healButton.onClick.RemoveListener(HandleHealClicked);
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
            SetText(legacyHpText, string.Empty);
            SetText(injuryStatusText, string.Empty);
            SetText(healingCostText, string.Empty);
            SetText(levelText, string.Empty);
            SetText(attackText, string.Empty);
            SetText(agilityText, string.Empty);
            SetText(defenseText, string.Empty);
            SetText(baseHealthText, string.Empty);
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
            SetText(baseHealthText, $"체력 : {stats.HP}");
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
