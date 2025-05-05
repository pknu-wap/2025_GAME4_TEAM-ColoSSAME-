using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior
{
    public class WarriorAnimator : MonoBehaviour
    {
        private static readonly int Moving = Animator.StringToHash("1_Move");
        private static readonly int Attacking = Animator.StringToHash("2_Attack");
        private static readonly int Damage = Animator.StringToHash("3_Damaged");
        private static readonly int Death = Animator.StringToHash("4_Death");
        private static readonly int isDead = Animator.StringToHash("isDeath");
        private static readonly int NormalState = Animator.StringToHash("NormalState");
        private static readonly int SkillState = Animator.StringToHash("SkillState");

        private Animator animator;

        public WeaponType CurrentWeapon { get; set; } = WeaponType.Sword;
        private WarriorAI warriorAI;

        public void Awake()
        {
            animator = GetComponent<Animator>();
            Reset();
        }

        public void Reset()
        {
            animator.SetBool(isDead, false);
            animator.SetBool(Moving, false);
            animator.ResetTrigger(Attacking);
            animator.ResetTrigger(Damage);
            animator.ResetTrigger(Death);
        }

        public void ChooseWeapon()
        {
            Debug.Log(NormalState);
            if (animator == null)
                animator = GetComponent<Animator>();

            if (warriorAI == null)
                warriorAI = GetComponentInParent<WarriorAI>();

            if (warriorAI == null)
            {
                Debug.LogWarning("WarriorAI 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            CurrentWeapon = warriorAI.weaponType;
            float weaponValue = (float)CurrentWeapon/100f;
            animator.SetFloat(NormalState, weaponValue);
        }

    
        public void Move()
        {
            animator.SetBool(Moving, true);
        }

        public void StopMove()
        {
            animator.SetBool(Moving, false);
        }

        public void Attack()
        {
            Reset();
            ChooseWeapon();
            animator.SetTrigger(Attacking);
        }

        public void Damaged()
        {
            animator.SetTrigger(Damage);
        }

        public void Dead()
        {
            animator.SetTrigger(Death);
            animator.SetBool(isDead, true);
        }
    }
}