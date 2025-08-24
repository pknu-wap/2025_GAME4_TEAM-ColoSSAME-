using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CooldownTimer
{
    // 마지막 사용 시간 저장
    private Dictionary<ActionType, float> lastUseTimes = new();

    // 쿨타임 지속 시간 저장
    private Dictionary<ActionType, float> cooldownDurations = new();

    // 쿨타임 설정 (초 단위)
    public void SetCooldown(ActionType type, float duration)
    {
        cooldownDurations[type] = duration;
    }

    // 쿨타임 시작 시 호출
    public void Use(ActionType type)
    {
        lastUseTimes[type] = Time.time;
    }

    // 사용 가능한지 확인
    public bool IsReady(ActionType type)
    {
        if (!cooldownDurations.ContainsKey(type)) return true; // 쿨타임 설정 안 됨
        if (!lastUseTimes.ContainsKey(type)) return true;       // 처음 사용

        return Time.time >= lastUseTimes[type] + cooldownDurations[type];
    }

    // 남은 시간 구하기 (디버그용)
    public float GetRemaining(ActionType type)
    {
        if (!cooldownDurations.ContainsKey(type) || !lastUseTimes.ContainsKey(type)) return 0;
        return Mathf.Max(0, lastUseTimes[type] + cooldownDurations[type] - Time.time);
    }
}