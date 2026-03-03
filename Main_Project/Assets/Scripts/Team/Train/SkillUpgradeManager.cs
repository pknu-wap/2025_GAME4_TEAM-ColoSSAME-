using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUpgradeManager : MonoBehaviour
{
    //유닛별 스킬 레벨 저장
    private Dictionary<string, Dictionary<int, int>> skillLevels = new Dictionary<string, Dictionary<int, int>>();

    //스킬 강화
    public void UpgradeSkill(string unitId, int skillIndex)
    {
        if (!skillLevels.ContainsKey(unitId))
            skillLevels[unitId] = new Dictionary<int, int>();

        if (!skillLevels[unitId].ContainsKey(skillIndex))
            skillLevels[unitId][skillIndex] = 0;

        skillLevels[unitId][skillIndex]++;

        Debug.Log($"✅ {unitId} {skillIndex}스킬 Lv.{skillLevels[unitId][skillIndex]}");
    }

    public int GetSkillLevel(string unitId, int skillIndex)
    {
        if (skillLevels.ContainsKey(unitId) &&
            skillLevels[unitId].ContainsKey(skillIndex))
        {
            return skillLevels[unitId][skillIndex];
        }

        return 0;
    }

}