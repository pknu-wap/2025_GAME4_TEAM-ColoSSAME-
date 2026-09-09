using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Stat;

//randomskillgrant 수정본

public class RandomSkillGrantA : MonoBehaviour
{
    [SerializeField] private List<SkillSO> tankSkill;
    [SerializeField] private List<SkillSO> archerSkill;
    [SerializeField] private List<SkillSO> mageSkill;
    [SerializeField] private List<SkillSO> swordSkill;
    [SerializeField] private List<SkillSO> thiefSkill;
    [SerializeField] private List<SkillSO> bufferSkill;

    private Dictionary<UnitClass, List<SkillSO>> skillPools;

    private void Awake()
    {
        skillPools = new Dictionary<UnitClass, List<SkillSO>>
        {
            { UnitClass.Shieldman, tankSkill },
            { UnitClass.Archer, archerSkill },
            { UnitClass.Mage, mageSkill },
            { UnitClass.Swordsman, swordSkill },
            { UnitClass.Thief, thiefSkill },
            { UnitClass.Priest, bufferSkill }
        };
    }

    public List<SkillSO> GetAllSkills(UnitClass unitClass)
    {
        if (skillPools.TryGetValue(unitClass, out List<SkillSO> pool))
        {
            return pool;
        }

        return new List<SkillSO>();
    }

    // 3성 / 4성 선택지
    public List<SkillSO> GetSkillChoices(UnitClass unitClass, int rarity)
    {
        List<SkillSO> result = new();

        if (!skillPools.TryGetValue(unitClass, out List<SkillSO> pool))
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

    public SkillSO GetUltimateSkill(UnitClass unitClass)
    {
        if (skillPools.TryGetValue(unitClass, out List<SkillSO> pool) &&
            pool.Count >= 5)
        {
            return pool[4];
        }

        return null;
    }
}