using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AchievementStage
{
    // UI 표시용 제목
    public string title;

    // UI 표시용 설명
    [TextArea]
    public string description;

    // 보상 골드
    public int rewardGold;

    // 이 단계의 조건들 (기본 AND)
    public List<AchievementCondition> conditions = new List<AchievementCondition>();
}