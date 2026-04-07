using System;
using System.Collections.Generic;
using BattleK.Scripts.AI.StaticScoreState;
using BattleK.Scripts.AI.StaticScoreState.ActionStates;
using BattleK.Scripts.AI.StaticScoreState.Attack;
using BattleK.Scripts.AI.StaticScoreState.StaticVerStates;
using BattleK.Scripts.AI.StaticScoreState.Targeting;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleK.Scripts.AI
{
    public class StaticAICore : MonoBehaviour
    {
        public StaticStateMachine OverrideMachine { get; private set; } 
        public StaticStateMachine MainMachine { get; private set; }
        
        [Header("AI Settings")]
        [SerializeField] private float _aiUpdateInterval = 0.2f;
        [SerializeField] private float _windupTime = 0.5f;
        [SerializeField] private float _activeTime = 0.5f;
        [SerializeField] private float _recoveryTime = 0.5f;
        public int AttackIndex;
        public int SkillIndex;
        public bool IsInitialized { get; private set; }
        
        [Header("References")]
        public AIPath AiPath;
        public Rigidbody2D Rigidbody;
        public StaticMeleeAttack MeleeWeapon;
        public StaticRangedAttack RangedWeapon;
        public HPBar HPBar;
        public PlayerObjC player;
        public AI_Manager AiManager;

        [Header("Stats")]
        public UnitStat Stat;
        public float CurrentMoveSpeed { get; private set; }
        public int CurrentAttackDamage { get; private set; }
        public int CurrentDefense { get; private set; }
        public int CurrentSkillPoint { get; private set; }
        public float CurrentEvasionRate { get; private set; }
        
        [Header("Runtime Info")]
        public Transform Target;
        
        [Header("Targeting")]
        public List<UnitClass> TargetClass;
        public TargetStrategy TargetingStrategy;
        public IStaticTargetingStrategy Targeting {get; private set;}
        public LayerMask TargetLayer;
        
        private StaticAICore _targetCore;
        public bool IsDead => OverrideMachine.CurrentState is StaticDeathState;

        [HideInInspector] public float LastRetreatFinishTime;
        private float _attackTimer;
        private float _lastDecisionTime;

        private readonly List<IStaticActionState> _actionCandidates = new();
        private readonly Dictionary<StatusType, Dictionary<object, float>> _modifiers = new();

        public bool IsAttackReady => _attackTimer <= 0f;
        private void Awake()
        {
            OverrideMachine = new StaticStateMachine(this);
            MainMachine = new StaticStateMachine(this);
            
            if(MeleeWeapon) MeleeWeapon.Initialize(this);
            if(RangedWeapon) RangedWeapon.Initialize(this);
            
            RegisterActionStates();
        }

        private void Start()
        {
            Targeting = TargetingStrategy switch
            {
                TargetStrategy.NearestTarget => new NearestTargetStrategy(),
                TargetStrategy.NearestTargetWithClass => new ClassPriorityTargetStrategy(TargetLayer, TargetClass),
                _ => Targeting
            };
            AiManager = AI_Manager.Instance;
        }

        private void Update()
        {
            if (!IsInitialized) return;
            if(_attackTimer > 0f) _attackTimer -= Time.deltaTime;

            CheckTargetStatus();
            
            if (OverrideMachine.CurrentState != null)
            {
                if(MainMachine.CurrentState != null && MainMachine.CurrentState is not StaticIdleState) MainMachine.ChangeState(new StaticIdleState(this));
                return;
            }
            
            if (Time.time < _lastDecisionTime + _aiUpdateInterval) return;
            _lastDecisionTime = Time.time;
            
            DecideNextAction();
        }
        
        public void Initialize()
        {
            IsInitialized = true; 
        }

        private void CheckTargetStatus()
        {
            if (!Target) return;
            if(!_targetCore || _targetCore.transform != Target) _targetCore = Target.GetComponent<StaticAICore>();
            if (_targetCore && _targetCore.gameObject.activeInHierarchy && !_targetCore.IsDead) return;
            Target = null;
            _targetCore = null;
            DecideNextAction();
        }

        public void EnableWeapon()
        {
            if(Stat.IsRanged) RangedWeapon.Fire(CurrentAttackDamage);
            else MeleeWeapon.EnableHitBox(CurrentAttackDamage);
        }

        public void DisableWeapon()
        {
            if (MeleeWeapon && !Stat.IsRanged) 
                MeleeWeapon.DisableHitBox();
        }
        
        public void SetAttackCooldown()
        {
            _attackTimer = Stat.AttackDelay;
        }

        public void StopMovement()
        {
            AiPath.isStopped = true;
            Rigidbody.velocity = Vector2.zero;
        }

        public void ResumeMovement()
        {
            AiPath.isStopped = false;
        }

        public void MoveTo(Vector3 position)
        {
            AiPath.isStopped = false;
            AiPath.destination = position;
        }

        public void LookAt(Vector3 targetPosition)
        {
            var dir = targetPosition - transform.position;
            if (dir.x == 0) return;
            var scale = transform.localScale;
            scale.x = dir.x > 0 ? -0.7f : 0.7f;
            transform.localScale = scale;
        }

        public void SetStatMultiplier(StatusType type, object source, float multiplier)
        {
            if (!_modifiers.ContainsKey(type)) _modifiers[type] = new Dictionary<object, float>();
            _modifiers[type][source] = multiplier;
            Debug.Log($"현재 {name}의 {type}형 stat이 {multiplier} 배율로 변화");
            UpdateFinalStat(type);
        }

        public void RemoveStatMultiplier(StatusType type, object source)
        {
            if (!_modifiers.TryGetValue(type, out var sourceDict)) return;
            sourceDict.Remove(source);
            UpdateFinalStat(type);
        }

        public bool HasDebuff() //디버프 확인(임시)
        {
            foreach (var type in _modifiers)
            {
                foreach (var value in type.Value.Values)
                {
                    if (value < 1f)
                        return true;
                }
            }
            return false;
        }

        public void RemoveAllDebuffs() //디버프 전체 제거()
        {
            var types = new List<StatusType>(_modifiers.Keys);

            foreach (var type in types)
            {
                var sources = new List<object>(_modifiers[type].Keys);

                foreach (var src in sources)
                {
                    if (_modifiers[type][src] < 1f)
                    {
                        RemoveStatMultiplier(type, src);
                    }
                }
            }
        }
        
        private void UpdateFinalStat(StatusType type)
        {
            var finalMul = 1f;

            if (_modifiers.TryGetValue(type, out var sourceDict))
            {
                foreach (var val in sourceDict.Values)
                {
                    finalMul *= val;
                }
            }
            switch (type)
            {
                case StatusType.MoveSpeedMultiplier:
                    CurrentMoveSpeed = Stat.MoveSpeed * finalMul;
                    AiPath.maxSpeed = CurrentMoveSpeed;
                    break;
                case StatusType.AttackDamageMultiplier:
                    CurrentAttackDamage = (int)(Stat.AttackDamage * finalMul);
                    break;
                case StatusType.DefenseMultiplier:
                    CurrentDefense = (int)(Stat.Defense * finalMul);
                    break;
                case StatusType.SkillPointMultiplier:
                    CurrentSkillPoint = (int)(Stat.SkillPoint * finalMul);
                    break;
                case StatusType.EvasionRateMultiplier:
                    CurrentEvasionRate = (int)(Stat.EvasionRate * finalMul);
                    break;
            }
        }
        
        public void ResetMoveSpeed()
        {
            AiPath.maxSpeed = Stat.MoveSpeed;
        }
        
        public void PlayAnimation( PlayerState animState, int animIndex = 0)
        {
            player.SetStateAnimationIndex(animState, animIndex);
            player.PlayStateAnimation(animState);
        }
        
        private void DecideNextAction()
        {
            IStaticActionState bestAction = null;
            var highestPriority = int.MinValue;

            foreach (var action in _actionCandidates)
            {
                if(!action.CanExecute()) continue;
                if (action.Priority <= highestPriority) continue;
                highestPriority = action.Priority;
                bestAction = action;
            }

            if (bestAction == null) return;
            if (MainMachine.CurrentState != bestAction)
            {
                MainMachine.ChangeState(bestAction);
            }
        }

        public void OnTakeDamage(int damage, bool isPenetrating = false)
        {
            if (IsDead || Stat.CurrentHP == 0) return;
            
            var randomVal = Random.Range(0f, 100f);
            if (randomVal < CurrentEvasionRate)
            {
                Debug.Log($"{name} is evaded");
                return;
            }
            var realDamage = isPenetrating ? damage : (int)Mathf.Max(1, damage * (float)(100.0 / (100 + CurrentDefense)));
            Stat.CurrentHP -= realDamage;
            HPBar.UpdateHPBar();
            if (Stat.CurrentHP <= 0)
            {
                OnDead();
                return;
            }
            if(OverrideMachine.CurrentState == null) OverrideMachine.ChangeState(new StaticHitState(this));
        }

        public void OnHeal(int amount)
        {
            Stat.CurrentHP = Mathf.Min(Stat.CurrentHP + amount, Stat.MaxHP);
            HPBar.UpdateHPBar();
        }

        public void EnterCCState(PlayerState state)
        {
            if (IsDead) return;
            OverrideMachine.ChangeState(new StaticCCState(this, state));
        }

        public void ExitCCState()
        {
            if (OverrideMachine.CurrentState is StaticCCState)
                OverrideMachine.StopAndClear();
        }

        public void OnDead()
        {
            OverrideMachine.ChangeState(new StaticDeathState(this));
            AiManager.UnregisterUnit(this);
            if (AiManager.IsAlreadyDone) return;
            AiManager.IsWinner();
        }

        private void RegisterActionStates()
        {
            if (Stat.Skills is { Count: > 0 })
            {
                _actionCandidates.Add(new StaticSkillState(this, Stat.Skills));
            }
            _actionCandidates.Add(new StaticRetreatState(this));
            _actionCandidates.Add(new StaticAttackState(this, _windupTime, _activeTime, _recoveryTime));
            _actionCandidates.Add(new StaticChaseState(this));
            _actionCandidates.Add(new StaticIdleState(this));
            _actionCandidates.Add(new StaticSearchState(this));
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Stat == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Stat.SightRange);
        }
#endif
    }
}
