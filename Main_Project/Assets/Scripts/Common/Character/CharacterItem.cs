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
        private int _portraitRequestVersion;

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
            _portraitRequestVersion++;
            gameObject.SetActive(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(isSelected);
            }
        }

        private void LoadPortrait(CharacterData data, AddressableAssetLoader<Sprite> portraitLoader, MonoBehaviour coroutineHost)
        {
            int requestVersion = ++_portraitRequestVersion;
            portraitImage.sprite = null;

            MonoBehaviour host = coroutineHost != null ? coroutineHost : this;
            host.StartCoroutine(CharacterInfoProvider.LoadPortraitAsync(
                portraitLoader,
                data,
                sprite =>
                {
                    if (requestVersion == _portraitRequestVersion) portraitImage.sprite = sprite;
                },
                () =>
                {
                    if (requestVersion == _portraitRequestVersion) portraitImage.sprite = null;
                    Debug.LogWarning($"[CharacterItem] 포트레이트 로드 실패: {data?.Unit_ID}");
                }
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
