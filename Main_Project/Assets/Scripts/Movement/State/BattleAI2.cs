using System.Collections;
using UnityEngine;
using Character.Movement.State;

public class BattleAI2 : MonoBehaviour
{
    public float speed = 3f;
    public float attackRange = 1.5f;
    public float attackDelay = 1.0f;
    public LayerMask enemyLayer;
    public GameObject healthBar; 
    public bool isFacingRight = true;
    
    private StateMachine stateMachine;
    private TargetingSystem targeting;
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
        
        targeting.Initialize(enemyLayer);   //외부에서 enemyLayer 지정
        targeting.StartTargeting(); //StartTargeting 호출
    }

    private void Start()
    {
        stateMachine.ChangeState(new IdleState(this, stateMachine));
        StartCoroutine(StateUpdater());
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
        weaponCollider.enabled = true;
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
        if (targetPos.x > transform.position.x && isFacingRight)
        {
            // 왼쪽으로 방향 전환
            Flip();
        }
        else if (targetPos.x < transform.position.x && !isFacingRight)
        {
            // 오른쪽으로 방향 전환
            Flip();
        }
    }
}
