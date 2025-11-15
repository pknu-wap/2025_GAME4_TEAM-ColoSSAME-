using System;
using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.UI;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerObjC))]
public class AICore : MonoBehaviour, IDamageable
{
    public PlayerObjC player;

    [Header("Name")]
    public string Ko_Name;
    public string En_Name;

    [Header("Image")] public Sprite Image;
    
    [Header("HPBar")] public HPBar hpBar;

    [Header("Stats")]
    public int maxHp;
    public int hp;
    public int def;
    public int moveSpeed;
    public int attackDamage;
    public float attackSpeed;     // (구) 공격속도 기반, 남겨둠
    public float attackRange;
    public float sightRange = 5f;
    public float evasionRate;
    public float skillRange;
    public float skillDelay;

    [Header("Attack Timing")]
    [Tooltip("공격 후 애니메이션이 재생되는 동안 대기하는 시간(초) - 이 동안은 제자리 Idle 유지")]
    public float attackAnimationDelay = 0.15f;

    [Tooltip("피격 시 추가로 묶어두는 시간(초). DamageState에서 attackDelay와 합산해 제자리 Idle 유지")]
    public float stun = 0.2f;

    [Header("Attack Cooldown")]
    [Tooltip("공격 쿨다운(초). 0이면 attackSpeed 기반(1/attackSpeed)을 사용")]
    public float attackDelay = 0.0f;

    public TargetStrategy TargetStrategy;
    public State State;
    public UnitClass unitClass;
    public UnitAttackDelay UnitAttackDelay;

    [Header("Manager Link")]
    public AI_Manager aiManager;

    [Header("Targeting")]
    public Transform target;
    public LayerMask targetLayer;
    public List<UnitClass> targetClasses = new List<UnitClass> { UnitClass.Archer, UnitClass.Mage };
    public Targeting targeting;

    [Header("Movement / Pathfinding")]
    public AIDestinationSetter destinationSetter;
    public AIPath aiPath;

    [Header("Attacks")]
    public MeleeAttack meleeAttack;
    public RangedAttack rangedAttack;

    [Header("Etc")]
    public LayerMask obstacleLayer;
    public StateMachine StateMachine { get; private set; }
    public SkillUse skillUse;
    public SkillDatabase skillDatabase;
    public SkillCooldownManager cooldownManager;

    // SPUM 렌더러(선택)
    public SpriteRenderer[] renderers;
    public Color[] originalColors;

    [Header("Combat Type")]
    public bool isRanged = false;

    [SerializeField] private float facingBaseScale = 0.7f;
    private float _absBaseScale = 0.7f;

    private float _cachedMaxSpeed = -1f;

    // ── 생명/이벤트 ──────────────────────────────────────────────────────────────
    public bool IsDead { get; private set; }
    public event Action<AICore> OnDied;

    // ── 공격 쿨다운 내부 타이머 ────────────────────────────────────────────────
    private float _nextAttackTime = 0f;

    private void Awake()
    {
        hp = maxHp;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
        player = GetComponent<PlayerObjC>();
        player._prefabs = GetComponentInChildren<SPUM_Prefabs>();

        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();

        meleeAttack = GetComponentInChildren<MeleeAttack>();
        if (meleeAttack != null) meleeAttack.Initialize(this);

        rangedAttack = GetComponentInChildren<RangedAttack>();

        skillUse = GetComponent<SkillUse>();
        cooldownManager = GetComponent<SkillCooldownManager>();

        StateMachine = new StateMachine(this);

        _absBaseScale = Mathf.Abs(facingBaseScale) > 1e-4f ? Mathf.Abs(facingBaseScale) : 0.7f;
    }

    private void OnEnable()
    {
        if (aiManager == null)
        {
            if (AI_Manager.Instance != null) aiManager = AI_Manager.Instance;
            else
            {
                AI_Manager.OnReady += HandleManagerReady;
                var found = AIManagerLocator.FindInActiveScene();
                if (found != null)
                {
                    aiManager = found;
                    AI_Manager.OnReady -= HandleManagerReady;
                }
            }
        }

        if (aiManager != null) aiManager.TryRegister(this, guessSideByLayer: true);
    }

    private void OnDisable()
    {
        if (IsDead && State != State.Death)
            State = State.Death;

        AI_Manager.OnReady -= HandleManagerReady;
        if (aiManager != null) aiManager.Unregister(this);
    }

    private void HandleManagerReady(AI_Manager mgr)
    {
        if (aiManager == null) aiManager = mgr;
        AI_Manager.OnReady -= HandleManagerReady;
    }

    private void Start()
    {
        if (aiPath != null) aiPath.maxSpeed = moveSpeed;

        switch (TargetStrategy)
        {
            case TargetStrategy.NearestTarget:
                targeting = new Targeting(new NearestTarget());
                break;
            case TargetStrategy.NearestTargetWithClass:
                targeting = new Targeting(new NearestClassTargeting(targetClasses));
                break;
        }
        if (skillDatabase != null) skillDatabase.Init();
        StateMachine.ChangeState(new IdleState(this));
    }

    public void ApplyDamage(int amount, GameObject attacker)
    {
        if (IsDead) return;

        hp -= Mathf.Max(0, amount - def);
        if (hp <= 0)
        {
            Kill(attacker);
            return;
        }

        if (StateMachine != null && !(StateMachine.CurrentState is DeathState))
            StateMachine.ChangeState(new DamageState(this, amount));
    }

    /// <summary>죽음을 한 번만 처리</summary>
    public void Kill(GameObject killer = null)
    {
        if (IsDead) return;
        IsDead = true;

        State = State.Death;
        StopAllActionsHard();

        if (StateMachine != null)
            StateMachine.ChangeState(new DeathState(this));

        try { OnDied?.Invoke(this); } catch { }
    }

    public void StopAllActionsHard()
    {
        meleeAttack?.CancelAll();
        rangedAttack?.CancelAll();
        skillUse?.CancelAll();

        StopMovementHard(alsoZeroMaxSpeed: true);

        var anim = player?._prefabs?._anim;
        if (anim != null)
        {
            anim.ResetTrigger("ATTACK");
            anim.ResetTrigger("MOVE");
            anim.SetBool("isMoving", false);
        }

        CancelInvoke();
        StopAllCoroutines();
    }

    public void StopMovementHard(bool alsoZeroMaxSpeed = false)
    {
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
            try { aiPath.isStopped = true; } catch { }
            aiPath.destination = transform.position;
            aiPath.SearchPath();

            if (alsoZeroMaxSpeed)
            {
                if (_cachedMaxSpeed < 0f) _cachedMaxSpeed = aiPath.maxSpeed;
                aiPath.maxSpeed = 0f;
            }
        }

        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null) rb2d.velocity = Vector2.zero;
    }

    public void ResumePathfinding()
    {
        if (IsDead) return;
        if (aiPath != null)
        {
            aiPath.canSearch = true;
            if (_cachedMaxSpeed >= 0f)
            {
                aiPath.maxSpeed = _cachedMaxSpeed;
                _cachedMaxSpeed = -1f;
            }
            try { aiPath.isStopped = false; } catch { }
        }
    }

#if UNITY_EDITOR
    private void OnValidate() { TrySetupRenderers(); }

    [ContextMenu("Setup SPUM Renderers")]
    private void TrySetupRenderers()
    {
        SPUM_Prefabs spumPrefab = GetComponentInChildren<SPUM_Prefabs>(true);
        Transform unitRoot = null;
        if (spumPrefab != null) unitRoot = spumPrefab.transform.Find("UnitRoot/Root");
        if (unitRoot == null)   unitRoot = transform.Find("UnitRoot/Root");

        if (unitRoot != null)
        {
            renderers = unitRoot.GetComponentsInChildren<SpriteRenderer>(true);
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) originalColors[i] = renderers[i].color;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else
        {
            Debug.LogWarning($"[AICore] UnitRoot/Root 경로를 찾을 수 없습니다. ({gameObject.name})");
        }
    }
#endif

    // ─── 바라보기 유틸 ───
    public void FaceByDirX(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.0001f) return;
        float sign = dirX >= 0 ? -1f : 1f;
        transform.localScale = new Vector3(_absBaseScale * sign, _absBaseScale, 1f);
    }

    public void FaceToward(Vector3 worldPos)
    {
        float dx = worldPos.x - transform.position.x;
        FaceByDirX(dx);
    }

    public void FaceByVelocity(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f) return;
        FaceByDirX(v.x);
    }

    // ─── 스킬 헬퍼 ───
    public bool TryUseSkill()
    {
        if (IsDead || skillDatabase == null) return false;

        SkillData skill = skillDatabase.GetSkill(unitClass, 0);
        if (skill == null || target == null) return false;

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= skill.range && cooldownManager != null && cooldownManager.IsCooldownReady(skill.skillID))
        {
            cooldownManager.SetCooldown(skill.skillID, skill.cooldown);
            if (skillUse != null) skillUse.UseSkill(skill, this, target);
            return true;
        }
        return false;
    }

    // ─── 공격 쿨다운 API ───
    /// <summary>지금 공격 가능?</summary>
    public bool CanAttack()
    {
        if (IsDead) return false;
        return Time.time >= _nextAttackTime;
    }

    /// <summary>남은 공격 쿨다운(초)</summary>
    public float RemainingAttackCooldown()
    {
        return Mathf.Max(0f, _nextAttackTime - Time.time);
    }

    /// <summary>이번 공격 직후 쿨다운 시작(고정 attackDelay가 우선, 0이면 1/attackSpeed 사용)</summary>
    public void StampAttackCooldown()
    {
        float baseDelay = (attackDelay > 0f) ? attackDelay : Mathf.Clamp(1f / Mathf.Max(0.01f, attackSpeed), 0.1f, 2f);
        _nextAttackTime = Time.time + Mathf.Max(0f, baseDelay);
    }
}
