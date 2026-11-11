using UnityEngine;

namespace BattleK.Scripts.Manager.Strategy.Runtime.PlayerStrategy
{
    [CreateAssetMenu(menuName = "Battle/Formation/Asset", fileName = "FM_")]
    public class FormationAsset : ScriptableObject
    {
        [Header("전략 식별")]
        [Tooltip("저장 데이터를 구분하는 고유 ID" + "다른 전략과 중복되지 않게 설정, 저장 기능 사용 후에는 변경 X")]
        [SerializeField] private string formationId;
        [Header("UI Anchored Positions (px, PreviewSlots 기준)")]
        public Vector2[] uiAnchoredPositions;
        public string FormationId => formationId;
    }
}