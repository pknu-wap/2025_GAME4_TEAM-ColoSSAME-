using UnityEngine;
using System.Collections.Generic;
using Movement.State;

public class WeaponTrigger : MonoBehaviour
{
    private BattleAI2 ai;

    private HashSet<GameObject> alreadyHitTargets = new HashSet<GameObject>();

    public void Initialize(BattleAI2 ownerAI)
    {
        ai = ownerAI;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & ai.enemyLayer) != 0)
        {
            if (alreadyHitTargets.Contains(other.gameObject)) return;

            alreadyHitTargets.Add(other.gameObject);

            BattleAI2 enemy = other.GetComponent<BattleAI2>();
            if (enemy != null)
            {
                enemy.TakeDamage(ai.damage);
            }
        }
    }

    // ✅ 여기가 바로 필요한 함수!
    public void ResetHitTargets()
    {
        alreadyHitTargets.Clear();
    }
}