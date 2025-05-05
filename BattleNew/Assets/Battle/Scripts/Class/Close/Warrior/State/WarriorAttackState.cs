using System.Collections;
using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior.State
{
    public class WarriorAttackState : IState
    {
        private WarriorAI ai;
        private bool isFinished;

        public WarriorAttackState(WarriorAI ai) { this.ai = ai; }

        public void EnterState()
        {
            ai.StopMoving();
            ai.rb.velocity = Vector2.zero;
            ai.warriorAnimator.StopMove();
            ai.warriorAnimator.Reset();
            ai.warriorAnimator.Attack();
            Debug.Log($"{ai.name} AttackDelay: {ai.AttackDelay}");
            
            ai.UseSkill(); // 스킬은 내부적으로 common/rare/epic 중 발동
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
                ai.StateMachine.ChangeState(new WarriorIdleState(ai));
                Debug.Log("공격 종료");
            }
        }

        public void ExitState()
        {
            ai.StopMoving();
        }
    }
}