using System.Collections.Generic;
using BattleK.Scripts.Data.ClassInfo;
using UnityEngine;

namespace BattleK.Scripts.CharacterCreator
{
    [CreateAssetMenu(fileName = "ClassDefinition", menuName = "BattleK/Class Definition")]
    public class ClassDefinitionSO : ScriptableObject
    {
        [Header("식별")]
        public UnitClass UnitClass;
        public AttackType AttackType;
        public bool isRecruit;

        [Header("전투 기본값")]
        public bool IsRangedDefault;
        public float AttackRange = 0.9f;
        public float MoveSpeed = 2f;
        public float SightRange = 9f;

        [Header("스킬")]
        [Tooltip("이 직업이 공통으로 사용하는 스킬 풀 (3성/4성/궁극기 슬롯 포함)")]
        public ClassSkillPoolSO CommonSkillPool;

        [Header("전용기 슬롯 정의 (선택)")]
        [Tooltip("직업 자체에 귀속되지 않고 유닛 개체별로 다르게 붙는 전용기가 있다면, 최대 개수 등 제약만 여기 둘 수 있음")]
        public int MaxUniqueSkillSlots = 1;

        public int AttackAnimationIndex => AttackType switch
        {
            AttackType.Archer => 2,
            AttackType.Mage or AttackType.Priest => 4,
            AttackType.Axeman => 5,
            AttackType.Spearman => 6,
            AttackType.Thief => 7,
            _ => 0
        };
        
        public List<ClassSkillPoolSO.SkillRef> GetCommonSkillChoices(int rarity)
        {
            return CommonSkillPool != null
                ? CommonSkillPool.GetSkillChoices(rarity)
                : new List<ClassSkillPoolSO.SkillRef>();
        }

        public ClassSkillPoolSO.SkillRef GetUltimate()
        {
            return CommonSkillPool != null ? CommonSkillPool.GetUltimate() : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (CommonSkillPool != null && CommonSkillPool.unitClass != UnitClass)
            {
                Debug.LogWarning($"[ClassDefinitionSO:{name}] CommonSkillPool의 클래스({CommonSkillPool.unitClass})가 " +
                                 $"이 정의의 클래스({UnitClass})와 일치하지 않습니다.");
            }
        }
#endif
    }
}