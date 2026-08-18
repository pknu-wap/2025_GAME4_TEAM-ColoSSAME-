using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;

[CreateAssetMenu(fileName = "ClassSkillPool", menuName = "Game/Class Skill Pool")]
public class ClassSkillPoolSO : ScriptableObject
{
    public string unitClass;          // 이 풀이 담당하는 직업 
    public List<SkillSO> skills;      // [0,1]=3성, [2,3]=4성, [4]=궁극기

    public List<SkillSO> GetSkillChoices(int rarity)
    {
        var result = new List<SkillSO>();
        if (rarity == 3 && skills.Count >= 2) { result.Add(skills[0]); result.Add(skills[1]); }
        else if (rarity == 4 && skills.Count >= 4) { result.Add(skills[2]); result.Add(skills[3]); }
        return result;
    }

    public SkillSO GetUltimate() => skills.Count >= 5 ? skills[4] : null;
}