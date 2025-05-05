using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior.State
{
    public class WarriorChaseState : IState
    {
        private WarriorAI ai;

        public WarriorChaseState(WarriorAI ai) { this.ai = ai; }

        public void EnterState()
        {
            ai.rb.WakeUp();
            ai.warriorAnimator.Move();
        }

        public void UpdateState()
        {
            if (ai.IsInAttackRange() && ai.CanAttack())
                ai.StateMachine.ChangeState(new WarriorAttackState(ai));
            ai.MoveTo(ai.CurrentTarget.position);
        }

        public void ExitState() => ai.StopMoving();
    }
}