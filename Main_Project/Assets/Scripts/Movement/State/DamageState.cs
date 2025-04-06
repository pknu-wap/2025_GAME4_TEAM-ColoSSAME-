using System.Collections;
using UnityEngine;

namespace Movement.State
{
    public class DamageState : IState
    {
        private WeaponTrigger weapondamage;
        private BattleAI2 ai;
        private CharacterValue CharacterValue;
        private StateMachine stateMachine;
        private float damage;

        public DamageState(BattleAI2 ai, StateMachine stateMachine, float damage)
        {
            this.ai = ai;
            this.stateMachine = stateMachine;
            this.CharacterValue = ai.characterValue;
            this.damage = damage;
        }

        public void EnterState()
        {
            ai.StopMoving();
            ai.GetCharAnimator().Idle(); // 피격 애니메이션 (없다면 Idle로 대체)
            Debug.Log("공격받았습니다.");
            Debug.Log(ai);
            CharacterValue.TakeDamage(damage);
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