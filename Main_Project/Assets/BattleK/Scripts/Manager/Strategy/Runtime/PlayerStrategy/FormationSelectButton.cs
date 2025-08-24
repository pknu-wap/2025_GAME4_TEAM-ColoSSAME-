using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FormationSelectButton : MonoBehaviour
{
    [Header("Hook")]
    public Button button;                      // 비워두면 자동 GetComponent
    public FormationManager formationManager;  // UI 미리보기
    public FormationAsset formation;           // SO 좌표

    [Header("Enemy에도 적용(선택)")]
    public bool alsoApplyToEnemy = false;
    public BattleStartUsingSlots battle;       // 적 변환에 필요

    // 내부: 재사용용
    private FixedLocalPositionsStrategy _runtimeFixedEnemy;
    private EnemyStrategySet _runtimeSet;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(Apply);
    }

    public void Apply()
    {
        if (formationManager != null && formation != null)
            formationManager.ApplyFormationAsset(formation);

        if (!alsoApplyToEnemy) return;

        if (battle == null || formation == null || formation.uiAnchoredPositions == null || formation.uiAnchoredPositions.Length == 0)
        {
            Debug.LogWarning("[FormationSelectButton] Enemy 적용 실패: battle/formation 누락");
            return;
        }
        if (battle.enemyFaction == null)
        {
            Debug.LogWarning("[FormationSelectButton] EnemyFactionConfig 누락");
            return;
        }

        // UI px -> Enemy 로컬 좌표(Vector3)
        var src = formation.uiAnchoredPositions;
        var local = new Vector3[src.Length];
        for (int i = 0; i < src.Length; i++)
        {
            var p = src[i];
            local[i] = new Vector3(p.x * battle.enemyUiToWorldScale, p.y * battle.enemyUiToWorldScale, 0f) + battle.enemyLocalOffset;
        }

        // 런타임 고정좌표 전략 SO 생성/재사용
        if (_runtimeFixedEnemy == null) _runtimeFixedEnemy = ScriptableObject.CreateInstance<FixedLocalPositionsStrategy>();
        _runtimeFixedEnemy.name = "ES_RuntimeEnemyFixed_FromFormation";
        _runtimeFixedEnemy.localPositions = local;
        _runtimeFixedEnemy.shuffle = false;
        _runtimeFixedEnemy.addBaseOffset = false;       // 중복 오프셋 방지
        _runtimeFixedEnemy.fallbackToLineIfEmpty = false;

        // 이 전략 하나만 들어있는 세트로 갈아끼우기
        if (_runtimeSet == null) _runtimeSet = ScriptableObject.CreateInstance<EnemyStrategySet>();
        _runtimeSet.strategies = new List<EnemyStrategySet.WeightedStrategy>
        {
            new EnemyStrategySet.WeightedStrategy{ weight = 1f, strategy = _runtimeFixedEnemy }
        };

        battle.enemyFaction.strategySet = _runtimeSet;
        battle.enemyUseFactionStrategyWhenNoSlots = true;

        Debug.Log("[FormationSelectButton] Formation을 Enemy 고정좌표 전략으로도 적용 완료");
    }
}