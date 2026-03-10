using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "BattleK/Skill/GeneralSkill")]
    public class SkillSO : ScriptableObject
    {
        public enum SpawnPosition { Owner, Target }
        
        [Header("Basic Settings")]
        public string SkillName;
        public int InternalPriority;
    
        [Header("Skill Prefab Settings")]
        public GameObject SkillPrefab;
        public float SkillAnimDuration;
        
        [Header("Combat Config")]
        public float Cooldown;
        public Vector2 SkillArea;
        public SpawnPosition SpawnAt;
        
        [Header("Timing Settings")]
        public float WindupTime;    // 선딜레이
        public float ActiveTime;    // 실행유지 시간
        public float RecoveryTime;  // 후딜레이
        
        [Header("Skill Logics")]
        [SerializeReference, SelectableReference]
        public List<ISkillLogic> SkillLogics = new ();

        [Header("Animation Config")]
        public int AnimationIndex;

        public void ExecuteSkill(StaticAICore owner, Transform target)
        {
            if (!SkillPrefab) return;

            var spawnPos = owner.transform.position;
            var spawnRot = owner.transform.rotation;

            spawnPos = SpawnAt switch
            {
                SpawnPosition.Owner => owner.transform.position,
                SpawnPosition.Target when target => target.position,
                _ => spawnPos
            };

            var instance = Instantiate(SkillPrefab, spawnPos, spawnRot);
            
            if (!instance.TryGetComponent(out LogicProcessor processor)) return;
            processor.Initialize(owner, SkillLogics, ActiveTime);
            
            processor.StartProcess();
        }
        
        public virtual IEnumerator ExecuteSkillRoutine(StaticAICore owner, Transform target)
        {
            yield return new WaitForSeconds(WindupTime);
            ExecuteSkill(owner, target);
            yield return new WaitForSeconds(ActiveTime);
            yield return new WaitForSeconds(RecoveryTime);
        }
        
        public virtual bool IsInArea(Transform owner, Transform target)
        {
            if (!target) return false;
            
            var relativePos = owner.InverseTransformPoint(target.position);
            
            var inSide = Mathf.Abs(relativePos.x) <= SkillArea.x * 0.5f;
            var inFront = relativePos.y >= 0 && relativePos.y <= SkillArea.y;

            return inSide && inFront;
        }
        
        public virtual void DrawGizmos(Transform owner) { }
    }
}