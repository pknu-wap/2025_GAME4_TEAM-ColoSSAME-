using System.Collections;
using UnityEngine;

namespace Movement.State
{
    public class RetreatState : IState
    {
        private BattleAI2 ai;
        private MovementSystem movementSystem;
        private StateMachine stateMachine;
        private float retreatDuration;
        private float retreatSpeed;
        private float retreatDistance;

        public void Initialize(float retreatDistance)
        {
            this.retreatDistance = retreatDistance;
        }
    
        public RetreatState(BattleAI2 ai, StateMachine stateMachine)
        {
            this.ai = ai;
            this.stateMachine = stateMachine;
            this.movementSystem = ai.GetMovementSystem();  // ✅ MovementSystem 직접 참조
            retreatDuration = ai.attackDelay;
            retreatSpeed = ai.speed;
        }

        public void EnterState()
        {
            ai.StopMoving();
            ai.GetCharAnimator().Move(); // ✅ 후퇴 모션 적용
        }

        public IEnumerator ExecuteState()
        {
            yield return new WaitForSeconds(0.2f); // ✅ 후퇴 전 대기

            Transform target = ai.GetTarget();
            ai.Flip();
            if (target == null)
            {
                stateMachine.ChangeState(new IdleState(ai, stateMachine));  // 타겟 없으면 Idle
                yield break;
            }

            // ✅ `MovementSystem`을 활용하여 후퇴 방향 결정
            Vector2 retreatDirection = movementSystem.GetRetreatDirection(ai.transform.position, target.position);
            float retreatTime = retreatDuration / retreatSpeed;
        
            float elapsedTime = 0f;
            while (elapsedTime < retreatTime)
            {
                ai.Move(retreatDirection);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            ai.StopMoving();
            ai.GetCharAnimator().Idle(); 
            yield return new WaitForSeconds(0.2f);

            stateMachine.ChangeState(new IdleState(ai, stateMachine)); 
        }

        public void ExitState()
        {
            ai.StopMoving();
        }
    }
}