using System.Collections;
using UnityEngine;

namespace Movement.State
{
    public class DamageState : IState
    {
        private BattleAI2 ai;
        private CharacterValue CharacterValue;
        private StateMachine stateMachine;
        private float damageDuration = 0.5f;

        public DamageState(BattleAI2 ai, StateMachine stateMachine)
        {
            this.ai = ai;
            this.stateMachine = stateMachine;
            this.CharacterValue = ai.characterValue;
        }

        public void EnterState()
        {
            ai.StopMoving();
            ai.GetCharAnimator().Idle(); // 피격 애니메이션 (없다면 Idle로 대체)
            Debug.Log("공격받았습니다.");
            Debug.Log(ai);
            CharacterValue.TakeDamage(10);
            Debug.Log(CharacterValue.currentHp);
            if (CharacterValue.currentHp <= 0)
            {
                stateMachine.ChangeState(new DeathState(ai, stateMachine));
            }
            stateMachine.ChangeState(new IdleState(ai, stateMachine));
        }

        public IEnumerator ExecuteState()
        {
            yield return null;
        }

        public void ExitState()
        {
            ai.isTakingDamage = false;
        }
    }
}