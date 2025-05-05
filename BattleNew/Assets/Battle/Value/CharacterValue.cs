using Battle.Scripts.Class.Close.Warrior;
using UnityEngine;

namespace Battle.Value
{
    public class CharacterValue : MonoBehaviour
    {
        private WarriorAI warriorAI;
        public float maxHp = 100;
        public float currentHp = 100;

        public HealthBar healthBar;

        public void StartSetting()
        {
            warriorAI = GetComponent<WarriorAI>();
            healthBar = GetComponentInChildren<HealthBar>();
            maxHp = warriorAI.hp;
            currentHp = maxHp;
            healthBar.SetHealth(currentHp, maxHp);
        }
        
        public void TakeDamage(float dmg)
        {
            currentHp -= dmg;
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
            healthBar.SetHealth(currentHp, maxHp);
        }
    }
}