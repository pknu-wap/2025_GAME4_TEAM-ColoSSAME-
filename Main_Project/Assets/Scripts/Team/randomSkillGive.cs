    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using BattleK.Scripts.AI.Skill.Base;

    public class RandomSkillGive : MonoBehaviour
    {
        public List<SkillSO> tankSkill;
        public List<SkillSO> archerSkill;
        public List<SkillSO> mageSkill;
        public List<SkillSO> swordSkill;
        public List<SkillSO> thiefSkill;
        public List<SkillSO> bufferSkill;

        public List<SkillSO> GetRandomSkills(string unitClass, int rarity)
        {
            List<SkillSO> result = new List<SkillSO>();

            if (rarity < 4)
                return result;

            List<SkillSO> pool = GetClassSkillPool(unitClass);

            if (pool == null || pool.Count == 0)
                return result;

            int skillCount = 2;

            for (int i = 0; i < skillCount; i++)
            {
                SkillSO skill = pool[Random.Range(0, pool.Count)];
                result.Add(skill);
            }

            return result;
        }

        private List<SkillSO> GetClassSkillPool(string unitClass)
        {
            switch (unitClass)
            {
                case "군단병":
                    return tankSkill;

                case "척후병":
                    return archerSkill;

                case "주술사":
                    return mageSkill;

                case "검투사":
                    return swordSkill;

                case "암살자":
                    return thiefSkill;
                case "사제":
                    return bufferSkill;
            }

            return null;
        }
    }

