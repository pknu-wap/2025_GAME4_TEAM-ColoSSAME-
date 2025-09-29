// AI_Manager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    /// <summary>전역 접근이 필요한 경우를 위해 제공(없어도 동작은 가능).</summary>
    public static AI_Manager Instance { get; private set; }

    /// <summary>매니저가 활성화되면 통지되어 늦게 켜진 AICore에서도 안전 접속 가능.</summary>
    public static event Action<AI_Manager> OnReady;

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

    // ─────────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // 싱글톤 설정(선택)
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[AI_Manager] Duplicate instance detected. Destroying this one.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // 매니저 준비됨 이벤트(초기화 이후 AICore들이 구독하고 있다가 즉시 연결)
        OnReady?.Invoke(this);
    }

    private void Start()
    {
        // 씬에 미리 배치된 경우에만 의미 있음.
        // 런타임 스폰/배치가 있다면 BattleStartUsingSlots 등에서 SetUnitList를 다시 호출.
        if (unitPool.Count > 0) SetUnitList();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 공개 API
    // ─────────────────────────────────────────────────────────────────────────────

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
    /// 외부에서 AICore가 런타임에 생성/활성화될 때 호출해 등록하고 싶다면 사용(선택).
    /// </summary>
    public void TryRegister(AICore core, bool guessSideByLayer = true)
    {
        if (core == null) return;

        if (_playerLayer < 0 || _enemyLayer < 0) ResolveLayers();

        int side = 0; // 기본 Player
        if (guessSideByLayer)
        {
            if (core.gameObject.layer == _enemyLayer) side = 1;
        }

        if (forceLayerBySide)
        {
            AssignLayer(core.gameObject, side == 0 ? _playerLayer : _enemyLayer);
        }

        int targetMask = ComputeTargetMaskFrom(core.gameObject.layer, side);
        core.targetLayer = targetMask;

        var list = (side == 0) ? playerUnits : EnemyUnits;
        if (!list.Contains(core)) list.Add(core);
    }

    /// <summary>
    /// 외부에서 AICore가 비활성/파괴될 때 해제하고 싶다면 사용(선택).
    /// </summary>
    public void Unregister(AICore core)
    {
        if (core == null) return;
        playerUnits.Remove(core);
        EnemyUnits.Remove(core);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 내부 유틸
    // ─────────────────────────────────────────────────────────────────────────────

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

    public void IsWinner()
    {
        if (playerUnits.Count < 1)
        {
            //Player Lost Enemy Won
        }

        if (EnemyUnits.Count < 1)
        {
            //Enemy Lost Player Won
        }
    }
}
