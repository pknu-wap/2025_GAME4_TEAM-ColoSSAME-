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
        public StaticAICore OwnerAI;

        private void Update()
        {
            ApplyBattleStats();
        }
        
        public void Apply()
        {
            if (!OwnerAI || OwnerAI.Stat == null) return;

            if (CharacterImage) CharacterImage.sprite = OwnerAI.Stat.CharacterImage;
            if (NameText)   NameText.text   = $"{OwnerAI.Stat.Name}";
            ApplyBattleStats();
        }

        private void ApplyBattleStats()
        {
            if (!OwnerAI || OwnerAI.Stat == null) return;

            int attack = OwnerAI.IsInitialized ? OwnerAI.CurrentAttackDamage : OwnerAI.Stat.AttackDamage;
            int defense = OwnerAI.IsInitialized ? OwnerAI.CurrentDefense : OwnerAI.Stat.Defense;

            if (AtkText) AtkText.text = $"ATK: {attack}";
            if (DefText) DefText.text = $"DEF: {defense}";
        }
    }
}
