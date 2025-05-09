using Battle.Scripts.Ai.State;
using Battle.Scripts.StateCore;
using System.Collections;
using UnityEngine;
namespace Battle.Ai.State
{
    public class AttackState : IState
    {
        private BattleAI ai;
        private bool isFinished;
        public AttackState(BattleAI ai) { this.ai = ai; }

        public void EnterState()
        {
            isFinished = true;
            ai.aiAnimator.Reset();
            ai.aiAnimator.Attack();
            ai.weaponTrigger.ActivateCollider();
            ai.RecordAttackTime();
            if(isFinished) ai.StartCoroutine(AttackDelay());
        }

        private IEnumerator AttackDelay()
        {
            isFinished = false;
            yield return new WaitForSeconds(ai.attackRange / 2f); // 이 숫자 변수로 받을수도?
            ai.weaponTrigger.ColliderMove();
            // 무기 활성화 시간만큼 대기
            yield return new WaitForSeconds(ai.AttackDelay/2f); // 이 숫자도 변수로 받을수도?
            ai.weaponTrigger.DeactivateCollider();
            Debug.Log($"{ai.gameObject.name}의 AttackState에서 IdleState");
            ai.StateMachine.ChangeState(new IdleState(ai,false,ai.AttackDelay));
        }

        public void UpdateState()
        {
            
        }

        public void ExitState()
        {
        }
    }
}