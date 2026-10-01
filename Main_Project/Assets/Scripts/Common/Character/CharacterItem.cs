using System;
using BattleK.Scripts.Data;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Colosseum.Character
{
    public class CharacterItem : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [FormerlySerializedAs("hpText")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedHighlight;

        private Unit _unit;
        private Action<Unit> _onSelected;

        public string UnitId => _unit?.Id;

        private void Awake()
        {
            if (selectButton == null) selectButton = GetComponent<Button>();
            if (selectButton != null) selectButton.onClick.AddListener(HandleClick);
        }

        public void SetData(Unit unit, CharacterData data, AddressableAssetLoader<Sprite> portraitLoader, MonoBehaviour coroutineHost, Action<Unit> onSelected)
        {
            _unit = unit;
            _onSelected = onSelected;

            if (statusText != null) statusText.SetText(InjuryStatusLocalization.GetDisplayName(unit.currentInjury));

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
            if (_unit == null || statusText == null) return;
            statusText.SetText(InjuryStatusLocalization.GetDisplayName(_unit.currentInjury));
        }

        private void LoadPortrait(CharacterData data, AddressableAssetLoader<Sprite> portraitLoader, MonoBehaviour coroutineHost)
        {
            MonoBehaviour host = coroutineHost != null ? coroutineHost : this;
            host.StartCoroutine(CharacterInfoProvider.LoadPortraitAsync(
                portraitLoader,
                data,
                sprite => portraitImage.sprite = sprite,
                () => Debug.LogWarning($"[CharacterItem] \ud3ec\ud2b8\ub808\uc774\ud2b8 \ub85c\ub4dc \uc2e4\ud328: {data?.Unit_ID}")
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
