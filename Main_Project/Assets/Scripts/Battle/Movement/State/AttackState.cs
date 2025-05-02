using System.Collections;
using UnityEngine;

namespace Battle.Movement.State
{
    public class AttackState : IState
    {
        private BattleAI2 ai;
        private StateMachine stateMachine;
        private Transform target;

        public AttackState(BattleAI2 ai, StateMachine stateMachine, Transform target)  //변수 참조
        {
            this.ai = ai;
            this.stateMachine = stateMachine;
            this.target = target;
        }

        public void EnterState()  //상태 시작
        {
            ai.StopMoving(); // ✅ 이동 정지
            ai.GetCharAnimator().Attack(); //공격 애니메이션 실행
            ai.EnableWeaponCollider(); //공격 판정 활성화
        }

        public IEnumerator ExecuteState()
        {
            
            yield return new WaitForSeconds(0.3f); // 공격 애니메이션 딜레이

            ai.DisableWeaponCollider(); //공격 판정 비활성화(중복 판정 예방)

            //공격 후 즉시 후퇴 상태로 전환 (Idle 거치지 않음)-임시
            stateMachine.ChangeState(new RetreatState(ai, stateMachine));
        }

        public void ExitState()  //상태 종료
        {
            
        }
    }
}