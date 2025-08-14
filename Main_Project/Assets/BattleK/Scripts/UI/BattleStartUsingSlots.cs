using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BattleStartUsingSlots : MonoBehaviour
{
    public enum UnitSourceMode { FromScene, FromPrefabsList, FromResources, FromAddressables }

    [Header("버튼")]
    public Button startBattleButton;

    [Header("배틀 유닛 루트 (BattleScene/BattleSet/Units/Player)")]
    public Transform playerUnitsRoot;

    [Header("Slot들 (Slot1~N 순서 중요)")]
    public Slot[] slots;

    [Header("UI px → Player 로컬 좌표 배율")]
    [Tooltip("Slots의 anchoredPosition(px)을 Player 로컬 좌표로 변환. 예: 0.01f")]
    public float uiToWorldScale = 0.01f;

    [Header("포메이션 중심 오프셋 (Player 기준)")]
    public Vector3 playerLocalOffset = Vector3.zero;

    [Header("이동 연출 (0이면 즉시)")]
    public float moveDuration = 0.25f;

    [Header("배틀 시작 시 비활성화할 UI 루트(버튼 부모 등)")]
    public GameObject uiRootToDisable;

    [Header("유닛 소스 모드")]
    public UnitSourceMode unitSource = UnitSourceMode.FromAddressables;

    [Header("FromPrefabsList 모드: 프리팹 드래그")]
    public List<GameObject> characterPrefabs = new(); // 프리팹 루트에 CharacterID 필요

    [Header("FromResources 모드: Resources/Prefabs/Characters 등")]
    public string resourcesFolder = "Prefabs/Characters";

    [Header("Addressables 모드: 매핑 SO")]
    public CharacterAddressBook addressBook;

    [Header("AI 연동")]
    public AI_Manager aiManager;
    public Transform enemyUnitsRoot;   // 있으면 할당 (없으면 null 허용)

    // ✅ 추가: 시작 가드 설정
    [Header("전투 시작 가드")]
    [Tooltip("전투 시작을 허용하기 위한 슬롯 점유 최소 수")]
    public int minUnitsToStart = 1;
    [Tooltip("슬롯 점유 수에 따라 시작 버튼 interactable 자동 갱신")]
    public bool autoToggleStartButton = true;

    // 씬 모드: key → scene object (Player 하위)
    private readonly Dictionary<string, GameObject> _sceneUnitByKey = new();

    // 프리팹/리소스 모드: key → prefab
    private readonly Dictionary<string, GameObject> _prefabByKey = new();

    // 프리팹/Addressables 모드: key → spawned instance (재사용)
    private readonly Dictionary<string, GameObject> _instanceByKey = new();

    // Addressables 비동기 스폰 대기 카운터
    private int _pendingSpawnOps = 0;

    private void Awake()
    {
        if (startBattleButton != null)
            startBattleButton.onClick.AddListener(OnClickStartBattle);

        BuildSources();
    }

    private void Update()
    {
        // ✅ 슬롯 점유 수에 따라 버튼 활성/비활성 자동 갱신
        if (autoToggleStartButton && startBattleButton != null)
            startBattleButton.interactable = GetOccupiedCount() >= Mathf.Max(1, minUnitsToStart);
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
                // Addressables는 AddressBook 참조로 사용
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

    private void OnClickStartBattle()
    {
        // ✅ 제일 먼저: 최소 배치 수 가드
        int occupied = GetOccupiedCount();
        if (occupied < Mathf.Max(1, minUnitsToStart))
        {
            Debug.LogWarning($"[BattleStart] 전투 시작 불가: 배치된 유닛 {occupied}/{minUnitsToStart}");
            // 필요하면 여기서 경고 UI/사운드 재생
            return;
        }

        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("[BattleStart] slots 설정이 필요합니다.");
            return;
        }

        // 1) 필요 없는 인스턴스/오브젝트 비활성화
        if (unitSource == UnitSourceMode.FromScene)
        {
            if (playerUnitsRoot == null) { Debug.LogError("[BattleStart] playerUnitsRoot가 비었습니다."); return; }
            foreach (Transform child in playerUnitsRoot)
                child.gameObject.SetActive(false);
        }
        else
        {
            foreach (var kv in _instanceByKey)
                if (kv.Value != null) kv.Value.SetActive(false);
        }

        // 2) 슬롯 점유자만 활성화/스폰 + 위치 이동
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null || !slot.IsOccupied || slot.Occupant == null) continue;

            var drag = slot.Occupant.GetComponent<UIDrag>();
            if (drag == null) continue;

            var key = (drag.characterKey ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key)) continue;

            // UI px → Player 로컬 좌표
            var slotRT = slot.GetComponent<RectTransform>();
            Vector2 uiPos = slotRT ? slotRT.anchoredPosition : Vector2.zero;
            Vector3 targetLocal = new Vector3(uiPos.x * uiToWorldScale, uiPos.y * uiToWorldScale, 0f) + playerLocalOffset;

            switch (unitSource)
            {
                case UnitSourceMode.FromScene:
                {
                    if (!_sceneUnitByKey.TryGetValue(key, out var unitGO) || unitGO == null) {
                        Debug.LogWarning($"[BattleStart] (씬) '{key}' 유닛을 Player 하위에서 찾을 수 없음");
                        continue;
                    }
                    PlaceUnitScene(unitGO, targetLocal);
                    break;
                }
                case UnitSourceMode.FromPrefabsList:
                case UnitSourceMode.FromResources:
                {
                    if (!_instanceByKey.TryGetValue(key, out var unitGO) || unitGO == null)
                    {
                        if (!_prefabByKey.TryGetValue(key, out var prefab) || prefab == null) {
                            Debug.LogWarning($"[BattleStart] 프리팹 키 '{key}'를 찾지 못함"); continue;
                        }
                        unitGO = Instantiate(prefab, playerUnitsRoot ? playerUnitsRoot : transform);
                        unitGO.name = $"Unit_{key}";
                        _instanceByKey[key] = unitGO;
                    }
                    PlaceUnitScene(unitGO, targetLocal);
                    break;
                }
                case UnitSourceMode.FromAddressables:
                {
                    if (addressBook == null || playerUnitsRoot == null) {
                        Debug.LogError("[BattleStart] Addressables 설정 누락(addressBook/playerUnitsRoot)");
                        continue;
                    }
                    if (!addressBook.TryGet(key, out var ar) || ar == null) {
                        Debug.LogWarning($"[BattleStart] Addressable 키 매핑 없음: {key}");
                        continue;
                    }
                    _pendingSpawnOps++;
                    CoroutineRunner.Run(EnsureAndPlaceAddressable(key, ar, targetLocal));
                    break;
                }
            }
        }

        // 3) UI 끄기(버튼 부모까지 포함)
        if (uiRootToDisable != null)
            uiRootToDisable.SetActive(false);

        // 4) 동기 스폰 모드는 지금 바로 AI 목록 갱신
        if (unitSource != UnitSourceMode.FromAddressables)
            NotifySetupCompleteWhenReady();
        // Addressables 모드는 코루틴 끝에서 호출
    }

    // ✅ 몇 개의 슬롯이 실제로 점유되었는지 계산
    private int GetOccupiedCount()
    {
        if (slots == null) return 0;
        int cnt = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s != null && s.IsOccupied && s.Occupant != null) cnt++;
        }
        return cnt;
    }

    private void PlaceUnitScene(GameObject unitGO, Vector3 targetLocal)
    {
        unitGO.SetActive(true);

        // X 스케일 -1(좌우 반전)
        var s = unitGO.transform.localScale;
        s.x = -Mathf.Abs(s.x);
        unitGO.transform.localScale = s;

        if (moveDuration > 0f)
            CoroutineRunner.Run(MoveLocal(unitGO.transform, targetLocal, moveDuration));
        else
            unitGO.transform.localPosition = targetLocal;
    }

    private IEnumerator EnsureAndPlaceAddressable(string key, AssetReferenceGameObject ar, Vector3 targetLocal)
    {
        if (!_instanceByKey.TryGetValue(key, out var go) || go == null)
        {
            var handle = ar.InstantiateAsync(playerUnitsRoot);
            yield return handle;
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[BattleStart] Addressables Instantiate 실패: {key}");
                _pendingSpawnOps = Mathf.Max(0, _pendingSpawnOps - 1);
                if (_pendingSpawnOps == 0) NotifySetupCompleteWhenReady();
                yield break;
            }
            go = handle.Result;
            go.name = $"Unit_{key}";
            _instanceByKey[key] = go;
        }

        // 활성화 + 반전 + 이동
        go.SetActive(true);
        var s = go.transform.localScale; s.x = -Mathf.Abs(s.x); go.transform.localScale = s;

        if (moveDuration > 0f) yield return MoveLocal(go.transform, targetLocal, moveDuration);
        else go.transform.localPosition = targetLocal;

        // 대기 카운터 처리
        _pendingSpawnOps = Mathf.Max(0, _pendingSpawnOps - 1);
        if (_pendingSpawnOps == 0) NotifySetupCompleteWhenReady();
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

    // 유닛 배치가 모두 끝났을 때 AI_Manager에 등록
    private void NotifySetupCompleteWhenReady()
    {
        if (aiManager == null) return;

        aiManager.unitPool.Clear();
        if (playerUnitsRoot != null) aiManager.unitPool.Add(playerUnitsRoot);
        if (enemyUnitsRoot  != null) aiManager.unitPool.Add(enemyUnitsRoot);

        aiManager.SetUnitList();
        // Debug.Log("[BattleStart] AI_Manager.SetUnitList 호출 완료");
    }
}
