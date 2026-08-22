using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.AI.Skill.Base.Projectile;
using BattleK.Scripts.Utils;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.Base
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "BattleK/Skill/GeneralSkill")]
    public sealed class SkillSO : ScriptableObject
    {
        public enum SpawnPosition { Owner, Target, OwnerMiddle, OwnerFront }
        public enum TargetingType { Enemy, Ally, None }

        private readonly struct SpriteFadeTarget
        {
            public readonly SpriteRenderer Renderer;
            public readonly Color BaseColor;

            public SpriteFadeTarget(SpriteRenderer renderer, Color baseColor)
            {
                Renderer = renderer;
                BaseColor = baseColor;
            }
        }
        
        [Header("Targeting Settings")]
        public TargetingType TargetType;
        [SerializeReference, SelectableReference]
        public List<IConditionLogic> ExecutionCondition = new();
        
        [Header("Basic Settings")]
        public string SkillName;
        public int InternalPriority;
    
        [Header("Skill Prefab Settings")]
        public GameObject SkillPrefab;
        [Min(0f)] public float SkillPrefabScale = 1f;
        public bool FlipSkillPrefabByOwnerFacing;
        public bool FollowSkillPrefab;
        [Min(1)] public int SkillPrefabSpawnCount = 1;
        [Min(0f)] public float SkillPrefabSpawnInterval;

        [Header("Windup Prefab Settings")]
        public GameObject WindupPrefab;
        public SpawnPosition WindupSpawnAt;
        [Min(0f)] public float WindupPrefabScale = 1f;
        public bool FlipWindupPrefabByOwnerFacing;
        public bool FadeWindupPrefab = true;
        [Min(0f)] public float WindupFadeInTime = 0.15f;
        [Min(0f)] public float WindupFadeOutTime = 0.15f;
        
        [Header("Combat Config")]
        public float Cooldown;
        public Vector2 SkillArea;
        [Min(0)] public int MaxHitTargets;
        [Min(0f)] public float OwnerFrontDistance = 1f;
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
            var activeTime = GetSkillActiveTime();
            var spawnCount = GetSkillPrefabSpawnCount();
            for (var i = 0; i < spawnCount; i++)
            {
                SpawnSkillPrefab(owner, target, activeTime);
            }
        }

        private GameObject SpawnSkillPrefab(StaticAICore owner, Transform target, float activeTime)
        {
            if (!SkillPrefab) return null;
            var spawnRot = owner.transform.rotation;
            var spawnPos = GetSpawnPosition(owner, target, SpawnAt);

            var instance = Instantiate(SkillPrefab, spawnPos, spawnRot);
            ApplySkillPrefabScale(instance);
            var processors = instance.GetComponents<LogicProcessor>();
            
            // 방향 계산
            Vector2 direction = Vector2.zero;
            if (target != null)
            {
                direction = ((Vector2)(target.position - spawnPos)).normalized;
            }

            // Initialize projectile movement.
            var movement = instance.GetComponent<ProjectileMovement>();
            if (movement != null)
            {
                movement.Init(direction);
            }

            ApplyOwnerFacingFlip(instance, owner, FlipSkillPrefabByOwnerFacing);
       
            LayerMask targetMask = TargetType switch
            {
                TargetingType.Enemy => owner.TargetLayer,
                TargetingType.Ally => (LayerMask)(1 << owner.gameObject.layer),
                _ => 0
            };
            
            foreach (var p in processors)
            {
                p.Initialize(owner, SkillLogics, activeTime, targetMask, target, spawnPos, MaxHitTargets);
                p.StartProcess();
            }

            if (processors.Length == 0)
            {
                Destroy(instance, activeTime);
            }

            return instance;
        }
        
        public IEnumerator ExecuteSkillRoutine(StaticAICore owner, Transform target)
        {
            GameObject windupInstance = null;
            List<SpriteFadeTarget> windupFadeTargets = null;

            if (WindupPrefab && WindupTime > 0f)
            {
                var windupPos = GetSpawnPosition(owner, target, WindupSpawnAt);
                windupInstance = Instantiate(WindupPrefab, windupPos, owner.transform.rotation);
                ApplyWindupPrefabScale(windupInstance);
                ApplyOwnerFacingFlip(windupInstance, owner, FlipWindupPrefabByOwnerFacing);

                if (FadeWindupPrefab)
                {
                    windupFadeTargets = CaptureSpriteFadeTargets(windupInstance);
                    SetSpriteFadeAlpha(windupFadeTargets, 0f);
                }
            }

            if (windupInstance)
            {
                var elapsed = 0f;

                while (elapsed < WindupTime)
                {
                    UpdateWindupFollow(windupInstance, owner, target);

                    if (FadeWindupPrefab)
                    {
                        SetSpriteFadeAlpha(windupFadeTargets, CalculateWindupFadeAlpha(elapsed, WindupTime));
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(WindupTime);
            }

            if (windupInstance)
            {
                if (FadeWindupPrefab)
                {
                    SetSpriteFadeAlpha(windupFadeTargets, 0f);
                }

                Destroy(windupInstance);
            }

            yield return SpawnSkillPrefabsRoutine(owner, target);
            yield return new WaitForSeconds(RecoveryTime);
        }

        private IEnumerator SpawnSkillPrefabsRoutine(StaticAICore owner, Transform target)
        {
            var skillInstances = new List<GameObject>();
            var activeTime = GetSkillActiveTime();
            var spawnCount = GetSkillPrefabSpawnCount();
            var spawnInterval = Mathf.Max(0f, SkillPrefabSpawnInterval);

            for (var i = 0; i < spawnCount; i++)
            {
                var skillInstance = SpawnSkillPrefab(owner, target, activeTime);
                if (skillInstance)
                {
                    skillInstances.Add(skillInstance);
                }

                if (i < spawnCount - 1 && spawnInterval > 0f)
                {
                    yield return WaitForSkillPrefabSpawnInterval(spawnInterval, skillInstances, owner, target);
                }
            }

            if (FollowSkillPrefab && skillInstances.Count > 0)
            {
                yield return FollowSkillPrefabsForActiveTime(skillInstances, owner, target, activeTime);
                yield break;
            }

            yield return new WaitForSeconds(activeTime);
        }

        private IEnumerator WaitForSkillPrefabSpawnInterval(float duration, List<GameObject> skillInstances, StaticAICore owner, Transform target)
        {
            if (!FollowSkillPrefab)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                UpdateSkillPrefabFollows(skillInstances, owner, target);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator FollowSkillPrefabsForActiveTime(
            List<GameObject> skillInstances,
            StaticAICore owner,
            Transform target,
            float activeTime)
        {
            var elapsed = 0f;
            while (elapsed < activeTime)
            {
                UpdateSkillPrefabFollows(skillInstances, owner, target);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void UpdateSkillPrefabFollows(List<GameObject> skillInstances, StaticAICore owner, Transform target)
        {
            for (var i = skillInstances.Count - 1; i >= 0; i--)
            {
                var skillInstance = skillInstances[i];
                if (!skillInstance)
                {
                    skillInstances.RemoveAt(i);
                    continue;
                }

                UpdateSkillPrefabFollow(skillInstance, owner, target);
            }
        }

        private int GetSkillPrefabSpawnCount()
        {
            return Mathf.Max(1, SkillPrefabSpawnCount);
        }

        private float GetSkillActiveTime()
        {
            var activeTime = Mathf.Max(0f, ActiveTime);
            if (SkillLogics == null) return activeTime;

            foreach (var logic in SkillLogics)
            {
                if (logic is TauntLogic tauntLogic)
                {
                    activeTime = Mathf.Max(activeTime, tauntLogic.GetDuration());
                }
            }

            return activeTime;
        }
        
        public bool CanExecute(StaticAICore owner, out Transform foundTarget)
        {
            foundTarget = null;
            
            if (ExecutionCondition == null) return false;

            LayerMask mask = TargetType switch
            {
                TargetingType.Enemy => owner.TargetLayer,
                TargetingType.Ally => (LayerMask)(1 << owner.gameObject.layer),
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

        private Vector3 GetSpawnPosition(StaticAICore owner, Transform target, SpawnPosition spawnAt)
        {
            if (!owner) return Vector3.zero;

            return spawnAt switch
            {
                SpawnPosition.Target when target => target.position,
                SpawnPosition.OwnerMiddle => GetOwnerMiddlePosition(owner),
                SpawnPosition.OwnerFront => GetOwnerMiddlePosition(owner) + GetOwnerFacingDirection(owner) * OwnerFrontDistance,
                _ => owner.transform.position
            };
        }

        private static Vector3 GetOwnerMiddlePosition(StaticAICore owner)
        {
            if (!owner) return Vector3.zero;

            if (owner.TryGetComponent(out Collider2D ownerCollider))
            {
                return ownerCollider.bounds.center;
            }

            var renderers = owner.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return owner.transform.position;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds.center;
        }

        private static Vector3 GetOwnerFacingDirection(StaticAICore owner)
        {
            if (!owner) return Vector3.right;

            return owner.transform.localScale.x < 0f ? Vector3.right : Vector3.left;
        }

        private static void ApplyOwnerFacingFlip(GameObject instance, StaticAICore owner, bool shouldFlip)
        {
            if (!instance || !owner || !shouldFlip) return;

            var scale = instance.transform.localScale;
            var facingDirection = GetOwnerFacingDirection(owner);

            scale.x = Mathf.Abs(scale.x) * (facingDirection.x > 0f ? -1f : 1f);
            instance.transform.localScale = scale;
        }

        private void ApplySkillPrefabScale(GameObject instance)
        {
            if (!instance || SkillPrefabScale <= 0f) return;

            instance.transform.localScale *= SkillPrefabScale;
        }

        private void ApplyWindupPrefabScale(GameObject instance)
        {
            if (!instance || WindupPrefabScale <= 0f) return;

            instance.transform.localScale *= WindupPrefabScale;
        }

        private void UpdateWindupFollow(GameObject instance, StaticAICore owner, Transform target)
        {
            if (!instance || !owner || WindupSpawnAt != SpawnPosition.Owner) return;

            instance.transform.position = GetSpawnPosition(owner, target, WindupSpawnAt);
            instance.transform.rotation = owner.transform.rotation;
        }

        private void UpdateSkillPrefabFollow(GameObject instance, StaticAICore owner, Transform target)
        {
            if (!instance || !owner || !FollowSkillPrefab) return;

            instance.transform.position = GetSpawnPosition(owner, target, SpawnAt);
            instance.transform.rotation = owner.transform.rotation;
        }

        private static List<SpriteFadeTarget> CaptureSpriteFadeTargets(GameObject instance)
        {
            var targets = new List<SpriteFadeTarget>();
            if (!instance) return targets;

            var renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in renderers)
            {
                if (!spriteRenderer) continue;
                targets.Add(new SpriteFadeTarget(spriteRenderer, spriteRenderer.color));
            }

            return targets;
        }

        private static void SetSpriteFadeAlpha(List<SpriteFadeTarget> targets, float alpha)
        {
            if (targets == null) return;

            alpha = Mathf.Clamp01(alpha);
            foreach (var target in targets)
            {
                if (!target.Renderer) continue;

                var color = target.BaseColor;
                color.a *= alpha;
                target.Renderer.color = color;
            }
        }

        private float CalculateWindupFadeAlpha(float elapsed, float duration)
        {
            if (duration <= 0f) return 1f;

            var alpha = 1f;
            if (WindupFadeInTime > 0f)
            {
                alpha = Mathf.Min(alpha, elapsed / WindupFadeInTime);
            }

            if (WindupFadeOutTime > 0f)
            {
                alpha = Mathf.Min(alpha, (duration - elapsed) / WindupFadeOutTime);
            }

            return Mathf.Clamp01(alpha);
        }
    }
}
