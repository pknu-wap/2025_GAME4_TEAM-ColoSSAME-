using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

        public void SetPending(string unitId)
        {
            UnitId = unitId;
        }

        public void SetFromRepository(UnitDisplayInfo info)
        {
            UnitId = info.UnitId;

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
    }
}