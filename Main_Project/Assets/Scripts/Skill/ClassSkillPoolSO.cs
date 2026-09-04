using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.ClassInfo;

[CreateAssetMenu(fileName = "ClassSkillPool", menuName = "Game/Class Skill Pool")]
public class ClassSkillPoolSO : ScriptableObject
{
    public UnitClass unitClass; 
    public List<SkillSO> skills;

    public List<SkillSO> GetSkillChoices(int rarity)
    {
        var result = new List<SkillSO>();
        if (rarity == 3 && skills.Count >= 2) { result.Add(skills[0]); result.Add(skills[1]); }
        else if (rarity == 4 && skills.Count >= 4) { result.Add(skills[2]); result.Add(skills[3]); }
        return result;
    }

    public SkillSO GetUltimate() => skills.Count >= 5 ? skills[4] : null;
}