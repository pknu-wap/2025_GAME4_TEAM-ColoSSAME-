using System.Collections;
using UnityEngine;

namespace Movement.State
{
    public class AttackState : IState
    {
        private BattleAI2 ai;
        private StateMachine stateMachine;
        private Transform target;

        public AttackState(BattleAI2 ai, StateMachine stateMachine, Transform target)
        {
            this.ai = ai;
            this.stateMachine = stateMachine;
            this.target = target;
        }

        public void EnterState()
        {
            ai.StopMoving(); // ✅ 이동 정지
            ai.GetCharAnimator().Attack(); // ✅ 공격 애니메이션 실행
            ai.EnableWeaponCollider(); // ✅ 공격 판정 활성화
        }

        public IEnumerator ExecuteState()
        {
            // ✅ 공격 애니메이션이 끝나는 시간을 기다리되, 너무 길지 않도록 설정
            yield return new WaitForSeconds(0.3f); // 필요하면 조정 가능

            ai.DisableWeaponCollider(); // ✅ 공격 판정 비활성화

            // ✅ 공격 후 즉시 후퇴 상태로 전환 (Idle 거치지 않음)
            stateMachine.ChangeState(new RetreatState(ai, stateMachine));
        }

        public void ExitState()
        {
            
        }
    }
}