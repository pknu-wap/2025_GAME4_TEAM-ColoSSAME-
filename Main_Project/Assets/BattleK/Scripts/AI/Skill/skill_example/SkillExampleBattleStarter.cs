using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Manager;
using UnityEngine;

namespace BattleK.Scripts.AI.Skill.SkillExample
{
    public sealed class SkillExampleBattleStarter : MonoBehaviour
    {
        [Header("Team 1")]
        [SerializeField] private List<StaticAICore> _team1 = new();

        [Header("Team 2")]
        [SerializeField] private List<StaticAICore> _team2 = new();

        [Header("Auto Battle")]
        [SerializeField] private AI_Manager _autoBattle;
        [SerializeField] private bool _autoStartOnPlay = true;
        [SerializeField] private bool _findSceneUnitsWhenTeamsAreEmpty = true;
        [SerializeField] private bool _setInitialTargets = true;
        [SerializeField] private bool _disableWinLoseFlowForSkillTest = true;
        [SerializeField] private bool _keepTeam2Stationary = true;
        [SerializeField] private bool _resumeBothTeamsForBattleTest = true;
        [SerializeField] private bool _logTargetChanges = true;
        [SerializeField] private bool _logHpChanges = true;
        [SerializeField] private bool _logInitialHp = true;
        [SerializeField] private float _targetLogInterval = 0.1f;
        [SerializeField] private float _hpLogInterval = 0.1f;
        [SerializeField] private float _startDelay = 0.1f;

        private Coroutine _startRoutine;
        private Coroutine _targetLogRoutine;
        private Coroutine _hpLogRoutine;

        private void Reset()
        {
            _autoBattle = FindObjectOfType<AI_Manager>();
        }

        private void Awake()
        {
            ResolveAutoBattle();
        }

        private void Start()
        {
            if (_autoStartOnPlay)
            {
                BeginBattle();
            }
        }

        [ContextMenu("Begin Skill Example Battle")]
        public void BeginBattle()
        {
            if (_startRoutine != null)
            {
                StopCoroutine(_startRoutine);
            }

            _startRoutine = StartCoroutine(BeginBattleRoutine());
        }

        private IEnumerator BeginBattleRoutine()
        {
            if (_startDelay > 0f)
            {
                yield return new WaitForSeconds(_startDelay);
            }

            if (_findSceneUnitsWhenTeamsAreEmpty && _team1.Count == 0 && _team2.Count == 0)
            {
                SplitSceneUnitsByPosition();
            }

            ResolveAutoBattle();
            if (!_autoBattle)
            {
                _autoBattle = gameObject.AddComponent<AI_Manager>();
            }

            if (!CanUseAutoBattleLayers())
            {
                _startRoutine = null;
                yield break;
            }

            if (_disableWinLoseFlowForSkillTest)
            {
                _autoBattle.IsAlreadyDone = true;
            }

            _autoBattle.playerUnits.Clear();
            _autoBattle.enemyUnits.Clear();

            RegisterTeam(_team1, 0);
            RegisterTeam(_team2, 1);

            if (_keepTeam2Stationary)
            {
                StopTeamAi(_team2);
            }
            else if (_resumeBothTeamsForBattleTest)
            {
                ResumeTeamAi(_team1);
                ResumeTeamAi(_team2);
            }

            if (_setInitialTargets)
            {
                AssignInitialTargets(_team1, _team2);
                if (!_keepTeam2Stationary)
                {
                    AssignInitialTargets(_team2, _team1);
                }
            }

            if (_logTargetChanges)
            {
                StartTargetLog();
            }

            if (_logHpChanges)
            {
                RefreshHpBars(_team1);
                RefreshHpBars(_team2);
                StartHpLog();
            }

            global::UnityEngine.Debug.Log($"[SkillExampleBattleStarter] Started skill example battle. Team1: {_team1.Count}, Team2: {_team2.Count}");
            _startRoutine = null;
        }

        private void ResolveAutoBattle()
        {
            if (_autoBattle)
            {
                return;
            }

            _autoBattle = AI_Manager.Instance ? AI_Manager.Instance : FindObjectOfType<AI_Manager>();
        }

        private bool CanUseAutoBattleLayers()
        {
            var playerLayer = LayerMask.NameToLayer(_autoBattle.playerLayerName);
            var enemyLayer = LayerMask.NameToLayer(_autoBattle.enemyLayerName);

            if (playerLayer != -1 && enemyLayer != -1)
            {
                return true;
            }

            global::UnityEngine.Debug.LogError("[SkillExampleBattleStarter] Player/Enemy layers are missing. Check AI_Manager layer names or Project Settings > Tags and Layers.");
            return false;
        }

        private void RegisterTeam(List<StaticAICore> team, int sideIndex)
        {
            for (var i = team.Count - 1; i >= 0; i--)
            {
                var unit = team[i];
                if (!unit)
                {
                    team.RemoveAt(i);
                    continue;
                }

                WarnIfUnitIsNotReady(unit);
                _autoBattle.RegisterUnit(unit, sideIndex);
            }
        }

        private void AssignInitialTargets(List<StaticAICore> attackers, List<StaticAICore> targets)
        {
            foreach (var attacker in attackers)
            {
                if (!attacker || attacker.Target)
                {
                    continue;
                }

                attacker.Target = FindNearestTarget(attacker, targets);
            }
        }

        private static Transform FindNearestTarget(StaticAICore attacker, List<StaticAICore> targets)
        {
            StaticAICore nearest = null;
            var nearestDistanceSq = float.MaxValue;

            foreach (var target in targets)
            {
                if (!target || target == attacker || !target.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var distanceSq = (target.transform.position - attacker.transform.position).sqrMagnitude;
                if (distanceSq >= nearestDistanceSq)
                {
                    continue;
                }

                nearestDistanceSq = distanceSq;
                nearest = target;
            }

            return nearest ? nearest.transform : null;
        }

        private void SplitSceneUnitsByPosition()
        {
            var units = FindObjectsOfType<StaticAICore>();
            if (units.Length < 2)
            {
                global::UnityEngine.Debug.LogWarning("[SkillExampleBattleStarter] Need at least two StaticAICore units in the scene.");
                return;
            }

            System.Array.Sort(units, (left, right) => left.transform.position.x.CompareTo(right.transform.position.x));

            var splitIndex = Mathf.CeilToInt(units.Length * 0.5f);
            for (var i = 0; i < units.Length; i++)
            {
                if (i < splitIndex)
                {
                    _team1.Add(units[i]);
                }
                else
                {
                    _team2.Add(units[i]);
                }
            }
        }

        private static void StopTeamAi(List<StaticAICore> team)
        {
            foreach (var unit in team)
            {
                if (!unit)
                {
                    continue;
                }

                if (unit.AiPath)
                {
                    unit.AiPath.isStopped = true;
                    unit.AiPath.canMove = false;
                    unit.AiPath.destination = unit.transform.position;
                }

                if (unit.Rigidbody)
                {
                    unit.Rigidbody.velocity = Vector2.zero;
                    unit.Rigidbody.angularVelocity = 0f;
                }

                unit.Target = null;
                unit.enabled = false;
            }
        }

        private static void ResumeTeamAi(List<StaticAICore> team)
        {
            foreach (var unit in team)
            {
                if (!unit)
                {
                    continue;
                }

                unit.enabled = true;

                if (unit.AiPath)
                {
                    unit.AiPath.canMove = true;
                    unit.AiPath.isStopped = false;
                }
            }
        }

        private void StartTargetLog()
        {
            if (_targetLogRoutine != null)
            {
                StopCoroutine(_targetLogRoutine);
            }

            _targetLogRoutine = StartCoroutine(TargetLogRoutine());
        }

        private void StartHpLog()
        {
            if (_hpLogRoutine != null)
            {
                StopCoroutine(_hpLogRoutine);
            }

            _hpLogRoutine = StartCoroutine(HpLogRoutine());
        }

        private IEnumerator TargetLogRoutine()
        {
            var lastTargets = new Dictionary<StaticAICore, Transform>();

            while (isActiveAndEnabled)
            {
                LogTargetChanges(_team1, "Team1", lastTargets);
                LogTargetChanges(_team2, "Team2", lastTargets);

                yield return new WaitForSeconds(Mathf.Max(0.02f, _targetLogInterval));
            }
        }

        private IEnumerator HpLogRoutine()
        {
            var lastHp = new Dictionary<StaticAICore, int>();

            while (isActiveAndEnabled)
            {
                LogHpChanges(_team1, "Team1", lastHp, _logInitialHp);
                LogHpChanges(_team2, "Team2", lastHp, _logInitialHp);

                yield return new WaitForSeconds(Mathf.Max(0.02f, _hpLogInterval));
            }
        }

        private static void LogTargetChanges(List<StaticAICore> team, string teamName, Dictionary<StaticAICore, Transform> lastTargets)
        {
            foreach (var unit in team)
            {
                if (!unit)
                {
                    continue;
                }

                lastTargets.TryGetValue(unit, out var lastTarget);
                if (lastTarget == unit.Target)
                {
                    continue;
                }

                lastTargets[unit] = unit.Target;
                var targetName = unit.Target ? unit.Target.name : "None";
                global::UnityEngine.Debug.Log($"[SkillExampleBattleStarter] {teamName} {unit.name} target -> {targetName}");
            }
        }

        private static void LogHpChanges(List<StaticAICore> team, string teamName, Dictionary<StaticAICore, int> lastHp, bool logInitialHp)
        {
            foreach (var unit in team)
            {
                if (!unit || unit.Stat == null)
                {
                    continue;
                }

                var currentHp = unit.Stat.CurrentHP;
                var maxHp = unit.Stat.MaxHP;

                if (!lastHp.TryGetValue(unit, out var previousHp))
                {
                    lastHp[unit] = currentHp;
                    if (logInitialHp)
                    {
                        global::UnityEngine.Debug.Log($"[SkillExampleBattleStarter] {teamName} {unit.name} HP {currentHp}/{maxHp}");
                    }

                    continue;
                }

                if (previousHp == currentHp)
                {
                    continue;
                }

                lastHp[unit] = currentHp;
                var delta = currentHp - previousHp;
                var signedDelta = delta > 0 ? $"+{delta}" : delta.ToString();

                global::UnityEngine.Debug.Log($"[SkillExampleBattleStarter] {teamName} {unit.name} HP {previousHp}/{maxHp} -> {currentHp}/{maxHp} ({signedDelta})");
            }
        }

        private static void RefreshHpBars(List<StaticAICore> team)
        {
            foreach (var unit in team)
            {
                if (!unit || !unit.HPBar)
                {
                    continue;
                }

                unit.HPBar.UpdateHPBar();
            }
        }

        private static void WarnIfUnitIsNotReady(StaticAICore unit)
        {
            if (unit.Stat == null)
            {
                global::UnityEngine.Debug.LogWarning($"[SkillExampleBattleStarter] {unit.name} has no Stat data.");
                return;
            }

            if (unit.Stat.Skills == null || unit.Stat.Skills.Count == 0)
            {
                global::UnityEngine.Debug.LogWarning($"[SkillExampleBattleStarter] {unit.name} has no skills in Stat > Skills.");
            }

            if (!unit.AiPath || !unit.Rigidbody || !unit.player || !unit.HPBar)
            {
                global::UnityEngine.Debug.LogWarning($"[SkillExampleBattleStarter] {unit.name} is missing AIPath, Rigidbody2D, PlayerObjC, or HPBar references.");
            }
        }
    }
}
