using UnityEngine;

[System.Serializable]
public class UnitSkillDelay
{
    public float attackCooldown = 1.5f; // 공격 쿨타임 (초)
    private float lastAttackTime = -Mathf.Infinity;

    public bool IsReady()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void Use()
    {
        lastAttackTime = Time.time;
    }
}