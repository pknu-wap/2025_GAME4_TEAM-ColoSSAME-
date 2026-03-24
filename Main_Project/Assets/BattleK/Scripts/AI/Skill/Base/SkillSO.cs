using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.AI.Skill.Base.Projectile;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "BattleK/Skill/GeneralSkill")]
    public sealed class SkillSO : ScriptableObject
    {
        public enum SpawnPosition { Owner, Target }
        public enum TargetingType { Enemy, Ally, None }
        
        [Header("Targeting Settings")]
        public TargetingType TargetType;
        [SerializeReference, SelectableReference]
        public List<IConditionLogic> ExecutionCondition = new();
        
        [Header("Basic Settings")]
        public string SkillName;
        public int InternalPriority;
    
        [Header("Skill Prefab Settings")]
        public GameObject SkillPrefab;
        
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
            var finalPos = (target) ? target.position : owner.transform.position;
            var spawnRot = owner.transform.rotation;
            var spawnPos = SpawnAt switch
            {
                SpawnPosition.Owner => owner.transform.position,
                SpawnPosition.Target when target => finalPos,
                _ => owner.transform.position
            };

            var instance = Instantiate(SkillPrefab, spawnPos, spawnRot);
            var processors = instance.GetComponents<LogicProcessor>();
            
            //임시-----------------------
            var movement = instance.GetComponent<ProjectileMovement>();
            if (movement != null)
            {
                movement.Init(owner.transform.right);
            }
            //임시----------------------

       
            LayerMask targetMask = TargetType switch
            {
                TargetingType.Enemy => owner.TargetLayer,
                TargetingType.Ally => (LayerMask)(1 << owner.gameObject.layer),
                _ => 0
            };
            
            foreach (var p in processors)
            {
                p.Initialize(owner, SkillLogics, ActiveTime, targetMask, target, spawnPos);
                p.StartProcess();
            }
        }
        
        public IEnumerator ExecuteSkillRoutine(StaticAICore owner, Transform target)
        {
            yield return new WaitForSeconds(WindupTime);
            ExecuteSkill(owner, target);
            yield return new WaitForSeconds(ActiveTime);
            yield return new WaitForSeconds(RecoveryTime);
        }
        
        public bool CanExecute(StaticAICore owner, out Transform foundTarget)
        {
            foundTarget = null;
            
            if (ExecutionCondition == null) return false;

            LayerMask mask = TargetType switch
            {
                TargetingType.Enemy => owner.TargetLayer,
                TargetingType.Ally => owner.gameObject.layer,
                _ => 0
            };
            
            if (TargetType != TargetingType.None)
                return ExecutionCondition[0].Evaluate(owner, mask, SkillArea, out foundTarget);
            foundTarget = owner.transform;
            return true;
        }

        public void DrawGizmos(Transform owner)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(owner.position, new Vector3(SkillArea.x, SkillArea.y, 1));
        }
    }
}