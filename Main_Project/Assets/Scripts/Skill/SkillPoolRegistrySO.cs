using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillPoolRegistry", menuName = "Game/Skill Pool Registry")]
public class SkillPoolRegistrySO : ScriptableObject
{
    public List<ClassSkillPoolSO> pools;   

    public ClassSkillPoolSO GetPool(string unitClass)
    {
        foreach (var p in pools)
            if (p != null && p.unitClass == unitClass) return p;
        return null;
    }
}