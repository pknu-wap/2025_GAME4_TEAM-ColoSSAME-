using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-500)] // PlayerObjC가 먼저 초기화되게
public class PlayerObjC : MonoBehaviour
{
    [Header("SPUM")]
    public SPUM_Prefabs _prefabs;

    [Header("Move/State")]
    public float _charMS = 3f;

    // 현재/직전 상태
    private PlayerState _currentState = PlayerState.IDLE;
    private PlayerState _lastPlayedState = (PlayerState)(-1);

    public bool isAction = false; // 공격 등 액션 중에는 Update에서 건드리지 않음

    // 상태별 재생 인덱스(버튼/AI가 선택)
    public Dictionary<PlayerState, int> IndexPair = new();

    // 내부 초기화 플래그
    private bool _controllerInitialized = false;
    private bool _listsPrepared = false;
    private bool _aliasesNormalized = false;

    private bool Ready => _controllerInitialized && _listsPrepared && _aliasesNormalized;

    void Start()
    {
        if (_prefabs == null)
            _prefabs = GetComponentInChildren<SPUM_Prefabs>(true);

        EnsureInitialized();

        foreach (PlayerState s in Enum.GetValues(typeof(PlayerState)))
            if (!IndexPair.ContainsKey(s)) IndexPair[s] = 0;

        // 시작 시 IDLE 1회만 재생
        ChangeState(PlayerState.IDLE);
    }

    /// 외부(AI/StateMachine)에서 진입 시 호출하면 안전
    public void EnsureInitialized()
    {
        if (_prefabs == null)
        {
            Debug.LogWarning($"[PlayerObjC] SPUM_Prefabs 가 없습니다. ({name})");
            return;
        }
        PrepareAnimationLists();
        NormalizeStateAliases();
        SafeInitOverrideController();
    }

    /// 매니저에서 프리팹 주입 직후 호출
    public void InitWithPrefabs(SPUM_Prefabs prefabs)
    {
        _prefabs = prefabs;
        EnsureInitialized();
    }

    private void PrepareAnimationLists()
    {
        if (_listsPrepared) return;
        if (_prefabs == null) return;

        try { _prefabs.PopulateAnimationLists(); } catch { /* 무시 */ }

        try
        {
            var keyNames = _prefabs.StateAnimationPairs.Keys.Select(k => k.ToString());
        }
        catch { /* 특수 구현일 수 있음 */ }

        _listsPrepared = true;
    }

    /// 사전 키(string)와 enum 이름을 1:1로 연결(별칭 추가)
    private void NormalizeStateAliases()
    {
        if (_aliasesNormalized) return;
        if (_prefabs == null || _prefabs.StateAnimationPairs == null) return;

        try
        {
            var keys = _prefabs.StateAnimationPairs.Keys.ToList();
            var ciSet = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);

            foreach (PlayerState s in Enum.GetValues(typeof(PlayerState)))
            {
                string enumKey = s.ToString(); // "IDLE","MOVE",...
                if (ciSet.Contains(enumKey)) continue;

                string found = FindClosestKey(keys, s);
                if (!string.IsNullOrEmpty(found))
                {
                    var listForFound = _prefabs.StateAnimationPairs[found];
                    _prefabs.StateAnimationPairs[enumKey] = listForFound; // 별칭
                    keys.Add(enumKey);
                    ciSet.Add(enumKey);
                    Debug.Log($"[PlayerObjC] Alias: '{enumKey}' → '{found}' ({_prefabs.name})");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerObjC] NormalizeStateAliases 예외: {e.Message}");
        }

        _aliasesNormalized = true;
    }

    private string FindClosestKey(List<string> keys, PlayerState s)
    {
        string target = s.ToString();
        var exact = keys.FirstOrDefault(k => string.Equals(k, target, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(exact)) return exact;

        switch (s)
        {
            case PlayerState.IDLE:
                return keys.FirstOrDefault(k =>
                    k.IndexOf("IDLE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("STAND", StringComparison.OrdinalIgnoreCase) >= 0);
            case PlayerState.MOVE:
                return keys.FirstOrDefault(k =>
                    k.IndexOf("MOVE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("WALK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("RUN", StringComparison.OrdinalIgnoreCase) >= 0);
            case PlayerState.ATTACK:
                return keys.FirstOrDefault(k =>
                    k.IndexOf("ATTACK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("ATK", StringComparison.OrdinalIgnoreCase) >= 0);
            case PlayerState.DAMAGED:
                return keys.FirstOrDefault(k =>
                    k.IndexOf("DAMAGED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("DAMAGE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("HIT", StringComparison.OrdinalIgnoreCase) >= 0);
            case PlayerState.DEBUFF:
                return keys.FirstOrDefault(k => k.IndexOf("DEBUFF", StringComparison.OrdinalIgnoreCase) >= 0);
            case PlayerState.DEATH:
                return keys.FirstOrDefault(k =>
                    k.IndexOf("DEATH", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("DIE", StringComparison.OrdinalIgnoreCase) >= 0);
            case PlayerState.OTHER:
                return keys.FirstOrDefault(k =>
                    k.IndexOf("OTHER", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    k.IndexOf("ETC", StringComparison.OrdinalIgnoreCase) >= 0);
        }
        return null;
    }

    private void SafeInitOverrideController()
    {
        if (_controllerInitialized) return;
        if (_prefabs == null || _prefabs._anim == null) return;

        var anim = _prefabs._anim;
        var current = anim.runtimeAnimatorController;

        var baseCtrl = UnwrapToBaseController(current);
        if (baseCtrl == null)
        {
            Debug.LogError($"[PlayerObjC] AnimatorOverrideController의 base가 없습니다. Animator에 '순정 AnimatorController'를 지정해주세요. ({name})");
            return;
        }

        if (current != baseCtrl)
            anim.runtimeAnimatorController = baseCtrl;

        try { _prefabs.OverrideControllerInit(); }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerObjC] OverrideControllerInit 오류: {e.Message}");
        }

        _controllerInitialized = true;
    }

    private RuntimeAnimatorController UnwrapToBaseController(RuntimeAnimatorController ctrl)
    {
        if (ctrl == null) return null;
        var cur = ctrl; int guard = 0;
        while (cur is AnimatorOverrideController aoc)
        {
            cur = aoc.runtimeAnimatorController;
            guard++; if (guard > 8) break;
        }
        return cur;
    }

    // ─────────────────────────────
    // 외부(AI/입력)에서 반드시 이걸 통해 상태 변경
    // ─────────────────────────────
    public void ChangeState(PlayerState newState, int? indexOverride = null, bool stopAction = true)
    {
        if (indexOverride.HasValue)
            SetStateAnimationIndex(newState, indexOverride.Value);

        if (stopAction) isAction = false; // 이동/대기 전환 시 액션 해제

        _currentState = newState;

        // 준비가 안 됐으면 초기화만 하고, 재생은 다음 프레임에
        if (!Ready)
        {
            EnsureInitialized();
            return;
        }

        // 상태가 바뀔 때만 재생
        if (_currentState != _lastPlayedState)
        {
            PlayStateAnimation(_currentState);
            _lastPlayedState = _currentState;
        }
    }

    public void SetStateAnimationIndex(PlayerState state, int index = 0)
    {
        IndexPair[state] = index;
    }

    public void PlayStateAnimation(PlayerState state)
    {
        if (_prefabs == null) return;
        if (!_listsPrepared) PrepareAnimationLists();
        if (!_aliasesNormalized) NormalizeStateAliases();

        int idx = IndexPair.TryGetValue(state, out var v) ? v : 0;

        try
        {
            _prefabs.PlayAnimation(state, idx);
        }
        catch (KeyNotFoundException)
        {
            if (state != PlayerState.IDLE)
            {
                int idleIdx = IndexPair.TryGetValue(PlayerState.IDLE, out var iv) ? iv : 0;
                Debug.LogWarning($"[PlayerObjC] '{state}' 키를 못 찾아 IDLE로 대체 재생합니다. ({_prefabs.name})");
                try { _prefabs.PlayAnimation(PlayerState.IDLE, idleIdx); } catch { /* 무시 */ }
            }
            else
            {
                Debug.LogWarning($"[PlayerObjC] IDLE 재생 실패. 프리팹 세팅 확인 필요. ({_prefabs.name})");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlayerObjC] PlayStateAnimation 예외: {e.Message}");
        }
    }

    void Update()
    {
        if (isAction) return; // 공격/스킬 등 액션 동안은 상태를 덮지 않음

        // 더 이상 매 프레임 재생하지 않음.
        // 외부에서 ChangeState(...)로만 상태를 바꾸고,
        // 여기서는 변경 검지 시에만 1회 재생.
        if (_currentState != _lastPlayedState && Ready)
        {
            PlayStateAnimation(_currentState);
            _lastPlayedState = _currentState;
        }

        // (원래 쓰던 Z-보정 유지)
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.localPosition.y * 0.01f);
    }
}
