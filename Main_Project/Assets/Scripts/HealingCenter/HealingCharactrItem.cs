using System;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Colosseum.HealingCenter
{
    public class HealingCharacterItem : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedHighlight;

        private Unit _unit;
        private Action<Unit> _onSelected;

        public string UnitId => _unit?.Id;

        private void Awake()
        {
            selectButton.onClick.AddListener(HandleClick);
        }

        public void SetData(Unit unit, CharacterData data, AddressableAssetLoader<Sprite> portraitLoader, MonoBehaviour coroutineHost, Action<Unit> onSelected)
        {
            _unit = unit;
            _onSelected = onSelected;

            hpText.SetText(InjuryStatusLocalization.GetDisplayName(unit.currentInjury));

            gameObject.SetActive(true);

            LoadPortrait(data, portraitLoader, coroutineHost);
        }

        public void Hide()
        {
            _unit = null;
            gameObject.SetActive(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(isSelected);
            }
        }

        public void RefreshStatus()
        {
            if (_unit == null) return;
            hpText.SetText(InjuryStatusLocalization.GetDisplayName(_unit.currentInjury));
        }

        private void LoadPortrait(CharacterData data, AddressableAssetLoader<Sprite> portraitLoader, MonoBehaviour coroutineHost)
        {
            MonoBehaviour host = coroutineHost != null ? coroutineHost : this;
            host.StartCoroutine(CharacterInfoProvider.LoadPortraitAsync(
                portraitLoader,
                data,
                sprite => portraitImage.sprite = sprite,
                () => Debug.LogWarning($"[HealingCharacterItem] \ud3ec\ud2b8\ub808\uc774\ud2b8 \ub85c\ub4dc \uc2e4\ud328: {data?.Unit_ID}")
            ));
        }

        private void HandleClick()
        {
            if (_unit != null)
            {
                _onSelected?.Invoke(_unit);
            }
        }
    }
}
