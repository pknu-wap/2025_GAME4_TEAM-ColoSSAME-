using System.Collections.Generic;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    [Tooltip("index 0 = Player 루트, index 1 = Enemy 루트(선택)")]
    public List<Transform> unitPool = new List<Transform>();

    [Header("수집된 유닛 리스트(읽기전용 성격)")]
    public List<AICore> playerUnits = new List<AICore>();
    public List<AICore> EnemyUnits  = new List<AICore>();

    [Header("레이어 설정")]
    [Tooltip("Project Settings > Tags and Layers 에서 만든 레이어 이름")]
    public string playerLayerName = "Player";
    public string enemyLayerName  = "Enemy";

    [Tooltip("유닛 오브젝트의 모든 하위 자식까지 레이어를 일괄 적용할지")]
    public bool applyLayerRecursively = true;

    [Header("팀/레이어 연동 옵션")]
    [Tooltip("사이드(0=Player,1=Enemy)에 맞춰 레이어를 강제로 통일할지 여부")]
    public bool forceLayerBySide = true;

    [Tooltip("AICore.targetLayer를 GameObject.layer를 기준으로 자동 산출할지(권장)")]
    public bool setTargetByLayer = true;

    // 내부 캐시(이름→인덱스)
    private int _playerLayer = -1;
    private int _enemyLayer  = -1;

    void Start()
    {
        // 씬에 미리 배치된 경우에만 의미 있음.
        // BattleStartUsingSlots가 스폰/배치 후 SetUnitList를 다시 호출함.
        if (unitPool.Count > 0) SetUnitList();
    }

    public void SetUnitList()
    {
        playerUnits.Clear();
        EnemyUnits.Clear();

        ResolveLayers(); // 이름 → 인덱스 (없으면 Default(0)로 폴백)

        for (int side = 0; side < unitPool.Count; side++)
        {
            var root = unitPool[side];
            if (root == null) continue;

            // 하위 어디에 붙어 있어도 수집
            var cores = root.GetComponentsInChildren<AICore>(includeInactive: false);
            foreach (var core in cores)
            {
                if (core == null || !core.gameObject.activeInHierarchy) continue;

                var go = core.gameObject;

                // 1) 사이드에 맞춰 레이어 정렬(옵션)
                if (forceLayerBySide)
                {
                    int wantLayer = (side == 0) ? _playerLayer : _enemyLayer;
                    AssignLayer(go, wantLayer);
                }

                // 2) targetLayer(마스크) 자동 산출
                int targetMask = ComputeTargetMaskFrom(go.layer, side);
                core.targetLayer = targetMask;

                // 3) 리스트 분류
                if (side == 0) // Player
                {
                    if (!playerUnits.Contains(core)) playerUnits.Add(core);
                }
                else if (side == 1) // Enemy
                {
                    if (!EnemyUnits.Contains(core)) EnemyUnits.Add(core);
                }
            }
        }

        // Debug.Log($"[AI_Manager] 등록 완료: Player={playerUnits.Count}, Enemy={EnemyUnits.Count} / Layers P={_playerLayer},E={_enemyLayer}");
    }

    /// <summary>
    /// 이름 기반으로 레이어 인덱스를 구해 캐시합니다. 없으면 Default(0)로 폴백합니다.
    /// </summary>
    private void ResolveLayers()
    {
        _playerLayer = LayerMask.NameToLayer(playerLayerName);
        _enemyLayer  = LayerMask.NameToLayer(enemyLayerName);

        if (_playerLayer < 0)
        {
            Debug.LogWarning($"[AI_Manager] '{playerLayerName}' 레이어를 찾을 수 없습니다. Default(0) 사용.");
            _playerLayer = 0; // Default
        }
        if (_enemyLayer < 0)
        {
            Debug.LogWarning($"[AI_Manager] '{enemyLayerName}' 레이어를 찾을 수 없습니다. Default(0) 사용.");
            _enemyLayer = 0; // Default
        }
    }

    /// <summary>
    /// targetLayer 마스크를 계산합니다.
    /// - setTargetByLayer=true 이면 GameObject.layer를 신뢰하고 반대편 레이어를 타겟으로 지정
    /// - false면 사이드(0=Player,1=Enemy) 기준으로 지정
    /// - 레이어가 둘 다 아닐 경우 사이드 기준으로 폴백
    /// </summary>
    private int ComputeTargetMaskFrom(int selfLayerIndex, int sideIndex)
    {
        int targetLayerIndex;

        if (setTargetByLayer)
        {
            if (selfLayerIndex == _playerLayer)      targetLayerIndex = _enemyLayer;
            else if (selfLayerIndex == _enemyLayer)  targetLayerIndex = _playerLayer;
            else                                     targetLayerIndex = (sideIndex == 0) ? _enemyLayer : _playerLayer; // 폴백
        }
        else
        {
            targetLayerIndex = (sideIndex == 0) ? _enemyLayer : _playerLayer;
        }

        return 1 << targetLayerIndex; // 마스크 반환
    }

    /// <summary>
    /// 레이어를 적용합니다(필요 시 전체 트리 재귀 적용).
    /// </summary>
    private void AssignLayer(GameObject go, int layerIndex)
    {
        if (go == null) return;
        if (applyLayerRecursively)
            SetLayerRecursively(go.transform, layerIndex);
        else
            go.layer = layerIndex;
    }

    private void SetLayerRecursively(Transform t, int layerIndex)
    {
        t.gameObject.layer = layerIndex;
        foreach (Transform c in t)
            SetLayerRecursively(c, layerIndex);
    }
}