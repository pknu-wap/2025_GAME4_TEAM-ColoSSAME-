using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AchievementCondition
{
    // 조건 종류(어떤 방식으로 검사할지)
    public AchievementConditionType type;
    
    public int targetStar; // 유닛 성급 관련

    public int itemId; // item id
    public int targetCount; // item 사용 횟수
}