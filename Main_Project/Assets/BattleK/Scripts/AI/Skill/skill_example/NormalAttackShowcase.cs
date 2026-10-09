using System;
using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.AI.Skill.Base.Logic.ExecuteLogic;
using BattleK.Scripts.AI.Skill.Base.Logic.LogicBase;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Manager;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.UI;

namespace BattleK.Scripts.AI.Skill.SkillExample
{
    [DisallowMultipleComponent]
    public sealed class NormalAttackShowcase : MonoBehaviour
    {
        [Serializable]
        public sealed class AttackEntry
        {
            public UnitClass UnitClass;
            public SkillSO NormalAttackSO;
            public StaticAICore Attacker;
            [Min(0.1f)] public float AttackRange = 0.9f;
            public bool IsRanged;
        }

        [Header("평타 SO 목록 (위에서부터 순서대로 시연)")]
        [SerializeField] private List<AttackEntry> _normalAttacks = new();
        [Header("시연 설정")]
        [SerializeField, Min(1)] private int _attacksPerEntry = 3;
        [SerializeField, Min(0.1f)] private float _attackInterval = 1f;
        [Header("씬 연결")]
        [SerializeField] private GameObject _actorsRoot;
        [SerializeField] private StaticAICore _target;
        [SerializeField] private AI_Manager _battleManager;
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Text _statusText;
        [Header("현재 시연 상태")]
        [SerializeField] private string _currentAttack = "Ready";
        [SerializeField] private int _completedAttackCount;

        private readonly Dictionary<StaticAICore, Vector3> _restPositions = new();
        private readonly Dictionary<StaticAICore, Vector3> _restScales = new();
        private readonly List<SkillSO> _runtimeSkills = new();
        private readonly List<GameObject> _runtimeTemplates = new();
        private Coroutine _sequence;

        public IReadOnlyList<AttackEntry> NormalAttacks => _normalAttacks;
        public int CompletedAttackCount => _completedAttackCount;
        public bool IsRunning => _sequence != null;
        public event Action<int, int, float> AttackStarted;

        private void Awake()
        {
            foreach (var entry in _normalAttacks)
            {
                if (entry?.Attacker) RememberRestPose(entry.Attacker);
            }
            if (_target) RememberRestPose(_target);
            UpdateStatus("Ready - press Start Battle");
        }

        public void BeginShowcase()
        {
            if (!Application.isPlaying || IsRunning) return;
            if (!ValidateSetup()) return;
            _completedAttackCount = 0;
            if (_startBattleButton) _startBattleButton.interactable = false;
            _sequence = StartCoroutine(ShowcaseRoutine());
        }

        private bool ValidateSetup()
        {
            if (!_actorsRoot || !_target || !_battleManager || _normalAttacks.Count == 0)
            {
                global::UnityEngine.Debug.LogError("[NormalAttackShowcase] 시연 유닛, 타겟, 매니저와 평타 목록을 연결하세요.", this);
                return false;
            }
            foreach (var entry in _normalAttacks)
            {
                if (entry != null && entry.Attacker && entry.NormalAttackSO && entry.NormalAttackSO.SkillPrefab)
                    continue;
                global::UnityEngine.Debug.LogError("[NormalAttackShowcase] 목록의 Attacker, Normal Attack SO, Skill Prefab 연결을 확인하세요.", this);
                return false;
            }
            return true;
        }

        private IEnumerator ShowcaseRoutine()
        {
            try
            {
                _battleManager.IsAlreadyDone = true;
                foreach (var entry in _normalAttacks) StopAutomaticAi(entry.Attacker);
                StopAutomaticAi(_target);
                _actorsRoot.SetActive(true);
                // Let SPUM and the actors complete Start before changing animations.
                yield return null;

                _battleManager.playerUnits.Clear();
                _battleManager.enemyUnits.Clear();
                foreach (var entry in _normalAttacks)
                {
                    PrepareActor(entry.Attacker);
                    _battleManager.RegisterUnit(entry.Attacker, 0);
                }
                PrepareActor(_target);
                _battleManager.RegisterUnit(_target, 1);

                for (var entryIndex = 0; entryIndex < _normalAttacks.Count; entryIndex++)
                {
                    var entry = _normalAttacks[entryIndex];
                    var actor = entry.Attacker;
                    actor.runtimeStat.AttackRange = entry.AttackRange;
                    actor.runtimeStat.IsRanged = entry.IsRanged;
                    var distance = entry.IsRanged ? entry.AttackRange * 0.8f : Mathf.Min(0.75f, entry.AttackRange * 0.8f);
                    MoveActor(actor, _target.transform.position + Vector3.left * Mathf.Max(0.1f, distance));
                    actor.Target = _target.transform;
                    actor.LookAt(_target.transform.position);
                    var attack = CreateRuntimeAttack(entry);

                    for (var repetition = 0; repetition < Mathf.Max(1, _attacksPerEntry); repetition++)
                    {
                        var startedAt = Time.time;
                        UpdateStatus($"{entryIndex + 1}/{_normalAttacks.Count}  {entry.UnitClass}  {repetition + 1}/{_attacksPerEntry}");
                        AttackStarted?.Invoke(entryIndex, repetition, startedAt);
                        actor.player.ChangeState(PlayerState.ATTACK, entry.NormalAttackSO.AnimationIndex);
                        yield return attack.ExecuteSkillRoutine(actor, _target.transform, waitForActiveTime: false);
                        _completedAttackCount++;
                        RestActor(actor, restorePosition: false);
                        var remaining = Mathf.Max(0.1f, _attackInterval) - (Time.time - startedAt);
                        if (remaining > 0f) yield return new WaitForSeconds(remaining);
                    }
                    RestActor(actor, restorePosition: true);
                }
                UpdateStatus($"Complete - {_completedAttackCount} attacks");
            }
            finally
            {
                FinishShowcase();
            }
        }

        private SkillSO CreateRuntimeAttack(AttackEntry entry)
        {
            // Keep the Inspector's SO and shared prefabs unchanged; processors exist only for this test run.
            var attack = Instantiate(entry.NormalAttackSO);
            var template = Instantiate(entry.NormalAttackSO.SkillPrefab);
            template.name = $"ShowcaseTemplate_{entry.UnitClass}";
            template.transform.SetParent(transform, false);
            template.transform.position = new Vector3(1000 + _runtimeTemplates.Count * 10, 1000, 0);
            if (template.GetComponents<LogicProcessor>().Length == 0)
            {
                if (entry.IsRanged) template.AddComponent<NormalAttackProjectileProcessor>();
                else template.AddComponent<NormalAttackTargetProcessor>();
            }
            attack.SkillPrefab = template;
            _runtimeSkills.Add(attack);
            _runtimeTemplates.Add(template);
            return attack;
        }

        private void PrepareActor(StaticAICore actor)
        {
            RememberRestPose(actor);
            StopAutomaticAi(actor);
            actor.runtimeStat.MaxHP = 1000;
            actor.runtimeStat.CurrentHP = 1000;
            actor.runtimeStat.AttackDamage = 20;
            actor.runtimeStat.Defense = 0;
            actor.runtimeStat.EvasionRate = 0;
            actor.runtimeStat.EquippedSkills.Clear();
            actor.ResolvedSkills.Clear();
            actor.SetInitialStats();
            actor.Initialize();
            RestActor(actor, restorePosition: true);
        }

        private static void StopAutomaticAi(StaticAICore actor)
        {
            if (!actor) return;
            actor.enabled = false;
            actor.MainMachine?.StopAndClear();
            actor.OverrideMachine?.StopAndClear();
            if (actor.AiPath)
            {
                actor.AiPath.isStopped = true;
                actor.AiPath.canMove = false;
                actor.AiPath.enabled = false;
            }
            var rvo = actor.GetComponent<RVOController>();
            if (rvo) rvo.enabled = false;
            if (actor.Rigidbody)
            {
                actor.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
                actor.Rigidbody.velocity = Vector2.zero;
                actor.Rigidbody.angularVelocity = 0f;
            }
            actor.Target = null;
        }

        private void RememberRestPose(StaticAICore actor)
        {
            if (_restPositions.ContainsKey(actor)) return;
            _restPositions.Add(actor, actor.transform.position);
            _restScales.Add(actor, actor.transform.localScale);
        }

        private void RestActor(StaticAICore actor, bool restorePosition)
        {
            if (!actor) return;
            StopAutomaticAi(actor);
            if (restorePosition && _restPositions.TryGetValue(actor, out var position))
            {
                MoveActor(actor, position);
                actor.transform.localScale = _restScales[actor];
            }
            if (actor.player) actor.player.ChangeState(PlayerState.IDLE);
        }

        private static void MoveActor(StaticAICore actor, Vector3 position)
        {
            actor.transform.position = position;
            if (actor.Rigidbody) actor.Rigidbody.position = position;
        }

        private void FinishShowcase()
        {
            foreach (var entry in _normalAttacks)
                if (entry != null) RestActor(entry.Attacker, restorePosition: true);
            RestActor(_target, restorePosition: true);
            foreach (var template in _runtimeTemplates) if (template) Destroy(template);
            foreach (var skill in _runtimeSkills) if (skill) Destroy(skill);
            _runtimeTemplates.Clear();
            _runtimeSkills.Clear();
            _sequence = null;
            if (_startBattleButton) _startBattleButton.interactable = true;
        }

        private void UpdateStatus(string status)
        {
            _currentAttack = status;
            if (_statusText) _statusText.text = status;
        }

        private void OnDisable()
        {
            if (_sequence != null) StopCoroutine(_sequence);
            FinishShowcase();
        }
    }
}
