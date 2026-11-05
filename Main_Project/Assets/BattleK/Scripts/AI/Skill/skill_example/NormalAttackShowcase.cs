using System;
using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;
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
        }

        [Header("평타 SO 목록 (위에서부터 순서대로 시연)")]
        [SerializeField] private List<AttackEntry> _normalAttacks = new();
        [SerializeField, Min(1)] private int _attacksPerEntry = 3;
        [SerializeField, Min(0.1f)] private float _attackInterval = 1f;
        [Header("씬 연결")]
        [SerializeField] private GameObject _actorsRoot;
        [SerializeField] private StaticAICore _target;
        [SerializeField] private AI_Manager _battleManager;
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Text _statusText;

        private readonly Dictionary<StaticAICore, Vector3> _restPositions = new();
        private readonly Dictionary<StaticAICore, Vector3> _restScales = new();
        private Coroutine _sequence;

        public IReadOnlyList<AttackEntry> NormalAttacks => _normalAttacks;
        public int CompletedAttackCount { get; private set; }
        public bool IsRunning => _sequence != null;

        private void Awake()
        {
            foreach (var entry in _normalAttacks)
                if (entry?.Attacker) RememberRestPose(entry.Attacker);
            if (_target) RememberRestPose(_target);
            SetStatus("Ready - press Start Battle");
        }

        public void BeginShowcase()
        {
            if (!Application.isPlaying || IsRunning) return;
            if (!_actorsRoot || !_target || !_battleManager || _normalAttacks.Count == 0) return;
            foreach (var entry in _normalAttacks)
            {
                if (entry != null && entry.Attacker && entry.NormalAttackSO && entry.NormalAttackSO.SkillPrefab)
                    continue;
                global::UnityEngine.Debug.LogError("[NormalAttackShowcase] Attacker와 평타 SO를 연결하세요.", this);
                return;
            }
            CompletedAttackCount = 0;
            if (_startBattleButton) _startBattleButton.interactable = false;
            _sequence = StartCoroutine(ShowcaseRoutine());
        }

        private IEnumerator ShowcaseRoutine()
        {
            try
            {
                _battleManager.IsAlreadyDone = true;
                foreach (var entry in _normalAttacks) StopAutomaticAi(entry.Attacker);
                StopAutomaticAi(_target);
                _actorsRoot.SetActive(true);
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

                for (var index = 0; index < _normalAttacks.Count; index++)
                {
                    var entry = _normalAttacks[index];
                    var actor = entry.Attacker;
                    var range = actor.runtimeStat.AttackRange;
                    var distance = actor.runtimeStat.IsRanged ? range * 0.8f : Mathf.Min(0.75f, range * 0.8f);
                    MoveActor(actor, _target.transform.position + Vector3.left * distance);
                    actor.Target = _target.transform;
                    actor.LookAt(_target.transform.position);

                    for (var repetition = 0; repetition < _attacksPerEntry; repetition++)
                    {
                        var startedAt = Time.time;
                        SetStatus($"{index + 1}/{_normalAttacks.Count}  {entry.UnitClass}  {repetition + 1}/{_attacksPerEntry}");
                        actor.player.ChangeState(PlayerState.ATTACK, entry.NormalAttackSO.AnimationIndex);
                        yield return entry.NormalAttackSO.ExecuteSkillRoutine(actor, _target.transform);
                        CompletedAttackCount++;
                        actor.player.ChangeState(PlayerState.IDLE);
                        var remaining = _attackInterval - (Time.time - startedAt);
                        if (remaining > 0f) yield return new WaitForSeconds(remaining);
                    }
                    RestActor(actor);
                }
                SetStatus($"Complete - {CompletedAttackCount} attacks");
            }
            finally
            {
                FinishShowcase();
            }
        }

        private void PrepareActor(StaticAICore actor)
        {
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
            RestActor(actor);
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
            _restPositions[actor] = actor.transform.position;
            _restScales[actor] = actor.transform.localScale;
        }

        private void RestActor(StaticAICore actor)
        {
            if (!actor) return;
            StopAutomaticAi(actor);
            if (_restPositions.TryGetValue(actor, out var position))
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
                if (entry != null) RestActor(entry.Attacker);
            RestActor(_target);
            _sequence = null;
            if (_startBattleButton) _startBattleButton.interactable = true;
        }

        private void SetStatus(string value)
        {
            if (_statusText) _statusText.text = value;
        }

        private void OnDisable()
        {
            if (_sequence != null) StopCoroutine(_sequence);
            FinishShowcase();
        }
    }
}
