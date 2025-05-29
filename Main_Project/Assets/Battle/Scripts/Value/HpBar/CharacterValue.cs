using System;
using Battle.Scripts.Ai;
using UnityEngine;

namespace Battle.Scripts.Value.HpBar
{
    public class CharacterValue : MonoBehaviour
    {
        private BattleAI warriorAI;
        public float maxHp = 100;
        public float currentHp = 100;

        public HealthBar healthBar;

        public void StartSetting()
        {
            warriorAI = GetComponent<BattleAI>();
            healthBar = GetComponentInChildren<HealthBar>();
            maxHp = warriorAI.hp;
            currentHp = maxHp;
            healthBar.SetHealth(currentHp, maxHp);
        }
        
        public void TakeDamage(float dmg)
        {
            currentHp -= dmg;
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
            currentHp = (float)(Math.Round(currentHp * 10f) / 10f);
            healthBar.SetHealth(currentHp, maxHp);
        }
    }
}