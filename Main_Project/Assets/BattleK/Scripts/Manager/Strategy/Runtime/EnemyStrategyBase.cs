using UnityEngine;

public abstract class EnemyStrategyBase : ScriptableObject
{
    [TextArea] public string description;

    /// <summary>
    /// 전략이 산출하는 "최종 목표 로컬좌표"들을 반환합니다.
    /// 반환 좌표는 Enemy Units Root 기준의 로컬 좌표여야 합니다.
    /// 필요한 경우 요청 내 playerLocalTargetsInEnemySpace를 이용해 미러링 등 구현.
    /// </summary>
    public abstract Vector3[] BuildLocalPositions(EnemyStrategyRequest req);
}

/// <summary>
/// 전략에 전달되는 입력 파라미터 모음
/// </summary>
public struct EnemyStrategyRequest
{
    public int unitCount;                          // 생성/배치할 적 유닛 수
    public Vector3[] playerLocalTargetsInEnemySpace; // 플레이어 타겟(최종) 로컬좌표를 Enemy root 기준으로 변환한 것 (미러용)
    public float uiToWorldScale;                   // 적 측 UI px -> 로컬 변환 배율 (FixedAnchors 등)
    public Vector3 baseOffset;                     // 적 측 포메이션 기본 오프셋
    public Vector3 formationStartCenter;           // 포메이션 중심 애니메이션 시작점
    public Vector3 formationEndCenter;             // 포메이션 중심 애니메이션 종점
}