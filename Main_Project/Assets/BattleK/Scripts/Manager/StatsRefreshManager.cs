using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class StatsRefreshManager : MonoBehaviour
    {
        [Header("참조(씬에서 Drag&Drop)")]
        [SerializeField] private UnitLoadManager unitLoadManager;
        [SerializeField] private FamilyStatsCollector familyStatsCollector;
        [SerializeField] private CalculateManager calculateManager;

        [Header("동작 옵션")]
        [Tooltip("버튼 클릭 시 UserSave.json을 디스크에서 다시 읽을지 여부")]
        [SerializeField] private bool reloadUserSaveOnClick = true;

        [Tooltip("수집 직전 1프레임 대기(예: CharacterImageLoader가 직전에 key를 세팅했다면 반영 시간 확보)")]
        [SerializeField] private bool waitOneFrameBeforeCollect = true;

        [Tooltip("수집 후 1프레임 대기(수집 결과가 다른 스크립트에 반영될 시간 확보)")]
        [SerializeField] private bool waitOneFrameBeforeCalculate = true;

        [Tooltip("CalculateManager가 비어 있으면 자동 재시도 흐름을 돌리도록 RefreshNow() 사용")]
        [SerializeField] private bool useCalculateManagerRefreshFlow = true;

        [Header("디버그 로그")]
        [SerializeField] private bool debugLogging = true;

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
}
