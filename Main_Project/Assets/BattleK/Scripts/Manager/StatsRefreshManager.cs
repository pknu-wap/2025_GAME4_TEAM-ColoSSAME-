using System.Collections;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class StatsRefreshManager : MonoBehaviour
    {
        [Header("참조(씬에서 Drag&Drop)")]
        [SerializeField] private UnitLoadManager _unitLoadManager;
        [SerializeField] private FamilyStatsCollector _familyStatsCollector;
        [SerializeField] private CalculateManager _calculateManager;

        [Header("동작 옵션")]

        [Tooltip("수집 직전 1프레임 대기(예: CharacterImageLoader가 직전에 key를 세팅했다면 반영 시간 확보)")]
        [SerializeField] private bool _waitOneFrameBeforeCollect = true;

        [Tooltip("수집 후 1프레임 대기(수집 결과가 다른 스크립트에 반영될 시간 확보)")]
        [SerializeField] private bool _waitOneFrameBeforeCalculate = true;

        [Tooltip("CalculateManager가 비어 있으면 자동 재시도 흐름을 돌리도록 RefreshNow() 사용")]
        [SerializeField] private bool _useCalculateManagerRefreshFlow = true;

        public void OnClickRefresh()
        {
            StartCoroutine(DoRefreshFlow());
        }

        private IEnumerator DoRefreshFlow()
        {
            _unitLoadManager.TryLoad(out _);

            if (_waitOneFrameBeforeCollect) yield return null;

            _familyStatsCollector.CollectFromBothTeams();

            if (_waitOneFrameBeforeCalculate) yield return null;

            if (_useCalculateManagerRefreshFlow)
            {
                _calculateManager.RefreshNow();
            }
            else
            {
                _calculateManager.RefreshFromCollectorOnce();
            }
        }
    }
}
