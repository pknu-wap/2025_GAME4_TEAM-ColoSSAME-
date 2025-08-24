using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager : MonoBehaviour
{
    private Dictionary<int, float> cooldownTimers = new();

    public bool IsCooldownReady(int skillID)
    {
        return !cooldownTimers.ContainsKey(skillID) || Time.time >= cooldownTimers[skillID];
    }

    public void SetCooldown(int skillID, float cooldown)
    {
        cooldownTimers[skillID] = Time.time + cooldown;
    }
}