using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior.State
{
    public class WarriorIdleState : IState
    {
        private WarriorAI ai;

        public WarriorIdleState(WarriorAI ai)
        {
            this.ai = ai;
        }

        public void EnterState()
        {
            ai.StopMoving();
            ai.warriorAnimator.Reset();
        }

        public void UpdateState()
        {
            if (ai.HasEnemyInSight())
            {
                if (ai.IsInAttackRange() && ai.CanAttack())
                {
                    ai.StateMachine.ChangeState(new WarriorAttackState(ai));
                    ai.rb.velocity = Vector2.zero;
                }

                ai.StateMachine.ChangeState(new WarriorChaseState(ai));
            }
        }

        public void ExitState() { }
    }
}