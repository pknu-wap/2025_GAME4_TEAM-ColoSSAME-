using System;
using BattleK.Scripts.AI;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace BattleK.Scripts.HP
{
    public class HPBar : MonoBehaviour
    {
        public StaticAICore OwnerAi;

        [SerializeField] private Slider hpSlider;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image shieldFillImage;

        [Header("UI Settings")]
        [SerializeField] private Canvas hpsCanvas;

        private Vector3 _originalScale;

        private void Awake()
        {
            if (!hpSlider) hpSlider = GetComponent<Slider>();
            if (!hpFillImage && hpSlider) hpFillImage = hpSlider.fillRect ? hpSlider.fillRect.GetComponent<Image>() : null;
            if (!hpsCanvas) hpsCanvas = GetComponentInParent<Canvas>(true);
            _originalScale = transform.localScale;

            hpsCanvas.worldCamera = FindObjectOfType<Camera>();

            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.wholeNumbers = false;
            hpSlider.interactable = false;

            if (shieldFillImage) shieldFillImage.fillAmount = 0f;

            if (OwnerAi && OwnerAi.Stat.MaxHP > 0)
                hpSlider.value = Mathf.Clamp01((float)OwnerAi.Stat.CurrentHP / OwnerAi.Stat.MaxHP);
        }

        private void LateUpdate()
        {
            transform.rotation = Quaternion.identity;

            if (!transform.parent) return;
            var newScale = _originalScale;

            newScale.x = transform.parent.lossyScale.x < 0
                ? -Mathf.Abs(_originalScale.x)
                : Mathf.Abs(_originalScale.x);

            transform.localScale = newScale;
            hpsCanvas.transform.localPosition = new Vector3(0, -0.4f, 0);
        }

        public void UpdateHPBar()
        {
            if (!hpSlider || !OwnerAi) return;
            var v = (OwnerAi.Stat.MaxHP <= 0) ? 0f : Mathf.Clamp01((float)OwnerAi.Stat.CurrentHP / OwnerAi.Stat.MaxHP);
            hpSlider.value = v;
        }

        public void UpdateShieldBar(int currentShield, int maxHp)
        {
            if (!shieldFillImage || maxHp <= 0) return;
            shieldFillImage.fillAmount = Mathf.Clamp01((float)currentShield / maxHp);
        }

        public void SetFillColor(Color color)
        {
            if (hpFillImage) hpFillImage.color = color;
        }

        public void SetShieldColor(Color color)
        {
            if (shieldFillImage) shieldFillImage.color = color;
        }
    }
}