using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;

[CreateAssetMenu(fileName = "SkillPool", menuName = "Game/Skill Pool")]
public class SkillPoolSO : ScriptableObject
{
    public List<SkillSO> assasinSkill;   
    public List<SkillSO> gladiatorSkill;  
    public List<SkillSO> legionarySkill;    
    public List<SkillSO> magicianSkill;   
    public List<SkillSO> priestSkill;  
    public List<SkillSO> skimisherSkill;  

    List<SkillSO> GetPool(string unitClass) => unitClass switch
    {
        "암살자" => assasinSkill,
        "검투사" => gladiatorSkill,
        "군단병" => legionarySkill,
        "주술사" => magicianSkill,
        "사제" => priestSkill,
        "척후병" => skimisherSkill,
        _ => null,
    };

    // 3성 → [0,1], 4성 → [2,3]
    public List<SkillSO> GetSkillChoices(string unitClass, int rarity)
    {
        var result = new List<SkillSO>();
        var pool = GetPool(unitClass);
        if (pool == null) return result;

        if (rarity == 3 && pool.Count >= 2) { result.Add(pool[0]); result.Add(pool[1]); }
        else if (rarity == 4 && pool.Count >= 4) { result.Add(pool[2]); result.Add(pool[3]); }
        return result;
    }

    // 5성 → [4]
    public SkillSO GetUltimateSkill(string unitClass)
    {
        var pool = GetPool(unitClass);
        return (pool != null && pool.Count >= 5) ? pool[4] : null;
    }
}