using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class StatWindow : MonoBehaviour
    {
        [Header("이름 텍스트")]
        public TextMeshProUGUI NameText;

        [Header("캐릭터 팀 설정")]
        public LayerMask TeamLayer;

        [Header("스탯 텍스트")]
        public TextMeshProUGUI AtkText;
        public TextMeshProUGUI DefText;
        public TextMeshProUGUI InjuredText; // 여기에는 HP를 표시하도록 사용

        [Header("캐릭터 이미지")]
        public Image CharacterImage;

        [Header("매칭용 (선택)")]
        [Tooltip("CalculateManager의 CharacterStatsRow.Unit_ID 와 일치시키면 정확 매칭. 비우면 인덱스 순서로 매칭.")]
        public string UnitId;
        
        public void Apply(FamilyStatsCollector.CharacterStatsRow row)
        {
            if (row == null) return;

            if (NameText)   NameText.text   = string.IsNullOrEmpty(row.Unit_Name) ? row.Unit_ID : row.Unit_Name;
            if (AtkText)    AtkText.text    = $"ATK: {row.ATK}";
            if (DefText)    DefText.text    = $"DEF: {row.DEF}";
        }
    }
}