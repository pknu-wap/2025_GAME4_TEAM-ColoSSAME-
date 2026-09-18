using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.Stat;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace BattleK.Scripts.Data.ClassInfo
{
    [System.Serializable]
    public class UnitRuntimeStat
    {
        public string Name;
        public Sprite CharacterImage;

        [Header("부상")]
        public InjuryStatus InjuryLevel;
    
        [Header("클래스")]
        public UnitClass UnitClass;
        public bool IsRanged;

        [Header("시야 범위")]
        public float SightRange;
    
        [Header("스킬")]
        [FormerlySerializedAs("Skills")]
        public List<ClassSkillPoolSO.SkillRef> EquippedSkills = new();
        public ClassSkillPoolSO SkillPoolSo;
        
        [Header("아이템")]
        public ItemData Item;
    
        [Header("능력치")]
        public int MaxHP;
        public int CurrentHP;
        public int AttackDamage;
        public int SkillPoint;
        public float AttackSpeed;
        public float AttackRange;
        public float AttackDelay;
        public int Defense;
        public float MoveSpeed;
        public float EvasionRate;
        
        public void LoadEquipped(List<UnitSkill> savedSkills)
        {
            if (SkillPoolSo == null || SkillPoolSo.skills.Count == 0)
            {
                EquippedSkills = new List<ClassSkillPoolSO.SkillRef>();
                return;
            }

            var savedNames = new HashSet<string>((savedSkills ?? Enumerable.Empty<UnitSkill>())
                .Where(u => u != null).Select(u => u.skillName));
            EquippedSkills = SkillPoolSo.skills
                .Where(s => s != null && savedNames.Contains(s.skillName)).ToList();
        }
        
        public async Task<List<SkillSO>> ResolveEquippedSkillsAsync()
        {
            var result = new List<SkillSO>();
            if (EquippedSkills == null || EquippedSkills.Count == 0) return result;

            foreach (var skillRef in EquippedSkills)
            {
                if (skillRef?.asset == null || !skillRef.asset.RuntimeKeyIsValid()) continue;

                AsyncOperationHandle<SkillSO> handle = skillRef.asset.LoadAssetAsync();
                var skill = await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded && skill != null)
                {
                    result.Add(skill);
                }
                else
                {
                    Debug.LogWarning($"[UnitRuntimeStat] 스킬 로드 실패: {skillRef.skillName}");
                }
            }

            return result;
        }
        
        public void ReleaseEquippedSkills()
        {
            if (EquippedSkills == null) return;
            foreach (var skillRef in EquippedSkills)
            {
                if (skillRef?.asset != null && skillRef.asset.IsValid())
                {
                    skillRef.asset.ReleaseAsset();
                }
            }
        }
        
        public void SaveTo(Unit unit)
        {
            unit.currentInjury = InjuryLevel;
            unit.equippedItemId = Item != null ? Item.id : -1;
            unit.EquippedSkills = EquippedSkills?.Where(s => s != null).Select(s => new UnitSkill(s.skillName, 1)).ToList() ?? new List<UnitSkill>();
        }

        public void LoadFrom(Unit unit, ItemDatabase itemDb)
        {
            if (unit == null) return;
            InjuryLevel = unit.currentInjury;
            Item = itemDb?.GetById(unit.equippedItemId);
            LoadEquipped(unit.EquippedSkills);
        }
    }
}