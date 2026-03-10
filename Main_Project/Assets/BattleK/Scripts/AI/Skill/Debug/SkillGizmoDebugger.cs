using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Debug
{
    public class SkillGizmoDebugger : MonoBehaviour
    {
        [SerializeField] private List<SkillSO> _skillSOList = new();
        private void OnDrawGizmos()
        {
            foreach (var skill in _skillSOList)
            {
                if (skill) skill.DrawGizmos(transform);
            }
        }
    }
}
