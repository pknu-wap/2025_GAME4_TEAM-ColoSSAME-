using System.Collections;
using UnityEngine;

namespace Character.Movement.State
{
    public class ChaseState : IState
    {
        private BattleAI2 ai;
        private StateMachine stateMachine;
        private Rigidbody2D rb;
        private MovementSystem movement;
        private Transform target;

        public ChaseState(BattleAI2 ai, StateMachine stateMachine, Transform target)
        {
            this.ai = ai;
            this.stateMachine = stateMachine;
            this.rb = ai.GetRigidbody();
            this.movement = ai.GetMovementSystem();
            this.target = target;
        }

        public void EnterState()
        {
            ai.GetCharAnimator().Move();
        }

        public IEnumerator ExecuteState()
        {
            while (true)
            {
                target = ai.GetTarget();
                if (target == null)
                {
                    stateMachine.ChangeState(new IdleState(ai, stateMachine));
                    yield break;
                }

                float distance = Vector2.Distance(ai.transform.position, target.position);
                Vector2 direction = (target.position - ai.transform.position).normalized;
                direction = movement.AvoidTeammates(direction);

                rb.velocity = direction * ai.speed;

                if (distance <= ai.attackRange)
                {
                    stateMachine.ChangeState(new AttackState(ai, stateMachine, target));
                    yield break;
                }

                yield return null;
            }
        }

        public void ExitState()
        {
            rb.velocity = Vector2.zero;
        }
    }
}