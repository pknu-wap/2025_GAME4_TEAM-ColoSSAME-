using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BattleStartUsingSlots : MonoBehaviour
{
    public enum UnitSourceMode { FromScene, FromPrefabsList, FromResources, FromAddressables }

    [Header("버튼")]
    public Button startBattleButton;

    [Header("배틀 유닛 루트")]
    [Tooltip("플레이어 유닛이 배치될 월드 트랜스폼 루트")]
    public Transform playerUnitsRoot;
    [Tooltip("적 유닛이 배치될 월드 트랜스폼 루트 (선택)")]
    public Transform enemyUnitsRoot;

    [Header("Slot들 (하위호환)")]
    [Tooltip("이전 버전 호환용: Player 전용 슬롯. 새 버전에선 아래의 playerSlots 사용 권장")]
    public Slot[] slots;

    [Header("양 팀 슬롯 (신규)")]
    [Tooltip("플레이어 측 UI 슬롯들")]
    public Slot[] playerSlots;
    [Tooltip("적 측 UI 슬롯들 (옵션: 비워두면 '가문 전략' 자동 배치 사용)")]
    public Slot[] enemySlots;

    [Header("UI px → 로컬 좌표 스케일")]
    public float playerUiToWorldScale = 0.01f;
    public float enemyUiToWorldScale = 0.01f;

    [Header("포메이션 중심 오프셋(기본 위치)")]
    public Vector3 playerLocalOffset = Vector3.zero;
    public Vector3 enemyLocalOffset  = Vector3.zero;

    [Header("이동 연출 (개별 유닛 이동 시간, 0이면 즉시)")]
    public float moveDuration = 0.25f;

    [Header("배틀 시작 시 비활성화할 UI 루트(버튼 부모 등)")]
    public GameObject uiRootToDisable;

    [Header("유닛 소스 모드 (양 팀 동일 모드 사용)")]
    public UnitSourceMode unitSource = UnitSourceMode.FromAddressables;

    [Header("FromPrefabsList 모드: 프리팹 드래그")]
    public List<GameObject> characterPrefabs = new();

    [Header("FromResources 모드: Resources/Prefabs/Characters 등")]
    public string resourcesFolder = "Prefabs/Characters";

    [Header("Addressables 모드: 매핑 SO 리스트")]
    public List<CharacterAddressBook> addressBooks = new();
    public int playerBookIndex = 0;
    public int enemyBookIndex = 1;

    [Header("AI 연동")]
    public AI_Manager aiManager;

    [Header("전투 시작 가드")]
    public int minUnitsToStart = 1;
    public bool autoToggleStartButton = true;

    // ====== 포메이션 중심 애니메이션 ======
    [Header("포메이션 중심 애니메이션 - 플레이어")]
    public bool   playerUseFormationCenterAnim = false;
    public Vector3 playerFormationStartCenter  = new Vector3(10f, 0f, 0f);
    public Vector3 playerFormationEndCenter    = new Vector3(-2.5f, 0f, 0f);
    public float   playerFormationTravelTime   = 3f;

    [Header("포메이션 중심 애니메이션 - 적")]
    public bool   enemyUseFormationCenterAnim = false;
    public Vector3 enemyFormationStartCenter  = new Vector3(-10f, 0f, 0f);
    public Vector3 enemyFormationEndCenter    = new Vector3(2.5f, 0f, 0f);
    public float   enemyFormationTravelTime   = 3f;

    // ====== 가문 전략 기반 자동 배치 ======
    [Header("적 자동 배치: 가문 전략")]
    public bool enemyUseFactionStrategyWhenNoSlots = true;
    public EnemyFactionConfig enemyFaction;

    // ===== 내부 캐시 =====
    private readonly Dictionary<string, GameObject> _sceneUnitByKey = new();
    private readonly Dictionary<string, GameObject> _prefabByKey = new();
    private readonly Dictionary<string, GameObject> _playerInstanceByKey = new();
    private readonly Dictionary<string, GameObject> _enemyInstanceByKey  = new();

    private int _pendingSpawnOpsPlayer = 0;
    private int _pendingSpawnOpsEnemy  = 0;

    private readonly List<Vector3> _lastPlayerTargets_Local_PlayerRoot = new();
    private readonly List<Vector3> _lastPlayerTargets_Local_EnemyRoot  = new();

    private void Awake()
    {
        if (startBattleButton != null)
            startBattleButton.onClick.AddListener(OnClickStartBattle);

        BuildSources();

        if ((playerSlots == null || playerSlots.Length == 0) && (slots != null && slots.Length > 0))
            playerSlots = slots;
    }

    private void Update()
    {
        if (autoToggleStartButton && startBattleButton != null)
        {
            int occupied = GetOccupiedCount(playerSlots);
            startBattleButton.interactable = occupied >= Mathf.Max(1, minUnitsToStart);
        }
    }

    [ContextMenu("Rebuild Sources")]
    private void RebuildSourcesMenu() => BuildSources();

    private void BuildSources()
    {
        _sceneUnitByKey.Clear();
        _prefabByKey.Clear();

        switch (unitSource)
        {
            case UnitSourceMode.FromScene:
                BuildUnitMapFromScene();
                break;
            case UnitSourceMode.FromPrefabsList:
                BuildPrefabMapFromList();
                break;
            case UnitSourceMode.FromResources:
                BuildPrefabMapFromResources();
                break;
            case UnitSourceMode.FromAddressables:
                break;
        }
    }

    private void BuildUnitMapFromScene()
    {
        if (playerUnitsRoot == null)
        {
            Debug.LogError("[BattleStart] playerUnitsRoot가 비었습니다.");
            return;
        }

        var ids = playerUnitsRoot.GetComponentsInChildren<CharacterID>(includeInactive: true);
        foreach (var id in ids)
        {
            if (id == null) continue;
            var key = (id.characterKey ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key)) continue;

            if (!_sceneUnitByKey.ContainsKey(key))
                _sceneUnitByKey.Add(key, id.gameObject);
            else
                Debug.LogWarning($"[BattleStart] 중복 키(씬): {key} (첫 번째만 사용)");
        }
    }

    private void BuildPrefabMapFromList()
    {
        foreach (var prefab in characterPrefabs)
        {
            if (prefab == null) continue;
            var id = prefab.GetComponent<CharacterID>();
            if (id == null) { Debug.LogWarning($"[BattleStart] 프리팹에 CharacterID 없음: {prefab.name}"); continue; }

            var key = (id.characterKey ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key)) { Debug.LogWarning($"[BattleStart] 프리팹 키 비어있음: {prefab.name}"); continue; }

            if (!_prefabByKey.ContainsKey(key))
                _prefabByKey.Add(key, prefab);
            else
                Debug.LogWarning($"[BattleStart] 중복 키(프리팹): {key} (첫 번째만 사용)");
        }
    }

    private void BuildPrefabMapFromResources()
    {
        var loaded = Resources.LoadAll<GameObject>(resourcesFolder);
        foreach (var prefab in loaded)
        {
            if (prefab == null) continue;
            var id = prefab.GetComponent<CharacterID>();
            if (id == null) continue;

            var key = (id.characterKey ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key)) continue;

            if (!_prefabByKey.ContainsKey(key))
                _prefabByKey.Add(key, prefab);
        }
        if (_prefabByKey.Count == 0)
            Debug.LogWarning($"[BattleStart] Resources에서 프리팹을 찾지 못했습니다: {resourcesFolder}");
    }

    private CharacterAddressBook GetBookOrNull(int index)
    {
        if (index < 0 || index >= addressBooks.Count) return null;
        return addressBooks[index];
    }

    private CharacterAddressBook ResolveEnemyBook()
    {
        if (enemyFaction != null && enemyFaction.addressBookOverride != null)
            return enemyFaction.addressBookOverride;
        return GetBookOrNull(enemyBookIndex);
    }

    private void OnClickStartBattle()
    {
        int occupiedPlayer = GetOccupiedCount(playerSlots);
        if (occupiedPlayer < Mathf.Max(1, minUnitsToStart))
        {
            Debug.LogWarning($"[BattleStart] 전투 시작 불가: 플레이어 배치 {occupiedPlayer}/{minUnitsToStart}");
            return;
        }

        if (unitSource == UnitSourceMode.FromScene)
        {
            if (playerUnitsRoot == null) { Debug.LogError("[BattleStart] playerUnitsRoot가 비었습니다."); return; }
            foreach (Transform child in playerUnitsRoot) child.gameObject.SetActive(false);
            if (enemyUnitsRoot != null) foreach (Transform child in enemyUnitsRoot) child.gameObject.SetActive(false);
        }
        else
        {
            foreach (var kv in _playerInstanceByKey) if (kv.Value != null) kv.Value.SetActive(false);
            foreach (var kv in _enemyInstanceByKey)  if (kv.Value != null) kv.Value.SetActive(false);
        }

        _lastPlayerTargets_Local_PlayerRoot.Clear();
        _lastPlayerTargets_Local_EnemyRoot.Clear();

        // 플레이어 배치
        ProcessTeam(
            isPlayer: true,
            teamSlots: playerSlots,
            unitsRoot: playerUnitsRoot,
            uiToWorldScale: playerUiToWorldScale,
            baseOffset: playerLocalOffset,
            faceLeft: true,
            bookIndex: playerBookIndex,
            useFormationCenterAnim: playerUseFormationCenterAnim,
            formationStartCenter: playerFormationStartCenter,
            formationEndCenter: playerFormationEndCenter,
            formationTravelTime: playerFormationTravelTime
        );

        // 플레이어 타겟을 적 좌표계로 변환
        if (enemyUnitsRoot != null)
        {
            foreach (var pLocal in _lastPlayerTargets_Local_PlayerRoot)
            {
                Vector3 world = playerUnitsRoot != null ? playerUnitsRoot.TransformPoint(pLocal) : pLocal;
                Vector3 enemyLocal = enemyUnitsRoot.InverseTransformPoint(world);
                _lastPlayerTargets_Local_EnemyRoot.Add(enemyLocal);
            }
        }

        // 적 배치: 슬롯이 있으면 슬롯 기반, 없으면 가문 전략 기반
        bool enemyHasSlots = (enemySlots != null && enemySlots.Length > 0);
        if (enemyHasSlots && enemyUnitsRoot != null)
        {
            ProcessTeam(
                isPlayer: false,
                teamSlots: enemySlots,
                unitsRoot: enemyUnitsRoot,
                uiToWorldScale: enemyUiToWorldScale,
                baseOffset: enemyLocalOffset,
                faceLeft: false,
                bookIndex: enemyBookIndex,
                useFormationCenterAnim: enemyUseFormationCenterAnim,
                formationStartCenter: enemyFormationStartCenter,
                formationEndCenter: enemyFormationEndCenter,
                formationTravelTime: enemyFormationTravelTime
            );
        }
        else if (!enemyHasSlots && enemyUseFactionStrategyWhenNoSlots && enemyUnitsRoot != null && enemyFaction != null)
        {
            SpawnEnemyByFactionStrategy();
        }
        else
        {
            Debug.Log("[BattleStart] 적 슬롯이 없고, 자동 배치가 꺼져 있거나 설정이 부족합니다. 적 스폰 생략.");
        }

        if (uiRootToDisable != null)
            uiRootToDisable.SetActive(false);

        if (unitSource != UnitSourceMode.FromAddressables)
        {
            NotifySetupCompleteWhenReady();
        }
        else
        {
            TryNotifyAfterAddressables();
        }
    }

    private void ProcessTeam(
        bool isPlayer,
        Slot[] teamSlots,
        Transform unitsRoot,
        float uiToWorldScale,
        Vector3 baseOffset,
        bool faceLeft,
        int bookIndex,
        bool useFormationCenterAnim,
        Vector3 formationStartCenter,
        Vector3 formationEndCenter,
        float formationTravelTime)
    {
        if (teamSlots == null || teamSlots.Length == 0 || unitsRoot == null) return;

        var targetLocalPositions = new List<Vector3>();
        var slotInfos = new List<(int index, Slot slot, string key)>();

        for (int i = 0; i < teamSlots.Length; i++)
        {
            var slot = teamSlots[i];
            if (slot == null || !slot.IsOccupied || slot.Occupant == null) continue;

            var cid = slot.Occupant.GetComponent<CharacterID>();
            string key = (cid != null ? cid.characterKey : null);
            key = (key ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[BattleStart] {(isPlayer ? "플레이어" : "적")} 슬롯 {i + 1}: CharacterID.characterKey 비어있음");
                continue;
            }

            var slotRT = slot.GetComponent<RectTransform>();
            Vector2 uiPos = slotRT ? slotRT.anchoredPosition : Vector2.zero;
            Vector3 targetLocal = new Vector3(uiPos.x * uiToWorldScale, uiPos.y * uiToWorldScale, 0f) + baseOffset;

            targetLocalPositions.Add(targetLocal);
            slotInfos.Add((i, slot, key));
        }

        if (slotInfos.Count == 0) return;

        if (isPlayer)
        {
            _lastPlayerTargets_Local_PlayerRoot.Clear();
            _lastPlayerTargets_Local_PlayerRoot.AddRange(targetLocalPositions);
        }

        Vector3 averageTargetCenter = Vector3.zero;
        for (int i = 0; i < targetLocalPositions.Count; i++) averageTargetCenter += targetLocalPositions[i];
        averageTargetCenter /= targetLocalPositions.Count;

        var relativeOffsets = new List<Vector3>(targetLocalPositions.Count);
        for (int i = 0; i < targetLocalPositions.Count; i++)
            relativeOffsets.Add(targetLocalPositions[i] - averageTargetCenter);

        Vector3 centerStart, centerEnd;
        float travelTime;
        if (useFormationCenterAnim)
        {
            centerStart = formationStartCenter;
            centerEnd   = formationEndCenter;
            travelTime  = Mathf.Max(0f, formationTravelTime);
        }
        else
        {
            centerStart = averageTargetCenter;
            centerEnd   = averageTargetCenter;
            travelTime  = 0f;
        }

        for (int idx = 0; idx < slotInfos.Count; idx++)
        {
            string key = slotInfos[idx].key;
            Vector3 offset = relativeOffsets[idx];

            switch (unitSource)
            {
                case UnitSourceMode.FromScene:
                {
                    if (isPlayer)
                    {
                        if (!_sceneUnitByKey.TryGetValue(key, out var unitGO) || unitGO == null)
                        {
                            Debug.LogWarning($"[BattleStart] (씬/{(isPlayer ? "플레이어" : "적")}) '{key}' 유닛을 Player 하위에서 찾을 수 없음");
                            continue;
                        }
                        PlaceUnitByFormation(unitGO, centerStart, centerEnd, offset, faceLeft, travelTime);
                    }
                    else
                    {
                        Debug.LogWarning("[BattleStart] 씬 모드에서 적 측 로드는 미구현입니다. Addressables/Prefab/Resources 권장.");
                    }
                    break;
                }
                case UnitSourceMode.FromPrefabsList:
                case UnitSourceMode.FromResources:
                {
                    if (!_prefabByKey.TryGetValue(key, out var prefab) || prefab == null)
                    {
                        Debug.LogWarning($"[BattleStart] {(isPlayer ? "플레이어" : "적")} 프리팹 키 '{key}'를 찾지 못함");
                        continue;
                    }

                    var instanceMap = isPlayer ? _playerInstanceByKey : _enemyInstanceByKey;
                    if (!instanceMap.TryGetValue(key, out var unitGO) || unitGO == null)
                    {
                        unitGO = Instantiate(prefab, unitsRoot ? unitsRoot : transform);
                        unitGO.name = $"{(isPlayer ? "P" : "E")}_Unit_{key}";
                        instanceMap[key] = unitGO;
                    }
                    PlaceUnitByFormation(unitGO, centerStart, centerEnd, offset, faceLeft, travelTime);
                    break;
                }
                case UnitSourceMode.FromAddressables:
                {
                    var book = isPlayer ? GetBookOrNull(playerBookIndex) : ResolveEnemyBook();
                    if (book == null || unitsRoot == null)
                    {
                        Debug.LogError($"[BattleStart] Addressables 설정 누락: book={(book==null?"null":"ok")}, unitsRoot={(unitsRoot==null?"null":"ok")}");
                        continue;
                    }
                    if (!book.TryGet(key, out var ar) || ar == null)
                    {
                        Debug.LogWarning($"[BattleStart] Addressable 키 매핑 없음({(isPlayer ? "플레이어" : "적")}): {key}");
                        continue;
                    }

                    if (isPlayer) _pendingSpawnOpsPlayer++;
                    else         _pendingSpawnOpsEnemy++;

                    var instanceMap = isPlayer ? _playerInstanceByKey : _enemyInstanceByKey;
                    CoroutineRunner.Run(EnsureAndPlaceAddressableByFormation(
                        key: key,
                        ar: ar,
                        unitsRoot: unitsRoot,
                        centerStart: centerStart,
                        centerEnd: centerEnd,
                        offset: offset,
                        faceLeft: faceLeft,
                        travelTime: travelTime,
                        instanceMap: instanceMap,
                        isPlayer: isPlayer
                    ));
                    break;
                }
            }
        }
    }

    private void SpawnEnemyByFactionStrategy()
    {
        if (enemyFaction == null || enemyUnitsRoot == null)
        {
            Debug.LogWarning("[BattleStart] 가문 전략 스폰 실패: 설정 누락");
            return;
        }

        var set = enemyFaction.strategySet;
        if (set == null || set.strategies == null || set.strategies.Count == 0)
        {
            Debug.LogWarning("[BattleStart] 가문 전략 스폰 실패: 전략 세트 비어있음");
            return;
        }

        var keys = enemyFaction.PickRosterKeys();
        if (keys == null || keys.Count == 0)
        {
            Debug.LogWarning("[BattleStart] 가문 전략 스폰 실패: 선발된 출전 키가 없음");
            return;
        }

        // 전략 입력
        var req = new EnemyStrategyRequest
        {
            unitCount = keys.Count,
            playerLocalTargetsInEnemySpace = _lastPlayerTargets_Local_EnemyRoot.ToArray(),
            uiToWorldScale = enemyUiToWorldScale,
            baseOffset = enemyLocalOffset,
            formationStartCenter = enemyUseFormationCenterAnim ? enemyFormationStartCenter : enemyFormationEndCenter,
            formationEndCenter   = enemyFormationEndCenter
        };

        // 전략 시도 + 세트 내 폴백 + 최종 라인 폴백
        var targets = TryBuildWithFallbacks(set, req, tryCount: Mathf.Max(1, set.strategies.Count));
        if (targets == null || targets.Length == 0)
        {
            // 최종 안전망: 가운데 정렬 라인 생성
            targets = BuildCenteredLine(keys.Count, enemyUiToWorldScale, enemyLocalOffset, 120f);
            Debug.LogWarning("[BattleStart] 모든 전략이 좌표를 생성하지 않아 기본 라인으로 폴백합니다.");
        }

        Vector3 centerStart = enemyUseFormationCenterAnim ? enemyFormationStartCenter : enemyFormationEndCenter;
        Vector3 centerEnd   = enemyFormationEndCenter;
        float   travelTime  = enemyUseFormationCenterAnim ? Mathf.Max(0f, enemyFormationTravelTime) : 0f;

        int count = Mathf.Min(keys.Count, targets.Length);
        var instanceMap = _enemyInstanceByKey;

        for (int i = 0; i < count; i++)
        {
            string key = (keys[i] ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[BattleStart] 가문 전략 스폰: 빈 키 (index {i})");
                continue;
            }

            Vector3 relativeOffset = targets[i] - centerEnd;

            switch (unitSource)
            {
                case UnitSourceMode.FromPrefabsList:
                case UnitSourceMode.FromResources:
                {
                    if (!_prefabByKey.TryGetValue(key, out var prefab) || prefab == null)
                    {
                        Debug.LogWarning($"[BattleStart] 적 프리팹 키 '{key}'를 찾지 못함");
                        continue;
                    }

                    if (!instanceMap.TryGetValue(key, out var unitGO) || unitGO == null)
                    {
                        unitGO = Instantiate(prefab, enemyUnitsRoot ? enemyUnitsRoot : transform);
                        unitGO.name = $"E_Unit_{key}";
                        instanceMap[key] = unitGO;
                    }
                    PlaceUnitByFormation(unitGO, centerStart, centerEnd, relativeOffset, faceLeft:false, travelTime:travelTime);
                    break;
                }
                case UnitSourceMode.FromAddressables:
                {
                    var book = ResolveEnemyBook();
                    if (book == null || enemyUnitsRoot == null)
                    {
                        Debug.LogError($"[BattleStart] Addressables 설정 누락(가문 전략): book={(book==null?"null":"ok")}, enemyUnitsRoot={(enemyUnitsRoot==null?"null":"ok")}");
                        continue;
                    }
                    if (!book.TryGet(key, out var ar) || ar == null)
                    {
                        Debug.LogWarning($"[BattleStart] Addressable 키 매핑 없음(가문 전략): {key}");
                        continue;
                    }

                    _pendingSpawnOpsEnemy++;

                    CoroutineRunner.Run(EnsureAndPlaceAddressableByFormation(
                        key: key,
                        ar: ar,
                        unitsRoot: enemyUnitsRoot,
                        centerStart: centerStart,
                        centerEnd: centerEnd,
                        offset: relativeOffset,
                        faceLeft: false,
                        travelTime: travelTime,
                        instanceMap: instanceMap,
                        isPlayer: false
                    ));
                    break;
                }
                case UnitSourceMode.FromScene:
                default:
                    Debug.LogWarning("[BattleStart] 씬 모드/미지원 모드에서 가문 전략 스폰은 비권장");
                    break;
            }
        }
    }

    private Vector3[] TryBuildWithFallbacks(EnemyStrategySet set, EnemyStrategyRequest req, int tryCount)
    {
        // 1) 우선 가중 랜덤으로 하나
        var first = set.PickRandom();
        var tried = new HashSet<EnemyStrategyBase>();
        Vector3[] attempt = BuildSafe(first, req);
        if (attempt != null && attempt.Length > 0) return attempt;
        tried.Add(first);

        // 2) 남은 전략들 순차 시도 (등록 순서)
        foreach (var w in set.strategies)
        {
            if (w.strategy == null || tried.Contains(w.strategy)) continue;
            attempt = BuildSafe(w.strategy, req);
            if (attempt != null && attempt.Length > 0) return attempt;
            tried.Add(w.strategy);
            if (tried.Count >= tryCount) break;
        }

        // 3) 실패
        return new Vector3[0];
    }

    private Vector3[] BuildSafe(EnemyStrategyBase strategy, EnemyStrategyRequest req)
    {
        if (strategy == null) return new Vector3[0];
        Vector3[] arr = null;
        try { arr = strategy.BuildLocalPositions(req); }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[BattleStart] 전략 실행 예외: {strategy.name} - {e.Message}");
            return new Vector3[0];
        }
        if (arr == null || arr.Length == 0) return new Vector3[0];
        return arr;
    }

    private Vector3[] BuildCenteredLine(int count, float scale, Vector3 baseOffset, float cellPxX)
    {
        var arr = new Vector3[count];
        float cell = Mathf.Max(1f, cellPxX) * scale;
        float startX = -(count - 1) * 0.5f * cell; // 가운데 정렬
        for (int i = 0; i < count; i++)
        {
            float x = startX + i * cell;
            arr[i] = new Vector3(x, 0f, 0f) + baseOffset;
        }
        return arr;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private int GetOccupiedCount(Slot[] teamSlots)
    {
        if (teamSlots == null) return 0;
        int cnt = 0;
        for (int i = 0; i < teamSlots.Length; i++)
        {
            var s = teamSlots[i];
            if (s != null && s.IsOccupied && s.Occupant != null) cnt++;
        }
        return cnt;
    }

    private void PlaceUnitByFormation(GameObject unitGO, Vector3 centerStart, Vector3 centerEnd, Vector3 relativeOffset, bool faceLeft, float travelTime)
    {
        unitGO.SetActive(true);

        var s = unitGO.transform.localScale;
        s.x = (faceLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x));
        unitGO.transform.localScale = s;

        unitGO.transform.localPosition = centerStart + relativeOffset;

        if (travelTime > 0f)
            CoroutineRunner.Run(MoveLocal(unitGO.transform, centerEnd + relativeOffset, travelTime));
        else if (moveDuration > 0f)
            CoroutineRunner.Run(MoveLocal(unitGO.transform, centerEnd + relativeOffset, moveDuration));
        else
            unitGO.transform.localPosition = centerEnd + relativeOffset;
    }

    private IEnumerator EnsureAndPlaceAddressableByFormation(
        string key,
        AssetReferenceGameObject ar,
        Transform unitsRoot,
        Vector3 centerStart,
        Vector3 centerEnd,
        Vector3 offset,
        bool faceLeft,
        float travelTime,
        Dictionary<string, GameObject> instanceMap,
        bool isPlayer)
    {
        if (!instanceMap.TryGetValue(key, out var go) || go == null)
        {
            var handle = ar.InstantiateAsync(unitsRoot);
            yield return handle;
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[BattleStart] Addressables Instantiate 실패: {key} ({(isPlayer ? "플레이어" : "적")})");
                if (isPlayer) _pendingSpawnOpsPlayer = Mathf.Max(0, _pendingSpawnOpsPlayer - 1);
                else          _pendingSpawnOpsEnemy  = Mathf.Max(0, _pendingSpawnOpsEnemy  - 1);
                TryNotifyAfterAddressables();
                yield break;
            }
            go = handle.Result;
            go.name = $"{(isPlayer ? "P" : "E")}_Unit_{key}";
            instanceMap[key] = go;
        }

        go.SetActive(true);
        var s = go.transform.localScale; s.x = (faceLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x)); go.transform.localScale = s;

        go.transform.localPosition = centerStart + offset;

        if (travelTime > 0f)
            yield return MoveLocal(go.transform, centerEnd + offset, travelTime);
        else if (moveDuration > 0f)
            yield return MoveLocal(go.transform, centerEnd + offset, moveDuration);
        else
            go.transform.localPosition = centerEnd + offset;

        if (isPlayer) _pendingSpawnOpsPlayer = Mathf.Max(0, _pendingSpawnOpsPlayer - 1);
        else          _pendingSpawnOpsEnemy  = Mathf.Max(0, _pendingSpawnOpsEnemy  - 1);

        TryNotifyAfterAddressables();
    }

    private void TryNotifyAfterAddressables()
    {
        if (unitSource != UnitSourceMode.FromAddressables) return;
        if (_pendingSpawnOpsPlayer == 0 && _pendingSpawnOpsEnemy == 0)
            NotifySetupCompleteWhenReady();
    }

    private IEnumerator MoveLocal(Transform t, Vector3 toLocalPos, float duration)
    {
        Vector3 from = t.localPosition;
        float acc = 0f;
        while (acc < duration)
        {
            acc += Time.deltaTime;
            float k = Mathf.Clamp01(acc / duration);
            t.localPosition = Vector3.Lerp(from, toLocalPos, k);
            yield return null;
        }
        t.localPosition = toLocalPos;
    }

    private void NotifySetupCompleteWhenReady()
    {
        if (aiManager == null) return;

        aiManager.unitPool.Clear();
        if (playerUnitsRoot != null) aiManager.unitPool.Add(playerUnitsRoot);
        if (enemyUnitsRoot  != null) aiManager.unitPool.Add(enemyUnitsRoot);

        aiManager.SetUnitList();
    }
}