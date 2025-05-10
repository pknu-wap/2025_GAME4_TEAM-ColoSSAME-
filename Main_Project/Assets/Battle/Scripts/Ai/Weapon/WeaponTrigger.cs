using System.Collections.Generic;
using Battle.Scripts.Ai.State;
using UnityEngine;

namespace Battle.Scripts.Ai.Weapon
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class WeaponTrigger : MonoBehaviour
    {
        private BattleAI ownerAI;
        private CircleCollider2D weaponCollider;
        private HashSet<BattleAI> alreadyHit = new HashSet<BattleAI>();

        void Awake()
        {
            weaponCollider = GetComponent<CircleCollider2D>();
            weaponCollider.enabled = false;
            weaponCollider.isTrigger = true;
            weaponCollider.radius = 0.01f;
        }

        public void Initialize(BattleAI ai)
        {
            ownerAI = ai;
            //weaponCollider.radius = ownerAI.attackRange + 0.5f;
        }

        public void ActivateCollider()
        {
            weaponCollider.enabled = true;
            transform.position = ownerAI.CurrentTarget.transform.position;
            ResetHitTargets();
        }

        public void DeactivateCollider()
        {
            weaponCollider.enabled = false;
        }

        public void ResetHitTargets()
        {
            alreadyHit.Clear();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BattleAI targetAI = other.GetComponent<BattleAI>();
            if (targetAI == null || targetAI.team == ownerAI.team) return;
            if (alreadyHit.Contains(targetAI)) return;

            alreadyHit.Add(targetAI);

            // 피격 처리: 데미지 전달
            targetAI.StateMachine.ChangeState(new DamageState(targetAI, ownerAI.damage));
        }
    }
}