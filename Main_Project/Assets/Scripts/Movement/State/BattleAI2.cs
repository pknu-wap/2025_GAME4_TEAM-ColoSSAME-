using System.Collections;
using UnityEngine;

namespace Movement.State
{
    public class BattleAI2 : MonoBehaviour
    {
        public float speed = 3f;
        public float attackRange = 1.5f;
        public float attackDelay = 1.0f;
        public LayerMask enemyLayer;
        public GameObject healthBar; 
        public bool isFacingRight = true;
        public string enemytag;
        public CharacterValue characterValue;
        public CapsuleCollider2D capsule;
        public bool isTakingDamage = false;

    
        private StateMachine stateMachine;
        public TargetingSystem targeting;
        private CharAnimator charAnimator;
        private MovementSystem movement;
        private Rigidbody2D rb;
        public BoxCollider2D weaponCollider;
        private void Awake()
        {
            stateMachine = new StateMachine();
            targeting = GetComponent<TargetingSystem>();
            charAnimator = GetComponentInChildren<CharAnimator>(); // 자식에서 찾음
            movement = GetComponent<MovementSystem>();
            rb = GetComponent<Rigidbody2D>();
            weaponCollider = GetComponentInChildren<BoxCollider2D>(true); // 비활성화된 상태에서도 찾음
            characterValue = GetComponentInChildren<CharacterValue>();
            capsule = GetComponent<CapsuleCollider2D>();
        
            targeting.Initialize(enemyLayer);   //외부에서 enemyLayer 지정
            targeting.StartTargeting(); //StartTargeting 호출

            // 무기 트리거 초기화
            WeaponTrigger trigger = weaponCollider.GetComponent<WeaponTrigger>();
            if (trigger != null)
                trigger.Initialize(this);
        }

        private void Start()
        {
            stateMachine.ChangeState(new IdleState(this, stateMachine));
            StartCoroutine(StateUpdater());
            DisableWeaponCollider();
        }

        private IEnumerator StateUpdater()
        {
            while (true)
            {
                yield return StartCoroutine(stateMachine.ExecuteState());
            }
        }

        public void StopMoving()
        {
            rb.velocity = Vector2.zero;
        }

        public void Move(Vector2 direction)
        {
            rb.velocity = direction * speed;
        }
    
        public void EnableWeaponCollider()
        {
            // 이미 OnTriggerEnter2D에서 중복 방지용 Reset이 필요하다고 했으니 추가
            WeaponTrigger trigger = weaponCollider.GetComponent<WeaponTrigger>();
            trigger?.ResetHitTargets();

            weaponCollider.enabled = true;

            // 일정 시간 후 자동으로 꺼지게 만듦 (중복 타격 방지 목적)
            Invoke(nameof(DisableWeaponCollider), 0.2f);
        }

        public void DisableWeaponCollider()
        {
            weaponCollider.enabled = false;
        }


        public Transform GetTarget()
        {
            return targeting.GetTarget();
        }

        public CharAnimator GetCharAnimator()
        {
            return charAnimator;
        }

        public Rigidbody2D GetRigidbody()
        {
            return rb;
        }

        public MovementSystem GetMovementSystem()
        {
            return movement;
        }
        public void Flip()
        {
            isFacingRight = !isFacingRight;

            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        
            // 체력바 방향 고정 호출
            if (healthBar != null)
            {
                healthBar.GetComponent<HealthBarFixDirection>().ForceFix();
            }
        
        }
    
        public void FaceTargetHorizontally(Vector3 targetPos)
        {
            if (targetPos.x < transform.position.x && isFacingRight)
            {
                // 왼쪽으로 방향 전환
                Flip();
            }
            else if (targetPos.x > transform.position.x && !isFacingRight)
            {
                // 오른쪽으로 방향 전환
                Flip();
            }
        }
        public void TakeDamage()
        {
            if (isTakingDamage) return;
            isTakingDamage = true;
            stateMachine.ChangeState(new DamageState(this, stateMachine));
        }

        public void KillThis()
        {
            Invoke(nameof(kill), 0.5f);
            Debug.Log("사망처리 실행됨");
        }

        private void kill()
        {
            Destroy(gameObject);
        }
    }
}
