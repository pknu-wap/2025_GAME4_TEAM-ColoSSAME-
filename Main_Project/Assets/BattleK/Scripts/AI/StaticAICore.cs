using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleK.Scripts.AI.AIState;
using BattleK.Scripts.AI.AIState.StaticVerStates;
using BattleK.Scripts.AI.CCState;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.AI.StaticScoreState.ActionStates;
using BattleK.Scripts.AI.StaticScoreState.Targeting;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Data.Stat;
using BattleK.Scripts.Data.Type.AIDataType.CC;
using BattleK.Scripts.HP;
using BattleK.Scripts.Manager;
using BattleK.Scripts.Combat;
using BattleK.Scripts.Combat.Damage;
using BattleK.Scripts.Combat.Revive;
using BattleK.Scripts.Combat.Status;
using BattleK.Scripts.Manager.Battle;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace BattleK.Scripts.AI
{
    public class StaticAICore : MonoBehaviour
    {
        public enum DeathReason
        {
            Combat,
            System
        }

        public StaticStateMachine OverrideMachine { get; private set; }
        public StaticStateMachine MainMachine { get; private set; }

        [Header("AI Settings")]
        [SerializeField] private float _aiUpdateInterval = 0.2f;
        public int AttackIndex;
        public int SkillIndex;
        public bool IsInitialized { get; private set; }

        [Header("References")]
        public AIPath AiPath;
        public Rigidbody2D Rigidbody;
        public HPBar HPBar;
        public PlayerObjC player;
        public AI_Manager AiManager;
        private UnitLoadManager _unitLoadManager;
        private UserSaveManager _userSaveManager;
        private EnemySaveManager _enemySaveManager;
        private League _league;

        private static readonly IDamageSourceProfile LegacyProfile = new SimpleDamageSourceProfile("Legacy");

        public CombatContext Combat => AiManager ? AiManager.Combat : null;
        private StatusEffectManager _statusEffects;
        public StatusEffectManager StatusEffects => _statusEffects ? _statusEffects : _statusEffects = GetComponent<StatusEffectManager>();
        private UnitHealth _health;
        public UnitHealth Health => _health ??= CreateHealth();

        private UnitStats _stats;
        public UnitStats Stats => _stats ??= CreateStats();

        private HitReaction _reaction;
        public HitReaction Reaction => _reaction ??= new HitReaction(this);

        private UnitHealth CreateHealth()
        {
            var health = new UnitHealth(this);
            health.Changed += _ => OnStatChanged?.Invoke();
            return health;
        }

        private UnitStats CreateStats()
        {
            var stats = new UnitStats(this);
            stats.Changed += OnStatsChanged;
            return stats;
        }

        [FormerlySerializedAs("Stat")] [Header("Stats")]
        public UnitRuntimeStat runtimeStat;
        public SkillSO NormalAttack;
        public List<SkillSO> ResolvedSkills = new();

        public float CurrentMoveSpeed => Stats.MoveSpeed;
        public int CurrentAttackDamage => Stats.AttackDamage;
        public int CurrentDefense => Stats.Defense;
        public int CurrentSkillPoint => Stats.SkillPoint;
        public float CurrentEvasionRate => Stats.EvasionRate;
        public float CurrentAttackSpeed => Stats.AttackSpeed;
        public float CurrentAttackDelay => Stats.AttackDelay;
        public int CurrentMaxHp => Stats.MaxHp;

        public long TotalDamageDealt => Combat != null ? Combat.Recorder.GetDealt(this) : 0;
        public long TotalDamageTaken => Combat != null ? Combat.Recorder.GetTaken(this) : 0;

        [Header("Runtime Info")]
        public Transform Target;

        [Header("Targeting")]
        public List<UnitClass> TargetClass;
        public TargetStrategy TargetingStrategy;
        public IStaticTargetingStrategy Targeting { get; private set; }
        public LayerMask TargetLayer;

        [Header("Revive")]
        [Tooltip("부활 시 채워지는 HP 비율 (MaxHP 기준)")]
        [SerializeField] private float _reviveHpRatio = 0.3f;
        [Tooltip("부활 시 표시할 시각 상태 (없으면 Normal)")]
        [SerializeField] private StatusVisualType _reviveVisualType = StatusVisualType.Normal;
        private ReviveController _revive;
        public ReviveController Revive => _revive ??= new ReviveController(this);
        public bool HasReviveCharge => Revive.Count > 0;

        private StaticAICore _targetCore;
        public bool IsDead => OverrideMachine.CurrentState is StaticDeathState;
        public bool IsInvincible => StatusEffects && StatusEffects.HasFlag(UnitStatusFlags.Invincible);
        public event Action OnStatChanged;

        [HideInInspector] public float LastRetreatFinishTime;
        private float _attackTimer;
        private float _lastDecisionTime;

        private readonly List<IStaticActionState> _actionCandidates = new();
        private readonly HashSet<StatusVisualType> _activeVisualStatuses = new();

        public bool IsAttackReady => _attackTimer <= 0f;

        public bool HasVisualStatus(StatusVisualType type) => _activeVisualStatuses.Contains(type);

        public void SetVisualStatus(StatusVisualType type, bool active)
        {
            if (active) _activeVisualStatuses.Add(type);
            else _activeVisualStatuses.Remove(type);

            if (!AiManager || !AiManager.HPManager) return;
            var isAlly = gameObject.layer == AiManager.PlayerLayer;
            AiManager.HPManager.NotifyStatusChanged(this, isAlly);
        }

        public async Task PrepareAsync()
        {
            ResolvedSkills = await runtimeStat.ResolveEquippedSkillsAsync();
        }

        public void InjectSaveDependencies(
            UnitLoadManager unitLoadManager,
            UserSaveManager userSaveManager,
            EnemySaveManager enemySaveManager,
            League league)
        {
            _unitLoadManager = unitLoadManager;
            _userSaveManager = userSaveManager;
            _enemySaveManager = enemySaveManager;
            _league = league;
        }

        public void ResetBattleStatistics()
        {
            Combat?.Recorder.ResetUnit(this);
        }

        private void Awake()
        {
            OverrideMachine = new StaticStateMachine(this);
            MainMachine = new StaticStateMachine(this);

            _enemySaveManager = EnemySaveManager.Instance;
            _league = LeagueManager.Instance.league;

            RegisterActionStates();
            if (HPBar) HPBar.Bind(this);
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
            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

            CheckTargetStatus();

            if (OverrideMachine.CurrentState != null)
            {
                if (MainMachine.CurrentState != null && MainMachine.CurrentState is not StaticIdleState) MainMachine.ChangeState(new StaticIdleState(this));
                return;
            }

            if (Time.time < _lastDecisionTime + _aiUpdateInterval) return;
            _lastDecisionTime = Time.time;

            DecideNextAction();
        }

        public void Initialize()
        {
            ResetBattleStatistics();
            IsInitialized = true;
            OnStatChanged?.Invoke();
        }

        private void CheckTargetStatus()
        {
            if (!Target) return;
            if (!_targetCore || _targetCore.transform != Target) _targetCore = Target.GetComponent<StaticAICore>();
            if (_targetCore && _targetCore.gameObject.activeInHierarchy && !_targetCore.IsDead) return;
            Target = null;
            _targetCore = null;
            DecideNextAction();
        }

        public void SetAttackCooldown()
        {
            _attackTimer = CurrentAttackDelay > 0f ? CurrentAttackDelay : runtimeStat.AttackDelay;
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

        public void SetInitialStats()
        {
            Stats.ResetFromBase();
            ClearAllVisualStatuses();

            Health.Initialize(Stats.MaxHp, runtimeStat.CurrentHP);
            ApplyStatSideEffects();
        }

        private void OnStatsChanged()
        {
            ApplyStatSideEffects();
            var newMax = Stats.MaxHp;
            if (newMax == Health.Max)
            {
                OnStatChanged?.Invoke();
                return;
            }
            Health.SetMax(newMax);
        }

        private void ApplyStatSideEffects()
        {
            if (AiPath) AiPath.maxSpeed = Stats.MoveSpeed;

            var delay = Stats.AttackDelay;
            if (delay > 0f && _attackTimer > delay) _attackTimer = delay;
        }

        public void SetStatMultiplier(StatusType type, object source, StatSourceCategory category, string label, float delta)
            => Stats.SetMultiplier(type, source, category, label, delta);

        public void RemoveStatMultiplier(StatusType type, object source)
            => Stats.RemoveMultiplier(type, source);

        public void SetFlatModifier(FlatStatusType type, object source, StatSourceCategory category, string label, float amount)
            => Stats.SetFlat(type, source, category, label, amount);

        public void RemoveFlatModifier(FlatStatusType type, object source)
            => Stats.RemoveFlat(type, source);

        public bool HasDebuff() => StatusEffects && StatusEffects.HasTag(StatusTags.Debuff);

        public bool HasStatus(StatusType type) => Stats.Has(type);

        public void RemoveAllDebuffs()
        {
            if (StatusEffects) StatusEffects.RemoveByTag(StatusTags.Debuff, StatusTags.HardCC);
        }

        public void ResetMoveSpeed()
        {
            AiPath.maxSpeed = runtimeStat.MoveSpeed;
        }

        public StatBreakdown GetBreakdown(StatusType type, float baseValue)
            => Stats.GetBreakdown(type, baseValue);

        public StatBreakdown GetFlatBreakdown(FlatStatusType type, float baseValue)
            => Stats.GetFlatBreakdown(type, baseValue);

        public (float Base, float Total, float Delta) GetDisplayValue(FlatStatusType flatType, StatusType multiplierType, float baseValue)
            => Stats.GetDisplayValue(flatType, multiplierType, baseValue);

        public void ClearAllVisualStatuses()
        {
            if (_activeVisualStatuses.Count == 0) return;
            _activeVisualStatuses.Clear();

            var isAlly = AiManager && gameObject.layer == AiManager.PlayerLayer;
            if (AiManager && AiManager.HPManager) AiManager.HPManager.NotifyStatusChanged(this, isAlly);
        }

        public void GrantRevive()
        {
            Revive.Add(new ChargeReviveProvider(_reviveHpRatio, _reviveVisualType));
        }

        public void PlayAnimation(PlayerState animState, int animIndex = 0)
        {
            player.SetStateAnimationIndex(animState, animIndex);
            player.PlayStateAnimation(animState);
        }

        private void DecideNextAction()
        {
            IStaticActionState bestAction = null;
            var highestPriority = int.MinValue;

            foreach (var action in _actionCandidates.Where(action => action.CanExecute()).Where(action => action.Priority > highestPriority))
            {
                highestPriority = action.Priority;
                bestAction = action;
            }

            if (bestAction == null) return;
            if (MainMachine.CurrentState != bestAction)
            {
                MainMachine.ChangeState(bestAction);
            }
        }

        public void OnTakeDamage(int damage, bool isPenetrating = false, StaticAICore attacker = null)
        {
            ReceiveDamage(LegacyProfile.CreateRequest(
                attacker, this, damage,
                DamageType.Physical,
                isPenetrating ? DamageFlags.IgnoreDefense : DamageFlags.None));
        }

        public void ReceiveDamage(in DamageRequest request)
        {
            var combat = Combat;
            if (combat == null) return;

            combat.Damage.Deal(request);
        }

        public void HandleKilled()
        {
            OnDead();
        }

        public void OnHeal(int amount, StaticAICore healer = null)
        {
            var missing = Mathf.Max(0, CurrentMaxHp - runtimeStat.CurrentHP);
            var effective = Mathf.Min(amount, missing);

            Health.Increase(amount);
            Combat?.Recorder.RecordHeal(healer, this, effective);
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

        public void OnDead(DeathReason reason = DeathReason.Combat)
        {
            if (IsDead) return;
            switch (reason)
            {
                case DeathReason.Combat:
                    if (AiManager.IsAlreadyDone) return;
                    AiManager.UnregisterUnit(this);
                    if (runtimeStat.InjuryLevel <= InjuryStatus.FatalInjury)
                        ++runtimeStat.InjuryLevel;
                    break;

                case DeathReason.System:
                    AiManager.UnregisterUnit(this);
                    break;
            }
            PersistUnitState();
            if (StatusEffects) StatusEffects.ClearAll();
            OverrideMachine.ChangeState(new StaticDeathState(this));
            if (reason == DeathReason.Combat)
            {
                AiManager.IsWinner();
            }
        }

        private void RegisterActionStates()
        {
            if (runtimeStat?.EquippedSkills is { Count: > 0 })
            {
                _actionCandidates.Add(new StaticSkillState(this, ResolvedSkills));
            }
            _actionCandidates.Add(new StaticRetreatState(this));
            _actionCandidates.Add(new StaticAttackState(this, NormalAttack));
            _actionCandidates.Add(new StaticChaseState(this));
            _actionCandidates.Add(new StaticIdleState(this));
            _actionCandidates.Add(new StaticSearchState(this));
        }

        private void PersistUnitState()
        {
            var isPlayerUnit = AiManager && gameObject.layer == AiManager.PlayerLayer;
            if (isPlayerUnit) PersistPlayerUnit();
            else PersistEnemyUnit();
        }

        private void PersistPlayerUnit()
        {
            if (!_unitLoadManager || !_userSaveManager) return;

            var user = _unitLoadManager.LoadedUser;
            var unitData = user?.myUnits?.Find(u => string.Equals(u.Id?.Trim(), runtimeStat.Name?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (unitData == null) return;

            runtimeStat.SaveTo(unitData);
            _userSaveManager.SaveUser(user);
        }

        private void PersistEnemyUnit()
        {
            if (_enemySaveManager == null || _league == null) return;
            var team = _enemySaveManager.GetTeam(_league.currentEnemyTeamId);

            foreach (var seenEnemy in team.units.Select(unit => new SeenEnemyData
                     {
                         unitId = unit.Id,
                         unitName = unit.UnitName,
                         teamFid = team.fid,
                         teamName = team.name
                     }))
            {
            }
            var unitData = team.units?.Find(u =>
                string.Equals(u.Id?.Trim(), runtimeStat.Name?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (unitData == null) return;

            _enemySaveManager.RecordSeenEnemyTeam(team);

            runtimeStat.SaveTo(unitData);
        }
    }
}