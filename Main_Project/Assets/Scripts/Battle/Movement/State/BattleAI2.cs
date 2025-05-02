using System.Collections;
using Battle.Value;
using UnityEngine;

namespace Battle.Movement.State
{
    public class BattleAI2 : MonoBehaviour
    {
        //변수 지정
        public float speed = 3f;
        public float attackRange = 1.5f;
        public float attackDelay = 1.0f;
        public float damage = 1f;
        public bool isFacingRight = true;
        public CapsuleCollider2D capsule;
        public bool isTakingDamage = false;
        private bool isDeath = false;
        
        //함수 호출용 변수
        public LayerMask enemyLayer;
        public GameObject healthBar;
        public CharacterValue characterValue;
        private StateMachine stateMachine;
        public TargetingSystem targeting;
        private CharAnimator charAnimator;
        private MovementSystem movement;
        private Rigidbody2D rb;
        private IsWinner isWinner;
        public BoxCollider2D weaponCollider;
        private void Awake()
        {
            stateMachine = new StateMachine();  //상태 설정 준비
            targeting = GetComponent<TargetingSystem>();  //타겟 설정 준비
            charAnimator = GetComponentInChildren<CharAnimator>(); // 애니메이션 설정
            movement = GetComponent<MovementSystem>();  //움직임 설정
            rb = GetComponent<Rigidbody2D>();  //rigidbody2d 대입
            weaponCollider = GetComponentInChildren<BoxCollider2D>(true); //무기 설정
            characterValue = GetComponentInChildren<CharacterValue>();  //HP바 설정
            capsule = GetComponent<CapsuleCollider2D>();  //플레이어 Collider 설정
            isWinner = FindObjectOfType<IsWinner>();
        
            targeting.Initialize(enemyLayer);   //외부에서 enemyLayer 지정
            targeting.StartTargeting(); //StartTargeting 호출

            // 무기 트리거 초기화
            WeaponTrigger trigger = weaponCollider.GetComponent<WeaponTrigger>();
            if (trigger != null)
                trigger.Initialize(this);
        }

        private void Start()  //시작 설정
        {
            Debug.Log(isWinner);
            stateMachine.ChangeState(new IdleState(this, stateMachine));
            StartCoroutine(StateUpdater());
            DisableWeaponCollider();
        }

        private IEnumerator StateUpdater()  //ExecuteState 설정
        {
            while (true)
            {
                yield return StartCoroutine(stateMachine.ExecuteState());
            }
        }

        public void StopMoving()  //정지
        {
            rb.velocity = Vector2.zero;
        }

        public void Move(Vector2 direction) //이동
        {
            rb.velocity = direction * speed;
        }
    
        public void EnableWeaponCollider()  //무기 판정 허가
        {
            // 이미 OnTriggerEnter2D에서 중복 방지용 Reset이 필요하다고 했으니 추가
            WeaponTrigger trigger = weaponCollider.GetComponent<WeaponTrigger>();
            trigger?.ResetHitTargets();

            weaponCollider.enabled = true;

            // 일정 시간 후 자동으로 꺼지게 만듦 (중복 타격 방지 목적)
            Invoke(nameof(DisableWeaponCollider), 0.2f);
        }

        public void DisableWeaponCollider()  //무기 판정 제거
        {
            weaponCollider.enabled = false;
        }


        public Transform GetTarget()  //타겟 지정
        {
            return targeting.GetTarget();
        }

        public CharAnimator GetCharAnimator()  //캐릭터 애니메이션 설정
        {
            return charAnimator;
        }

        public Rigidbody2D GetRigidbody()  //rigidbody 대입
        {
            return rb;
        }

        public MovementSystem GetMovementSystem()  //움직임 반환
        {
            return movement;
        }
        public void Flip()  //좌우반전
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
    
        public void FaceTargetHorizontally(Vector3 targetPos)  //타겟 방향에 따른 좌우 반전
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
        public void TakeDamage(float damage)  //데미지 판정
        {
            if (isTakingDamage) return;
            isTakingDamage = true;
            stateMachine.ChangeState(new DamageState(this, stateMachine, damage));
        }


        public void KillThis()  //사망 처리(딜레이를 위한 함수)
        {
            isDeath = true;
            if (isDeath)
            {
                isDeath = false;
                Invoke(nameof(kill), 0.5f);
                Debug.Log("사망처리 실행됨");
            }
        }

        private void kill()  //사망 처리(실질적인 사망처리)
        {
            isWinner.Winner(this.gameObject);
            Destroy(gameObject);
        }
    }
}
