using BattleK.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class StatWindow : MonoBehaviour
    {
        [Header("이름 텍스트")]
        public TextMeshProUGUI NameText;

        [Header("스탯 텍스트")]
        public TextMeshProUGUI AtkText;
        public TextMeshProUGUI DefText;
        public TextMeshProUGUI InjuredText;

        [Header("캐릭터 이미지")]
        public Image CharacterImage;

        public string UnitId { get; private set; }

        public void SetPending(string unitId)
        {
            UnitId = unitId;
        }

        public void SetFromRepository(UnitDisplayInfo info)
        {
            UnitId = info.UnitId;

            if (CharacterImage) CharacterImage.sprite = info.CharacterImage;
            if (NameText) NameText.text = info.UnitName;

            if (AtkText) AtkText.text = $"ATK: {info.Stat.AttackDamage}";
            if (DefText) DefText.text = $"DEF: {info.Stat.Defense}";
            if (InjuredText) InjuredText.text = $"HP: {info.Stat.CurrentHp} / {info.Stat.MaxHp}";
        }
    }
}