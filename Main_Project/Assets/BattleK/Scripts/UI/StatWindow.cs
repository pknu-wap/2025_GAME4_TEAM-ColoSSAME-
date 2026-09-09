using BattleK.Scripts.AI;
using BattleK.Scripts.AI.CCState;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class StatWindow : MonoBehaviour
    {
        [Header("Name")]
        public TextMeshProUGUI NameText;

        [Header("Tier/Level")]
        public TextMeshProUGUI TierText;
        public TextMeshProUGUI LevelText;

        [Header("StatText")]
        public TextMeshProUGUI AtkText;
        public TextMeshProUGUI DefText;
        public TextMeshProUGUI HPText;

        [Header("CharacterImage")]
        public Image CharacterImage;

        [Header("Class")]
        public ClassIconTable ClassIconTable;
        public Image ClassIcon;

        public string UnitId { get; private set; }

        private StaticAICore _boundCore;

        private int _baseAttackDamage;
        private int _baseDefense;

        public void SetPending(string unitId)
        {
            UnbindCore();
            UnitId = unitId;
        }

        public void SetFromRepository(UnitDisplayInfo info)
        {
            UnbindCore();

            UnitId = info.UnitId;
            _baseAttackDamage = info.Stat.AttackDamage;
            _baseDefense = info.Stat.Defense;

            if (NameText) NameText.text = info.UnitName;
            if (TierText) TierText.text = info.Tier.ToString();
            if (LevelText) LevelText.text = info.Level.ToString();
            if (CharacterImage) CharacterImage.sprite = info.CharacterImage;
            if (ClassIcon && ClassIconTable)
            {
                var icon = ClassIconTable.GetIcon(info.UnitClass);
                ClassIcon.sprite = icon;
                ClassIcon.enabled = icon != null;
            }

            if (AtkText) AtkText.text = $"ATK: {info.Stat.AttackDamage}";
            if (DefText) DefText.text = $"DEF: {info.Stat.Defense}";
            if (HPText) HPText.text = $"HP: {info.Stat.MaxHp}";
        }

        public void BindToCore(StaticAICore core, UnitDisplayInfo info)
        {
            SetFromRepository(info);

            _boundCore = core;
            if (!_boundCore) return;

            _boundCore.OnStatChanged += HandleStatChanged;
            HandleStatChanged();
        }

        private void HandleStatChanged()
        {
            if (!_boundCore) return;

            var atk = _boundCore.GetDisplayValue(FlatStatusType.AttackDamageFlat, StatusType.AttackDamageMultiplier, _boundCore.runtimeStat.AttackDamage);
            var def = _boundCore.GetDisplayValue(FlatStatusType.DefenseFlat, StatusType.DefenseMultiplier, _boundCore.runtimeStat.Defense);

            if (AtkText) AtkText.text = FormatWithDelta("ATK", atk.Total, atk.Delta);
            if (DefText) DefText.text = FormatWithDelta("DEF", def.Total, def.Delta);
            if (HPText) HPText.text = $"HP: {_boundCore.runtimeStat.CurrentHP}/{_boundCore.CurrentMaxHp}";
        }

        private static string FormatWithDelta(string label, float currentValue, float delta)
        {
            if (Mathf.Abs(delta) < 0.01f)
                return $"{label}: {currentValue:0}";

            var sign = delta > 0 ? "+" : "-";
            return $"{label}: {currentValue:0} ({sign}{Mathf.Abs(delta):0})";
        }

        private void UnbindCore()
        {
            if (!_boundCore) return;

            _boundCore.OnStatChanged -= HandleStatChanged;
            _boundCore = null;
        }

        private void OnDisable() => UnbindCore();
        private void OnDestroy() => UnbindCore();
    }
}