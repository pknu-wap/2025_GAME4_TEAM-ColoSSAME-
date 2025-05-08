using System.Collections;
using Battle.Scripts.StateCore;
using UnityEngine;

namespace Battle.Scripts.Ai.State
{
    public class AttackState : IState
    {
        private BattleAI ai;
        private bool isFinished;

        public AttackState(BattleAI ai) { this.ai = ai; }

        public void EnterState()
        {
            ai.aiPath.canMove = false;
            ai.StopMoving();
            ai.aiAnimator.StopMove();
            ai.aiAnimator.Reset();
            ai.aiAnimator.Attack();
            
            ai.weaponTrigger.ActivateCollider();
            
            ai.RecordAttackTime();
            
            isFinished = false;
            ai.StartCoroutine(AttackDelay());
        }

        private IEnumerator AttackDelay()
        {
            // 무기 활성화 시간만큼 대기
            yield return new WaitForSeconds(ai.AttackDelay/2f); // << 더 빨리 켜지게 하기 위해 StartCoroutine보다 먼저 호출도 가능
            ai.weaponTrigger.DeactivateCollider();

            // 공격 애니메이션 끝날 때까지 기다림
            yield return new WaitForSeconds(ai.AttackDelay/2f); // 또는 ai.AttackDelay * 0.7f
            isFinished = true;
        }

        public void UpdateState()
        {
            if (isFinished)
            {
                ai.StateMachine.ChangeState(new IdleState(ai));
                Debug.Log("공격 종료");
            }
        }

        public void ExitState()
        {
            ai.StopMoving();
        }
    }
}