using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    public abstract class SkillSO : ScriptableObject
    {
        [Header("Basic Settings")]
        public string SkillName;
        public int InternalPriority;
    
        [Header("Skill Prefab Settings")]
        public GameObject SkillPrefab;
        public float SkillAnimDuration;
        
        [Header("Combat Config")]
        public float Cooldown;
        public Vector2 SkillArea;
        
        [Header("Timing Settings")]
        public float WindupTime;
        public float ActiveTime;
        public float RecoveryTime;
        
        [SerializeReference, SelectableReference]
        public List<ISkillLogic> SkillLogics = new ();
        
        public enum SpawnPosition { Owner, Target }
        public SpawnPosition SpawnAt;

        [Header("Animation Config")]
        public int AnimationIndex;

        public abstract void ExecuteSkill(StaticAICore owner, Transform target);
        
        public virtual IEnumerator ExecuteSkillRoutine(StaticAICore owner, Transform target)
        {
            yield return new WaitForSeconds(WindupTime);
            ExecuteSkill(owner, target);
            yield return new WaitForSeconds(ActiveTime);
            yield return new WaitForSeconds(RecoveryTime);
        }
        public abstract bool IsInArea(Transform owner, Transform target);
        
        public virtual void DrawGizmos(Transform owner) { }
    }
}