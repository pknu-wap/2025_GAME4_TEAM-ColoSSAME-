using UnityEngine;
using System.Collections;

public class BattleAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private TargetingSystem targeting;
    private MovementSystem movement;
    private float attackRange, attackDelay, retreatDistance, speed;
    private bool isAttacking = false;

    public void Initialize(Rigidbody2D rb, TargetingSystem targeting, MovementSystem movement, float attackRange, float attackDelay, float retreatDistance, float speed)
    {
        this.rb = rb;
        this.targeting = targeting;
        this.movement = movement;
        this.attackRange = attackRange;
        this.attackDelay = attackDelay;
        this.retreatDistance = retreatDistance;
        this.speed = speed;
    }

    public void StartBattle()
    {
        StartCoroutine(AutoBattleAI());
    }

    private IEnumerator AutoBattleAI()          // 자동 전투
    {
        while (true)
        {
            Transform target = targeting.GetTarget();
            if (target == null)
            {
                targeting.StartTargeting();
            }

            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                Vector2 direction = (target.position - transform.position).normalized;
                direction = movement.AvoidTeammates(direction);
                targeting.FaceTarget(target.position);

                if (distance > attackRange)
                {
                    rb.velocity = direction * speed;
                }
                else if (!isAttacking)
                {
                    StartCoroutine(AttackSequence(target));
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }

            yield return null;
        }
    }

    private IEnumerator AttackSequence(Transform target)        //공격
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        targeting.FaceTarget(target.position);

        Debug.Log("공격!");
        yield return new WaitForSeconds(attackDelay);

        Vector2 retreatDirection = movement.GetRetreatDirection(transform.position, target.position);
        float retreatTime = retreatDistance / speed;

        float elapsedTime = 0f;
        while (elapsedTime < retreatTime)
        {
            rb.velocity = retreatDirection * speed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        isAttacking = false;
    }
}
