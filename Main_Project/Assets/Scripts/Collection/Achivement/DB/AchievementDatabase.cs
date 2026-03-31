using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Achievement/Achievement Database", fileName = "AchievementDatabase")]
public class AchievementDatabase : ScriptableObject
{
    // 모든 도감 그룹 목록
    public List<AchievementGroup> groups = new List<AchievementGroup>();

    /// <summary>
    /// groupId로 도감 그룹을 찾아 반환
    /// </summary>
    public AchievementGroup GetGroup(string groupId)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i].groupId == groupId)
                return groups[i];
        }
        return null;
    }
}