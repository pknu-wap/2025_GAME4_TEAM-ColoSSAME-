using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AchievementGroup
{
    // 저장키로 쓰는 ID (절대 변경 금지)
    public string groupId;

    // UI 표시용 그룹명
    public string displayName;

    // UI 설명
    [TextArea]
    public string groupDescription;

    // 단계들 (0=1단계)
    public List<AchievementStage> stages = new List<AchievementStage>();
}