using System.Collections.Generic;
using BattleK.Scripts.AI.StaticScoreState;
using BattleK.Scripts.AI.StaticScoreState.ActionStates;
using BattleK.Scripts.AI.StaticScoreState.Attack;
using BattleK.Scripts.AI.StaticScoreState.StaticVerStates;
using BattleK.Scripts.AI.StaticScoreState.Targeting;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager;
using Pathfinding;
using UnityEngine;

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
        
        [Header("Runtime Info")]
        public Transform Target;
        
        [Header("Targeting")]
        public List<UnitClass> TargetClass;
        public TargetStrategy TargetingStrategy;
        public IStaticTargetingStrategy Targeting {get; private set;}

        public LayerMask TargetLayer;
        public bool IsDead => OverrideMachine.CurrentState is StaticDeathState;

        [HideInInspector] public float LastRetreatFinishTime;
        private float _attackTimer;
        private float _lastDecisionTime;

        private readonly List<IStaticActionState> _actionCandidates = new();

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
                TargetStrategy.NearestTarget => new NearestTargetStrategy(TargetLayer),
                TargetStrategy.NearestTargetWithClass => new ClassPriorityTargetStrategy(TargetLayer, TargetClass),
                _ => Targeting
            };
            AiManager = AI_Manager.Instance;
        }

        private void Update()
        {
            if(_attackTimer > 0f) _attackTimer -= Time.deltaTime;

            if (OverrideMachine.CurrentState != null)
            {
                if(MainMachine.CurrentState != null && MainMachine.CurrentState is not StaticIdleState) MainMachine.ChangeState(new StaticIdleState(this));
                return;
            }
            
            if (Time.time < _lastDecisionTime + _aiUpdateInterval) return;
            _lastDecisionTime = Time.time;
            
            DecideNextAction();
        }

        public void EnableWeapon()
        {
            if(Stat.IsRanged) RangedWeapon.Fire(Stat.AttackDamage);
            else MeleeWeapon.EnableHitBox(Stat.AttackDamage);
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

        public void SetMoveSpeedMultiplier(float multiplier)
        {
            var newSpeed = Stat.MoveSpeed * multiplier;
            AiPath.maxSpeed = newSpeed;
        }

        public void ResetMoveSpeed()
        {
            AiPath.maxSpeed = Stat.MoveSpeed;
        }
        
        public void PlayAnimation( PlayerState animState)
        {
            player.SetStateAnimationIndex(animState);
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

        public void OnTakeDamage(int damage)
        {
            if (IsDead) return;
            Stat.CurrentHP -= damage;
            if (Stat.CurrentHP <= 0)
            {
                OnDead();
                return;
            }
            
            if(OverrideMachine.CurrentState == null) OverrideMachine.ChangeState(new StaticHitState(this));
        }

        public void EnterCCState(float duration, PlayerState state)
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
            if (Stat.IsPlayer) AiManager.playerUnits.Remove(this);
            else AiManager.enemyUnits.Remove(this);
        }

        private void RegisterActionStates()
        {
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
