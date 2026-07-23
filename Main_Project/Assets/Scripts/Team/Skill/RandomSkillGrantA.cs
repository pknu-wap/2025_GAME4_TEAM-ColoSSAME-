using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;
//randomskillgrant 수정본

public class RandomSkillGrantA : MonoBehaviour
{
    public List<SkillSO> tankSkill;
    public List<SkillSO> archerSkill;
    public List<SkillSO> mageSkill;
    public List<SkillSO> swordSkill;
    public List<SkillSO> thiefSkill;
    public List<SkillSO> bufferSkill;

    // 3성 / 4성 선택지 
    public List<SkillSO> GetSkillChoices(string unitClass, int rarity)
    {
        List<SkillSO> result = new();
        List<SkillSO> pool = GetClassSkillPool(unitClass);

        if (pool == null)
            return result;

        if (rarity == 3 && pool.Count >= 2)
        {
            result.Add(pool[0]);
            result.Add(pool[1]);
        }

        else if (rarity == 4 && pool.Count >= 4)
        {
            result.Add(pool[2]);
            result.Add(pool[3]);
        }

        return result;
    }

    public SkillSO GetUltimateSkill(string unitClass)
    {
        List<SkillSO> pool = GetClassSkillPool(unitClass);

        if (pool != null && pool.Count >= 5)
            return pool[4];

        return null;
    }

    private List<SkillSO> GetClassSkillPool(string unitClass)
    {
        switch (unitClass)
        {
            case "군단병": return tankSkill;
            case "척후병": return archerSkill;
            case "주술사": return mageSkill;
            case "검투사": return swordSkill;
            case "암살자": return thiefSkill;
            case "사제": return bufferSkill;
        }

        return null;
    }
}