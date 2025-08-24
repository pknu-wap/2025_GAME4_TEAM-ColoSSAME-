using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/SkillDatabase")]
public class SkillDatabase : ScriptableObject
{
    public List<SkillData> allSkills;

    private Dictionary<UnitClass, List<SkillData>> cache;

    public void Init()
    {
        cache = new();
        foreach (var skill in allSkills)
        {
            if (!cache.ContainsKey(skill.unitClass))
                cache[skill.unitClass] = new List<SkillData>();
            cache[skill.unitClass].Add(skill);
        }
    }

    public SkillData GetSkill(UnitClass unitClass, int index)
    {
        if (cache == null) Init();
        return cache.TryGetValue(unitClass, out var list) && index < list.Count ? list[index] : null;
    }
}