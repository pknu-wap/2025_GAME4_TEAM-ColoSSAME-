using BattleK.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BattleK.Scripts.Manager.Strategy.Runtime.PlayerStrategy
{
    public class FormationSelectButton : MonoBehaviour
    {
        [Header("UI 연결")]
        [SerializeField] private Button button;
        [SerializeField] private FormationManager formationManager;
        [SerializeField] private BattleStartUsingSlots battleStart;

        [Header("데이터")]
        [SerializeField] private FormationAsset formation;

        [SerializeField] private bool alsoApplyToEnemy;
        private FixedLocalPositionsStrategy _runtimeFixedEnemy;
        private EnemyStrategySet _runtimeSet;

        private void Awake()
        {
            button.onClick.AddListener(Apply);
        }

        private void Apply()
        {
            formationManager.ApplyFormationAsset(formation);

            switch (alsoApplyToEnemy)
            {
                case true when !battleStart:
                    battleStart.SetEnemyFormationOverride(formation);
                    Debug.Log($"[FormationButton] 적군 포메이션 오버라이드 설정: {formation.name}");
                    break;
                case false:
                    return;
            }
        }
    }
}