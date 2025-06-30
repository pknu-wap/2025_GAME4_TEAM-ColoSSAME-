using System.Collections;
using Battle.Scripts.Ai.State;
using Battle.Scripts.Ai.Weapon;
using Battle.Scripts.Value;
using Battle.Scripts.Value.Data.Class;
using Battle.Scripts.Value.HpBar;
using Pathfinding;
using UnityEngine;
using CharacterInfo = Battle.Scripts.Value.Data.CharacterInfo;
using StateMachine = Battle.Scripts.StateCore.StateMachine;

namespace Battle.Scripts.Ai
{
    public enum TeamType { Player, Enemy }
    public enum WeaponType { Sword = 0, LongSpear = 66, TwoHanded = 100, ShortSword = 83, Bow = 16, Magic = 33 }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(CharacterValue))]
    public class BattleAI : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        public Transform CurrentTarget;
        public Transform Retreater;

        public Vector2 retreatAreaMin = new Vector2(-7f, -3f);
        public Vector2 retreatAreaMax = new Vector2(7f, 3f);

        public TeamType team;
        public WeaponType weaponType;

        public float sightRange = 20f;
        public float attackRange = 0.5f;
        public float moveSpeed = 2f;
        public float hp = 100f;
        public float defense = 1f;
        public float damage = 1f;
        public float retreatDistance = 3f;
        public float waitTime = 0.25f;
        public float stunTime;
        public ClassType Class;
        public AudioClip attackSound;
        public AudioClip skillSound;

        [Min(0.25f)] public float AttackDelay = 0.5f;
        private float lastAttackTime;

        public AiAnimator aiAnimator;
        public WeaponTrigger weaponTrigger;
        public ArrowWeapon arrowWeaponTrigger;
        public GameObject HealthBar;
        public GameObject Weapon;

        public Targeting Targeting;
        public Rigidbody2D rb;
        private CircleCollider2D PlayerCollider;
        public CharacterValue characterValue;
        public SpriteRenderer[] renderers;
        public Color[] originalColors;
        private Coroutine flashCoroutine;
        public AIPath aiPath;
        public AIDestinationSetter destinationSetter;
        public LayerMask obstacleMask;
        public IsWinner isWinner;
        private AddPrefab addPrefab;
        private bool isDead;

        private void Awake()
        {
            Targeting = new Targeting(this);
            StateMachine = new StateMachine();
            addPrefab = GetComponent<AddPrefab>();
        }

        public void BattleStart()
        {
            isWinner = IsWinner.Instance;
            if (weaponTrigger != null)
                weaponTrigger.Initialize(this);

            StateMachine.ChangeState(new IdleState(this, true, 0f));
        }

        private void Update()
        {
            ValidateTarget();
            StateMachine.Update();
        }

        public void ApplyRandomJobSettings()
        {
            isDead = false;

            if (!ClassDataBase.ClassStatsMap.TryGetValue(Class, out var stats) ||
                !ClassDataBase.ClassRandomRanges.TryGetValue(Class, out var range))
            {
                Debug.LogError($"[BattleAI] {Class} 직업 설정 오류");
                return;
            }

            hp = RandomInRangeInt(stats.hp, range.hpRange);
            damage = RandomInRangeFloat(stats.attack, range.attackRange);
            defense = RandomInRangeFloat(stats.defense, range.defenseRange);
            moveSpeed = RandomInRangeFloat(stats.moveSpeed, range.moveSpeedRange);
            attackRange = stats.attackRange;
            AttackDelay = RandomInRangeFloat(stats.attackDelay, range.attackDelayRange);
            weaponType = stats.weaponType;
            retreatDistance = stats.retreatDistance;

            ApplyStatToComponents();
        }

        public void ApplyLoadedStats(CharacterInfo info)
        {
            if (!ClassDataBase.ClassStatsMap.TryGetValue(info.classType, out var classStat))
            {
                Debug.LogError($"[BattleAI] {info.classType}에 대한 기본 스탯을 찾을 수 없습니다.");
                return;
            }

            hp = info.CON;
            damage = info.ATK;
            defense = info.DEF;
            moveSpeed = classStat.moveSpeed;
            attackRange = classStat.attackRange;
            AttackDelay = classStat.attackDelay;
            retreatDistance = classStat.retreatDistance;
            weaponType = info.weaponType;
            Class = info.classType;
            team = info.team;

            ApplyStatToComponents();
        }

        private void ApplyStatToComponents()
        {
            characterValue.maxHp = hp;
            characterValue.currentHp = hp;

            addPrefab.AddWeapon(this);
            addPrefab.LoadHpPrefab(this);
            aiPath.maxSpeed = moveSpeed;
            HealthBar?.GetComponent<HealthBarFixDirection>()?.ForceFix();
        }

        private float RandomInRangeFloat(float baseValue, float range)
        {
            return range > 0 ? Mathf.Round(Random.Range(baseValue - range, baseValue + range) * 10f) / 10f : baseValue;
        }

        private float RandomInRangeInt(float baseValue, float range)
        {
            if (range <= 0) return Mathf.Round(baseValue);
            float min = baseValue - range;
            float max = baseValue + range;
            return Random.Range(Mathf.CeilToInt(min), Mathf.FloorToInt(max + 1));
        }

        public bool HasEnemyInSight()
        {
            if (CurrentTarget == null)
                CurrentTarget = Targeting.FindNearestEnemy();
            return CurrentTarget != null;
        }

        public bool CanAttack()
        {
            return Time.time >= lastAttackTime + AttackDelay;
        }

        public void RecordAttackTime() => lastAttackTime = Time.time;

        public bool IsInAttackRange()
        {
            if (CurrentTarget == null) return false;
            StopMoving();
            return Vector2.Distance(transform.position, CurrentTarget.position) <= attackRange;
        }

        public void MoveTo(Vector3 target)
        {
            aiPath.canMove = true;
            Flip(target);
        }

        public void Flip(Vector3 target)
        {
            Vector2 dir = (target - transform.position).normalized;
            transform.localScale = new Vector3(dir.x > 0 ? -0.8f : 0.8f, 0.8f, 0.8f);
        }

        public void FlipToRight() => Flip(transform.position + Vector3.right);
        public void FlipToLeft() => Flip(transform.position + Vector3.left);

        public void StopMoving()
        {
            rb.velocity = Vector2.zero;
            aiPath.canMove = false;
        }

        public bool IsInRetreatDistance()
        {
            return Vector2.Distance(transform.position, Retreater.position) <= 0.1f;
        }

        public void UseSkill() { /* To be implemented */ }

        private void ValidateTarget()
        {
            if (CurrentTarget == null || CurrentTarget.Equals(null))
            {
                CurrentTarget = null;
                StopMoving();
            }

            if (destinationSetter.target == null || destinationSetter.target.Equals(null))
            {
                destinationSetter.target = null;
                StopMoving();
            }
        }

        public void TakeDamage(float amount)
        {
            characterValue.TakeDamage(amount);
        }

        public void FlashRedTransparent(float alpha = 0.5f, float duration = 0.2f)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashColorBlend(new Color(1f, 0f, 0f, alpha), duration));
        }

        private IEnumerator FlashColorBlend(Color flashColor, float duration)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                Color blended = Color.Lerp(originalColors[i], flashColor, 0.6f);
                blended.a = flashColor.a;
                renderers[i].color = blended;
            }

            yield return new WaitForSeconds(duration);

            for (int i = 0; i < renderers.Length; i++)
                renderers[i].color = originalColors[i];
        }

        public bool IsDead() => characterValue.currentHp <= 0;

        public void Kill()
        {
            if (isDead) return;
            StopMoving();
            isDead = true;
            isWinner.Winner(gameObject);
            Destroy(Retreater.gameObject);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

        [ContextMenu("Setup AI Components Automatically")]
        public void SetupComponents()
        {
            characterValue = GetComponent<CharacterValue>();
            characterValue.StartSetting();

            rb = GetComponent<Rigidbody2D>();
            PlayerCollider = GetComponent<CircleCollider2D>();

            if (Retreater == null)
            {
                GameObject retreatObj = new GameObject(this.gameObject.name + "Retreat");
                var retreatTarget = retreatObj.AddComponent<RetreatTarget>();
                retreatTarget.ai = this;
                Retreater = retreatObj.transform;
            }

            var child = transform.Find("UnitRoot");
            if (!child.TryGetComponent(out aiAnimator))
                aiAnimator = child.gameObject.AddComponent<AiAnimator>();

            renderers = transform.Find("UnitRoot/Root").GetComponentsInChildren<SpriteRenderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                originalColors[i] = renderers[i].color;

            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            PlayerCollider.isTrigger = true;
            PlayerCollider.radius = 0.5f;
            PlayerCollider.offset = Vector2.zero;

            if (GetComponent<AIPath>() == null)
                gameObject.AddComponent<AIPath>();

            aiPath = GetComponent<AIPath>();
            destinationSetter = GetComponent<AIDestinationSetter>();

            aiPath.maxSpeed = moveSpeed;
            aiPath.orientation = OrientationMode.YAxisForward;
            aiPath.whenCloseToDestination = CloseToDestinationMode.ContinueToExactDestination;
            aiPath.enableRotation = false;
        }
    }
}
