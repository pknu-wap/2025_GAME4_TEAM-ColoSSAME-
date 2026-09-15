using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Data.ClassInfo;

[CreateAssetMenu(fileName = "ClassSkillPool", menuName = "Game/Class Skill Pool")]
public class ClassSkillPoolSO : ScriptableObject
{
    [System.Serializable]
    public class SkillRef
    {
        public string skillName;                // 부여 시 사용 
        public AssetReferenceT<SkillSO> asset;  // 필요할 때만 온디맨드 로드용
    }

    public UnitClass unitClass;
    public List<SkillRef> skills;   // [0,1]=3성, [2,3]=4성, [4]=궁극기

    public List<SkillRef> GetSkillChoices(int rarity)
    {
        var result = new List<SkillRef>();
        if (rarity == 3 && skills.Count >= 2) { result.Add(skills[0]); result.Add(skills[1]); }
        else if (rarity == 4 && skills.Count >= 4) { result.Add(skills[2]); result.Add(skills[3]); }
        return result;
    }

    public SkillRef GetUltimate() => skills.Count >= 5 ? skills[4] : null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (skills == null) return;
        foreach (var s in skills)
        {
            if (s?.asset == null) continue;
            if (s.asset.editorAsset is SkillSO so) s.skillName = so.SkillName;
        }
    }
#endif
}
