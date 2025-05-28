using System.Collections;
using Battle.Scripts.Ai.State;
using Battle.Scripts.Ai.Weapon;
using Battle.Scripts.Value;
using Battle.Scripts.Value.Data.Class;
using Battle.Scripts.Value.HpBar;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Screen = UnityEngine.Device.Screen;
using StateMachine = Battle.Scripts.StateCore.StateMachine;
namespace Battle.Scripts.Ai
{
    public enum TeamType { Player, Enemy }
    public enum WeaponType { Sword = 0, LongSpear = 66, TwoHanded = 100, ShortSword = 83, Bow = 16, Magic = 33,}

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(CharacterValue))]
    public class BattleAI : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        
        public Transform CurrentTarget;
        public Transform tempTarget;
        public Transform Retreater;
        
        public Vector2 retreatAreaMin = new Vector2(-7f, -3f);  // 좌하단 모서리
        public Vector2 retreatAreaMax = new Vector2(7f, 3f);    // 우상단 모서리

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
        public float stunTime = 0f;
        public ClassType Class;
        public AudioClip attackSound;
        
        private bool isDead;
        public void ApplyJobSettings()
        {
            isDead = false;
            if (!ClassDataBase.ClassStatsMap.TryGetValue(Class, out var stats))
            {
                Debug.LogError($"[BattleAI] 존재하지 않는 직업: {Class}");
                return;
            }

            if (!ClassDataBase.ClassRandomRanges.TryGetValue(Class, out var range))
            {
                Debug.LogError($"[BattleAI] {Class}의 랜덤 범위 없음");
                return;
            }

            hp = RandomInRangeInt(stats.hp, range.hpRange);
            damage = RandomInRangeFloat(stats.attack, range.attackRange);
            defense = RandomInRangeInt(stats.defense, range.defenseRange);
            moveSpeed = RandomInRangeFloat(stats.moveSpeed, range.moveSpeedRange);
            attackRange = stats.attackRange;
            AttackDelay = RandomInRangeFloat(stats.attackDelay, range.attackDelayRange);
            weaponType = stats.weaponType;
            retreatDistance = stats.retreatDistance;

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
            if (range <= 0) return Mathf.Round(baseValue); // 또는 (int)baseValue

            float min = baseValue - range;
            float max = baseValue + range;

            int result = Random.Range(Mathf.CeilToInt(min), Mathf.FloorToInt(max + 1)); // 정수 범위

            return result; // float으로 반환되지만 값은 항상 정수 (예: 117.0f)
        }
        
        public bool IsRetreating = false;

        [Min(0.25f)] public float AttackDelay = 0.5f;

        private float lastAttackTime;

        public AiAnimator aiAnimator;
        public WeaponTrigger weaponTrigger;
        public ArrowWeapon arrowWeaponTrigger;
        public GameObject HealthBar;

        public Targeting Targeting;
        public Rigidbody2D rb;
        private CircleCollider2D PlayerCollider;
        public CharacterValue characterValue;

        private Transform child;
        public GameObject Weapon;
        
        public SpriteRenderer[] renderers;
        public Color[] originalColors;
        private Coroutine flashCoroutine;
        
        public AIPath aiPath;
        public AIDestinationSetter destinationSetter;
        
        public LayerMask obstacleMask;
        
        public IsWinner isWinner;
        private AddPrefab addPrefab;
        private void Awake()
        {
            Targeting = new Targeting(this);
            StateMachine = new StateMachine();
            characterValue = GetComponent<CharacterValue>();
            addPrefab = GetComponent<AddPrefab>();
        }

        public void BattleStart()
        {
            isWinner = IsWinner.Instance;
            if (weaponTrigger != null) // 근접 무기 설정
            {
                weaponTrigger.Initialize(this);
            }
            
            StateMachine.ChangeState(new IdleState(this, true, 0f));
        }

        private void Update()
        {
            ValidateTarget();
            StateMachine.Update();
        }

        public bool HasEnemyInSight()
        {
            if (CurrentTarget == null)
            {
                CurrentTarget = Targeting.FindNearestEnemy();
            }
            return CurrentTarget != null;
        }

        public bool CanAttack()
        {
            aiAnimator.StopMove();
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
            float distance = Vector2.Distance(transform.position, target);

            if (distance < 0.1f)  // ← 너무 가까우면 이동하지 않도록
            {
                StopMoving();
                return;
            }

            aiPath.canMove = true;
            Flip(target);
        }

        public void Flip(Vector3 target)
        {
            Vector2 direction = (target - transform.position).normalized;
            transform.localScale = new Vector3(direction.x > 0 ? -0.8f : 0.8f, 0.8f, 0.8f);
            HealthBar?.GetComponent<HealthBarFixDirection>()?.ForceFix();
        }
        
        public void FlipToRight()
        {
            Flip(transform.position + Vector3.right);
        }

        public void FlipToLeft()
        {
            Flip(transform.position + Vector3.left);
        }

        public void StopMoving()
        {
            rb.velocity = Vector2.zero;
            aiPath.canMove = false;
        }

        public bool IsInRetreatDistance() {
            return Vector2.Distance(transform.position, Retreater.position) <= 0.1f;
        }

        public void UseSkill()
        {
            
        }
        
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
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashColorBlend(new Color(1f, 0f, 0f, alpha), duration));
        }

        private IEnumerator FlashColorBlend(Color flashColor, float duration)
        {
            // 붉게 + 반투명하게 덧씌움
            for (int i = 0; i < renderers.Length; i++)
            {
                Color blended = Color.Lerp(originalColors[i], flashColor, 0.6f); // 붉은 기조합
                blended.a = flashColor.a;
                renderers[i].color = blended;
            }

            yield return new WaitForSeconds(duration);

            // 정확하게 원래 색으로 복원
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].color = originalColors[i];
            }

            flashCoroutine = null;
        }
        
        public bool IsDead() => characterValue.currentHp <= 0;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

        public void Kill()
        {
            if(isDead) return;
            isDead = true;
            isWinner.Winner(gameObject);
            Destroy(Retreater.gameObject);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        // AI 설정창
        [ContextMenu("Setup AI Components Automatically")]
        public void SetupComponents()
        {
            characterValue = GetComponent<CharacterValue>();
            characterValue.StartSetting();

            rb = GetComponent<Rigidbody2D>();
            PlayerCollider = GetComponent<CircleCollider2D>();
            
            // Retreater 생성 및 연결
            if (Retreater == null)
            {
                GameObject retreatObj = new GameObject(this.gameObject.name + "Retreat");
                var retreatTarget = retreatObj.AddComponent<RetreatTarget>();
                retreatTarget.ai = this;
                Retreater = retreatObj.transform;
            }
            
            // UnitRoot 연결
            child = transform.Find("UnitRoot");
            if (!child.TryGetComponent(out aiAnimator))
            {
                aiAnimator = child.gameObject.AddComponent<AiAnimator>();
            }
            
            // 캐릭터 전체 렌더러와 원래 색상 저장
            renderers = transform.Find("UnitRoot/Root").GetComponentsInChildren<SpriteRenderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].color;
            }

            // Rigidbody 설정
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Collider 설정
            PlayerCollider.isTrigger = true;
            PlayerCollider.radius = 0.5f;
            PlayerCollider.offset = new Vector2(0f, 0f);

            // A* 알고리즘 연결
            if (GetComponent<AIPath>() == null)
            {
                gameObject.AddComponent<AIPath>();
                Debug.Log("AIPath 컴포넌트를 추가했습니다.");
            }
            aiPath = GetComponent<AIPath>();
            destinationSetter = GetComponent<AIDestinationSetter>();
            
            //A* 알고리즘 설정
            aiPath.maxSpeed = moveSpeed;
            aiPath.orientation = OrientationMode.YAxisForward;
            aiPath.whenCloseToDestination = CloseToDestinationMode.ContinueToExactDestination;
            aiPath.enableRotation = false;
            
            ApplyJobSettings();
        }
    }
}