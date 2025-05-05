using System;
using Battle.Scripts.Class.Close.Warrior.State;
using Battle.Scripts.Class.Close.Warrior.Weapon;
using Battle.Value;
using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior
{
    public enum TeamType { Player, Enemy }

    public enum WeaponType
    {
        Sword = 0,
        LongSpear = 66,
        Axe = 100,
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(CharacterValue))]
    public class WarriorAI : MonoBehaviour
    {
        public StateMachine StateMachine { get; private set; }
        public Transform CurrentTarget;
        
        public TeamType team;
        public WeaponType weaponType;

        public float sightRange = 5f;           // 시야 거리
        public float attackRange = 1.5f;        // 공격 범위
        public float moveSpeed = 2f;            // 이동 속도
        public float hp = 100f;                 // 체력
        public float damage = 1f;               // 공격력
        private float lastAttackTime;           // 공격 쿨타임

        [Min(0.5f)]
        public float AttackDelay = 0.5f;

        public WarriorAnimator warriorAnimator;
        public WeaponTrigger weaponTrigger;

        private Targeting Targeting;
        public Rigidbody2D rb;
        private CircleCollider2D PlayerCollider;
        
        private CharacterValue characterValue;

        private Transform child;   // UnitRoot
        private Transform Weapon;  // Weapon

        private void Awake()
        {
            Targeting = new Targeting(this);
            StateMachine = new StateMachine();
            characterValue = GetComponent<CharacterValue>();
        }

        private void Start()
        {
            // 무기 트리거 초기화
            if (weaponTrigger != null)
            {
                weaponTrigger.Initialize(this);
            }
            StateMachine.ChangeState(new WarriorIdleState(this));
        }

        private void Update()
        {
            StateMachine.Update();
        }

        // === 내부 동작 구현 ===

        public bool HasEnemyInSight()
        {
            return Targeting.FindNearestEnemy();
        }
        
        public bool CanAttack()
        {
            return Time.time >= lastAttackTime + AttackDelay;
        }
        
        public void RecordAttackTime()
        {
            lastAttackTime = Time.time;
        }
        
        public bool IsInAttackRange()
        {
            if (CurrentTarget == null) return false;

            float distance = Vector2.Distance(transform.position, CurrentTarget.position);
            return distance <= attackRange;
        }

        public void MoveTo(Vector3 target)
        {
            Vector2 direction = (target - transform.position).normalized;

            // 방향 전환
            transform.localScale = new Vector3(
                direction.x > 0 ? -1f : 1f,
                1f,
                1f
            );

            rb.velocity = direction * moveSpeed;
        }

        public void StopMoving()
        {
            rb.velocity = Vector2.zero;
        }

        public void UseSkill()
        {
            Debug.Log($"{gameObject.name} UseSkill");
        }

        public void TakeDamage(float amount)
        {
            Debug.Log($"Warrior took {amount} damage. Remaining HP: {characterValue.currentHp}");
            characterValue.TakeDamage(amount);
        }

        public bool IsDead()
        {
            return characterValue.currentHp <= 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

        // === 개발용 자동 설정 ===
        [ContextMenu("Setup Warrior Components Automatically")]
        public void SetupComponents()
        {
            characterValue = GetComponent<CharacterValue>();
            characterValue.StartSetting();
            rb = GetComponent<Rigidbody2D>();
            PlayerCollider = GetComponent<CircleCollider2D>();
            Weapon = transform.Find("Weapon");
            if (Weapon != null)
            {
                if (Weapon.GetComponent<WeaponTrigger>() == null)
                    Weapon.gameObject.AddComponent<WeaponTrigger>();

                weaponTrigger = Weapon.GetComponent<WeaponTrigger>();
            }
            else
            {
                Debug.LogWarning("Weapon 오브젝트를 찾을 수 없습니다.");
            }

            child = transform.Find("UnitRoot");
            if (child != null)
            {
                if (child.GetComponent<WarriorAnimator>() == null)
                    child.gameObject.AddComponent<WarriorAnimator>();

                warriorAnimator = child.GetComponent<WarriorAnimator>();
            }
            else
            {
                Debug.LogWarning("UnitRoot 오브젝트를 찾을 수 없습니다.");
            }
            // 초기 Rigidbody 설정
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // Collider 설정
            PlayerCollider.isTrigger = true;
            PlayerCollider.offset = new Vector2(0f, 0.3f);

            Debug.Log("Warrior 설정이 완료되었습니다.");
        }
    }
}
