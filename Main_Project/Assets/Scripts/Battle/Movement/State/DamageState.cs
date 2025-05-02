using System.Collections;
using UnityEngine;

namespace Battle.Movement.State
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
            ai.GetCharAnimator().Idle(); // 피격 애니메이션
            Debug.Log("공격받았습니다.");  // 피격 판정 확인
            Debug.Log(ai);  //데미지 입은 캐릭터 확인
            CharacterValue.TakeDamage(damage);  //데미지 계산 호출
            Debug.Log(CharacterValue.currentHp);  //현재 hp 확인
            if (CharacterValue.currentHp <= 0)  //사망 판정
            {
                stateMachine.ChangeState(new DeathState(ai));  //사망 상태 호출
            }
        }

        public IEnumerator ExecuteState()  //문제가 되는 부분
        {
            Debug.Log("공격받았습니다.");  // 피격 판정 확인
            Debug.Log(ai);  //데미지 입은 캐릭터 확인
            CharacterValue.TakeDamage(damage);  //데미지 계산 호출
            Debug.Log(CharacterValue.currentHp);  //현재 hp 확인
            if (CharacterValue.currentHp <= 0)  //사망 판정
            {
                stateMachine.ChangeState(new DeathState(ai));  //사망 상태 호출
            }
            yield return null;
        }

        public void ExitState()  //상태 탈출
        {
            ai.isTakingDamage = false;
        }
    }
}