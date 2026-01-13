using BattleK.Scripts.AI;
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
        public TextMeshProUGUI InjuredText; // 여기에는 HP를 표시하도록 사용

        [Header("캐릭터 이미지")]
        public Image CharacterImage;
        [Header("AICore")]
        public AICore OwnerAI;
        
        public void Apply()
        {
            if (CharacterImage) CharacterImage.sprite = OwnerAI.Image;
            if (NameText)   NameText.text   = $"{OwnerAI.Ko_Name}";
            if (AtkText)    AtkText.text    = $"ATK: {OwnerAI.attackDamage}";
            if (DefText)    DefText.text    = $"DEF: {OwnerAI.def}";
        }
    }
}