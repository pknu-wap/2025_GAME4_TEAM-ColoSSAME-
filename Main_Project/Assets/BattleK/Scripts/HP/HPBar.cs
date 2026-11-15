using BattleK.Scripts.AI;
using BattleK.Scripts.Combat;
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
        private UnitHealth _health;

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

            if (OwnerAi) UpdateHPBar();
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
        
        public void Bind(StaticAICore core)
        {
            Unsubscribe();
            OwnerAi = core;
            _health = core ? core.Health : null;

            if (!isActiveAndEnabled) return;
            Subscribe();
            UpdateHPBar();
        }

        private void OnEnable()
        {
            if (_health == null && OwnerAi) _health = OwnerAi.Health;
            Subscribe();
            UpdateHPBar();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_health == null) return;
            _health.Changed -= OnHealthChanged;
            _health.Changed += OnHealthChanged;
        }

        private void Unsubscribe()
        {
            if (_health != null) _health.Changed -= OnHealthChanged;
        }

        private void OnHealthChanged(UnitHealth health)
        {
            UpdateHPBar();
        }

        public void UpdateHPBar()
        {
            if (!hpSlider || !OwnerAi) return;
            hpSlider.value = OwnerAi.Health.Ratio;
        }
    }
}