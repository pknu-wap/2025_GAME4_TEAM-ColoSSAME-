using System.Collections.Generic;
using Battle.Scripts.Class.Close.Warrior.State;
using UnityEngine;

namespace Battle.Scripts.Class.Close.Warrior.Weapon
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class WeaponTrigger : MonoBehaviour
    {
        private WarriorAI ownerAI;
        private CircleCollider2D weaponCollider;
        private HashSet<WarriorAI> alreadyHit = new HashSet<WarriorAI>();

        void Awake()
        {
            weaponCollider = GetComponent<CircleCollider2D>();
            weaponCollider.enabled = false;
            weaponCollider.isTrigger = true;
        }

        public void Initialize(WarriorAI ai)
        {
            ownerAI = ai;
            weaponCollider.radius = ownerAI.attackRange + 0.5f;
        }

        public void ActivateCollider()
        {
            weaponCollider.enabled = true;
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
            WarriorAI targetAI = other.GetComponent<WarriorAI>();
            if (targetAI == null || targetAI.team == ownerAI.team) return;
            if (alreadyHit.Contains(targetAI)) return;

            alreadyHit.Add(targetAI);

            Debug.Log($"{ownerAI.name} → {targetAI.name} 공격!");

            // 피격 처리: 데미지 전달
            targetAI.StateMachine.ChangeState(new WarriorDamageState(targetAI, ownerAI.damage));
        }
    }
}