using System.Collections;
using BattleK.Scripts.Manager;
using UnityEngine;

/// <summary>
/// 버튼 OnClick에 연결해서 다음 순서로 갱신을 오케스트레이션:
/// 1) (선택) UnitLoadManager: UserSave.json 재로드
/// 2) FamilyStatsCollector: CollectFromBothTeams() 실행
/// 3) CalculateManager: RefreshFromCollectorOnce()로 로컬 복사본 갱신
///
/// * 기존 스크립트는 수정 불필요.
/// * 프레임 타이밍 이슈를 피하려고 단계 사이에 선택적 대기(한 프레임/지연)를 지원.
/// </summary>
public class StatsRefreshManager : MonoBehaviour
{
    [Header("참조(씬에서 Drag&Drop)")]
    public UnitLoadManager unitLoadManager;
    public FamilyStatsCollector familyStatsCollector;
    public CalculateManager calculateManager;

    [Header("동작 옵션")]
    [Tooltip("버튼 클릭 시 UserSave.json을 디스크에서 다시 읽을지 여부")]
    public bool reloadUserSaveOnClick = true;

    [Tooltip("수집 직전 1프레임 대기(예: CharacterImageLoader가 직전에 key를 세팅했다면 반영 시간 확보)")]
    public bool waitOneFrameBeforeCollect = true;

    [Tooltip("수집 후 1프레임 대기(수집 결과가 다른 스크립트에 반영될 시간 확보)")]
    public bool waitOneFrameBeforeCalculate = true;

    [Tooltip("CalculateManager가 비어 있으면 자동 재시도 흐름을 돌리도록 RefreshNow() 사용")]
    public bool useCalculateManagerRefreshFlow = true;

    [Header("디버그 로그")]
    public bool debugLogging = true;

    /// <summary>버튼 OnClick에서 호출하세요.</summary>
    public void OnClickRefresh()
    {
        StartCoroutine(DoRefreshFlow());
    }

    private IEnumerator DoRefreshFlow()
    {
        if (unitLoadManager == null)
            unitLoadManager = FindObjectOfType<UnitLoadManager>();
        if (familyStatsCollector == null)
            familyStatsCollector = FindObjectOfType<FamilyStatsCollector>();
        if (calculateManager == null)
            calculateManager = FindObjectOfType<CalculateManager>();

        // 1) (선택) UserSave 재로드
        if (reloadUserSaveOnClick && unitLoadManager != null)
        {
            if (debugLogging) Debug.Log("[StatsRefreshButton] Reload UserSave...");
            unitLoadManager.TryLoad(out var msg);
            if (debugLogging) Debug.Log($"[StatsRefreshButton] UserSave: {msg}");
        }

        // (선택) 수집 전 1프레임 대기
        if (waitOneFrameBeforeCollect) yield return null;

        // 2) FamilyStatsCollector 수집
        if (familyStatsCollector != null)
        {
            if (debugLogging) Debug.Log("[StatsRefreshButton] CollectFromBothTeams()");
            familyStatsCollector.CollectFromBothTeams();
        }
        else if (debugLogging)
        {
            Debug.LogWarning("[StatsRefreshButton] FamilyStatsCollector를 찾지 못했습니다.");
        }

        // (선택) 계산 전 1프레임 대기
        if (waitOneFrameBeforeCalculate) yield return null;

        // 3) CalculateManager 갱신
        if (calculateManager != null)
        {
            if (useCalculateManagerRefreshFlow)
            {
                if (debugLogging) Debug.Log("[StatsRefreshButton] CalculateManager.RefreshNow()");
                calculateManager.RefreshNow(); // 내부에서 코루틴으로 갱신 + 자동 재시도까지 관리
            }
            else
            {
                if (debugLogging) Debug.Log("[StatsRefreshButton] CalculateManager.RefreshFromCollectorOnce()");
                calculateManager.RefreshFromCollectorOnce(); // 1회 갱신
            }
        }
        else if (debugLogging)
        {
            Debug.LogWarning("[StatsRefreshButton] CalculateManager를 찾지 못했습니다.");
        }
    }
}
