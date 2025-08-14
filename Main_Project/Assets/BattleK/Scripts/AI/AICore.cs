using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(PlayerObjC))]
public class AICore : MonoBehaviour, IDamageable
{
    public PlayerObjC player;

    public int hp;
    public int def;
    public int moveSpeed;
    public int attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float sightRange = 5f;
    public float evasionRate;
    public float skillRange;
    public float skillDelay;

    public TargetStrategy TargetStrategy;

    public State State;
    public UnitClass unitClass;
    public UnitAttackDelay UnitAttackDelay;

    public AI_Manager aiManager;

    public Transform target;
    public LayerMask targetLayer;
    public List<UnitClass> targetClasses = new List<UnitClass> { UnitClass.Archer, UnitClass.Mage };
    public Targeting targeting;

    public AIDestinationSetter destinationSetter;
    public AIPath aiPath;

    public MeleeAttack meleeAttack;
    public RangedAttack rangedAttack;

    public LayerMask obstacleLayer;
    public StateMachine StateMachine { get; private set; }

    public SkillUse skillUse;
    public SkillDatabase skillDatabase;
    public SkillCooldownManager cooldownManager;

    // SPUM 렌더러(선택)
    public SpriteRenderer[] renderers;
    public Color[] originalColors;

    // 전투 타입 플래그 (인스펙터에서 설정)
    [Header("Combat Type")]
    public bool isRanged = false;

    // 바라보기(스케일 반전) 기준값
    [SerializeField] private float facingBaseScale = 0.7f;
    private float _absBaseScale = 0.7f;

    // maxSpeed 0으로 잠금 시 복구용
    private float _cachedMaxSpeed = -1f;

    private void Awake()
    {
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

    public bool TryUseSkill()
    {
        if (skillDatabase == null) return false;

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

    // 피해 입구 (투사체/근접에서 호출)
    public void ApplyDamage(int amount, GameObject attacker)
    {
        if (StateMachine != null)
            StateMachine.ChangeState(new DamageState(this, amount));
    }

#if UNITY_EDITOR
    private void OnValidate() { TrySetupRenderers(); }

    [ContextMenu("Setup SPUM Renderers")]
    private void TrySetupRenderers()
    {
        Transform unitRoot = null;
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("SPUM_", StringComparison.Ordinal))
            {
                unitRoot = child.Find("UnitRoot/Root");
                if (unitRoot != null) break;
            }
        }
        if (unitRoot == null) unitRoot = transform.Find("UnitRoot/Root");

        if (unitRoot != null)
        {
            renderers = unitRoot.GetComponentsInChildren<SpriteRenderer>(true);
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) originalColors[i] = renderers[i].color;
            UnityEditor.EditorUtility.SetDirty(this);
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
        // 프로젝트 규칙: 왼쪽 보면 +, 오른쪽 보면 - (0.7 / -0.7)
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

    // ─── 공격 중 이동 완전 정지(미끄러짐 방지) ───
    public void StopMovementHard(bool alsoZeroMaxSpeed = false)
    {
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
            try { aiPath.isStopped = true; } catch { /* 구버전 예외 무시 */ }

            // 목적지를 현재 위치로 고정(steering 끌림 방지)
            aiPath.destination = transform.position;
            aiPath.SearchPath();

            // 선택: maxSpeed=0으로 추가 차단(복구 필요)
            if (alsoZeroMaxSpeed)
            {
                if (_cachedMaxSpeed < 0f) _cachedMaxSpeed = aiPath.maxSpeed;
                aiPath.maxSpeed = 0f;
            }
        }

        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null) rb2d.velocity = Vector2.zero;
    }

    // 공격 종료 후 탐색 재개 (이동 on/off는 각 상태에서 제어)
    public void ResumePathfinding()
    {
        if (aiPath != null)
        {
            aiPath.canSearch = true;
            // maxSpeed 0으로 잠그었으면 복구
            if (_cachedMaxSpeed >= 0f)
            {
                aiPath.maxSpeed = _cachedMaxSpeed;
                _cachedMaxSpeed = -1f;
            }
            try { aiPath.isStopped = false; } catch { }
            // aiPath.canMove는 Move/Retreat에서 켭니다.
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
