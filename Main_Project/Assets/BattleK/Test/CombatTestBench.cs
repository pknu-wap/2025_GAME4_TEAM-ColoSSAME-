using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleK.Scripts.AI;
using BattleK.Scripts.AI.Skill.Base;
using BattleK.Scripts.Combat.Damage;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Manager;
using Pathfinding.RVO;
using UnityEngine;

namespace BattleK.Test
{
    // 평타와 스킬을 골라 바로 시험하는 전투 테스트 벤치 (전투 코드는 그대로 쓰고 AI만 끔)
    public sealed class CombatTestBench : MonoBehaviour
    {
        [Serializable]
        public sealed class Caster
        {
            public UnitClass UnitClass;
            public StaticAICore Actor;
            public SkillSO NormalAttack;
            public List<SkillSO> Skills = new();
        }

        [Header("씬 연결 (Tools/전투 테스트/씬 다시 만들기로 자동 연결)")]
        [SerializeField] private AI_Manager _manager;
        [SerializeField] private GameObject _actorsRoot;
        [SerializeField] private List<Caster> _casters = new();
        [SerializeField] private List<StaticAICore> _enemies = new();
        [SerializeField] private StaticAICore _ally;
        [SerializeField] private List<SkillSO> _exampleSkills = new();

        [Header("캐릭터 (비어 있으면 에디터에서 Family/*/UnitRoot_Prefabs를 자동으로 읽음)")]
        [SerializeField] private List<GameObject> _characters = new();

        [Header("기본값")]
        [SerializeField] private Vector2 _casterPosition = new(-2f, 0f);
        [SerializeField] private int _testHp = 99999;

        private static readonly string[] SpeedLabels = { "0.1x", "0.25x", "0.5x", "1x" };
        private static readonly float[] Speeds = { 0.1f, 0.25f, 0.5f, 1f };
        private static readonly string[] CountLabels = { "1", "2", "3", "4", "5" };
        // 범위 스킬 확인용으로 첫 번째 적 주변에 배치
        private static readonly Vector2[] EnemyOffsets = { new(0f, 0f), new(0.4f, 0.6f), new(0.4f, -0.6f), new(0.8f, 1.2f), new(0.8f, -1.2f) };
        private const int MaxLog = 40;

        private int _casterIndex;
        private int _skillIndex;
        private int _enemyCount = 1;
        private float _distance = 2f;
        private float _angle; // 적 방향 (0 오른쪽, 양수 위, 음수 아래)
        private bool _fitRange = true;
        private bool _repeat;
        private float _interval = 1.5f;
        private bool _autoHeal = true;
        private int _speedIndex = 3;
        private bool _paused;
        private float _cameraSize = 2.5f;
        private bool _ready;
        private string _status = "";
        private Coroutine _castRoutine;
        private Vector2 _skillScroll;
        private Vector2 _logScroll;

        private readonly List<string> _log = new();
        private float _logStart;
        private long _total;
        private int _hits;
        private float _firstHit = -1f;
        private readonly Dictionary<SkillSO, SkillSO> _backups = new();

        // 캐릭터 바꾸기: 한 번 만든 유닛은 다시 고를 때 재사용
        private string[] _families = Array.Empty<string>();
        private int _familyIndex;
        private int _characterIndex;
        private bool _swapping;
        private Vector2 _controlScroll;
        private readonly Dictionary<GameObject, StaticAICore> _spawnedCasters = new();
        private readonly Dictionary<GameObject, List<StaticAICore>> _spawnedEnemies = new();
        private List<StaticAICore> _defaultEnemies = new();

        private Caster Current => _casters.Count > 0 ? _casters[_casterIndex] : null;
        private StaticAICore CurrentActor => Current?.Actor;

        public void Setup(AI_Manager manager, GameObject actorsRoot, List<Caster> casters,
            List<StaticAICore> enemies, StaticAICore ally, List<SkillSO> exampleSkills)
        {
            _manager = manager;
            _actorsRoot = actorsRoot;
            _casters = casters;
            _enemies = enemies;
            _ally = ally;
            _exampleSkills = exampleSkills;
        }

        private IEnumerator Start()
        {
            if (!_manager) _manager = AI_Manager.Instance;
            if (!_manager || _casters.Count == 0)
            {
                _status = "연결이 비어 있음. Tools/전투 테스트/씬 다시 만들기";
                yield break;
            }

            _manager.SuppressResultFlow = true;
            _manager.IsAlreadyDone = false; // 씬에 끝난 상태로 저장돼 있으면 데미지가 전부 무시됨
            if (_actorsRoot) _actorsRoot.SetActive(true);
            yield return null; // 유닛 Start에서 AiManager가 연결될 때까지

            foreach (var c in _casters) Prepare(c.Actor, 0, false);
            foreach (var e in _enemies) Prepare(e, 1, true);
            Prepare(_ally, 0, true);

            _manager.Combat.Events.Damaged += OnDamaged;
            Backup();
            ClearLog();
            _defaultEnemies = new List<StaticAICore>(_enemies);
            LoadCharacters();
            _ready = true;
            SelectCaster(0);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (_manager && _manager.Combat != null) _manager.Combat.Events.Damaged -= OnDamaged;
            foreach (var b in _backups.Values) if (b) Destroy(b);
        }

        // ---------- 준비 ----------

        private void Prepare(StaticAICore unit, int side, bool dummy)
        {
            if (!unit) return;
            StopAi(unit);
            _manager.RegisterUnit(unit, side);
            unit.runtimeStat.MaxHP = _testHp;
            unit.runtimeStat.CurrentHP = _testHp;
            if (dummy)
            {
                unit.runtimeStat.Defense = 0;
                unit.runtimeStat.EvasionRate = 0;
            }
            unit.SetInitialStats();
            unit.Initialize();
            StopAi(unit);
            if (unit.player) unit.player.ChangeState(PlayerState.IDLE);
        }

        // NormalAttackShowcase와 같은 방식으로 자동 AI를 끔
        private static void StopAi(StaticAICore unit)
        {
            unit.enabled = false;
            unit.MainMachine?.StopAndClear();
            if (unit.AiPath)
            {
                unit.AiPath.isStopped = true;
                unit.AiPath.canMove = false;
                unit.AiPath.enabled = false;
            }
            var rvo = unit.GetComponent<RVOController>();
            if (rvo) rvo.enabled = false;
            if (unit.Rigidbody)
            {
                unit.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
                unit.Rigidbody.velocity = Vector2.zero;
                unit.Rigidbody.angularVelocity = 0f;
            }
            unit.Target = null;
        }

        private void SelectCaster(int index)
        {
            StopCast();
            _casterIndex = Mathf.Clamp(index, 0, _casters.Count - 1);
            _skillIndex = 0;
            for (var i = 0; i < _casters.Count; i++)
                if (_casters[i].Actor) _casters[i].Actor.gameObject.SetActive(i == _casterIndex);
            if (_fitRange) _distance = FitDistance();
            Layout();
        }

        private float Range => CurrentActor ? CurrentActor.runtimeStat.AttackRange : 0f;

        // 실전처럼 사거리 끝에서 공격 (StaticChaseState는 사거리 안에 들어오면 멈춤)
        private float FitDistance() => Mathf.Max(0.5f, Range * 0.95f);

        private void Layout()
        {
            var caster = CurrentActor;
            if (!caster) return;
            var origin = (Vector3)_casterPosition;

            Place(caster, origin);
            caster.LookAt(origin + Vector3.right);

            for (var i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (!e) continue;
                var on = i < _enemyCount;
                e.gameObject.SetActive(on);
                if (!on) continue;
                var o = EnemyOffsets[Mathf.Min(i, EnemyOffsets.Length - 1)];
                Place(e, origin + Quaternion.Euler(0f, 0f, _angle) * new Vector3(_distance + o.x, o.y));
                e.LookAt(origin);
            }

            if (_ally)
            {
                Place(_ally, origin + new Vector3(-0.8f, -0.7f));
                _ally.LookAt(origin + Vector3.right);
            }
            FitCamera();
        }

        private static void Place(StaticAICore unit, Vector3 position)
        {
            unit.transform.position = position;
            if (unit.Rigidbody) unit.Rigidbody.position = position;
        }

        private void FitCamera()
        {
            var cam = Camera.main;
            if (!cam) return;
            cam.orthographicSize = _cameraSize;
            var p = cam.transform.position;
            var mid = (Vector3)_casterPosition + Quaternion.Euler(0f, 0f, _angle) * new Vector3(_distance * 0.5f, 0f);
            cam.transform.position = new Vector3(mid.x, mid.y + 0.3f, p.z);
        }

        // ---------- 캐릭터 바꾸기 ----------

        private void LoadCharacters()
        {
#if UNITY_EDITOR
            if (_characters.Count == 0)
                _characters = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/BattleK/Family" })
                    .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.Contains("/UnitRoot_Prefabs/"))
                    .OrderBy(p => p)
                    .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>)
                    .Where(g => g && g.GetComponent<StaticAICore>())
                    .ToList();
#endif
            _characters.RemoveAll(c => !c);
            _families = _characters.Select(Family).Distinct().ToArray();
        }

        private static string Family(GameObject prefab) => prefab.name.Split('_')[0];

        private List<GameObject> FamilyCharacters() =>
            _families.Length == 0 ? new List<GameObject>()
                : _characters.Where(c => Family(c) == _families[_familyIndex]).ToList();

        private static string CharacterLabel(GameObject prefab)
        {
            var family = Family(prefab);
            var name = prefab.name.StartsWith(family + "_") ? prefab.name.Substring(family.Length + 1) : prefab.name;
            var core = prefab.GetComponent<StaticAICore>();
            return core && core.runtimeStat != null ? $"{name} ({core.runtimeStat.UnitClass})" : name;
        }

        private StaticAICore Spawn(GameObject prefab, string unitName)
        {
            var parent = _actorsRoot ? _actorsRoot.transform : null;
            var go = Instantiate(prefab, (Vector3)_casterPosition + Vector3.down * 10f, Quaternion.identity, parent);
            go.name = unitName;
            var unit = go.GetComponent<StaticAICore>();
            if (unit) return unit;
            Destroy(go);
            return null;
        }

        // 같은 직업 칸의 시전자를 바꿈 (없는 직업이면 칸을 새로 만듦)
        private IEnumerator SwapCaster(GameObject prefab)
        {
            _swapping = true;
            StopCast();
            if (!_spawnedCasters.TryGetValue(prefab, out var unit) || !unit)
            {
                unit = Spawn(prefab, prefab.name);
                if (!unit) { _status = $"{prefab.name}: StaticAICore 없음"; _swapping = false; yield break; }
                yield return null; // 유닛 Start에서 AiManager가 연결될 때까지
                Prepare(unit, 0, false);
                _spawnedCasters[prefab] = unit;
            }

            var unitClass = unit.runtimeStat.UnitClass;
            var index = _casters.FindIndex(c => c.UnitClass == unitClass);
            if (index < 0)
            {
                _casters.Add(new Caster { UnitClass = unitClass, Actor = unit, NormalAttack = unit.NormalAttack });
                index = _casters.Count - 1;
            }
            else
            {
                var slot = _casters[index];
                if (slot.Actor && slot.Actor != unit) slot.Actor.gameObject.SetActive(false);
                slot.Actor = unit;
                if (unit.NormalAttack) slot.NormalAttack = unit.NormalAttack;
            }
            BackupOne(_casters[index].NormalAttack);

            _swapping = false;
            _status = $"시전자: {prefab.name}";
            SelectCaster(index);
        }

        // 적 허수아비 전체를 이 캐릭터로 바꿈
        private IEnumerator SwapEnemies(GameObject prefab)
        {
            _swapping = true;
            StopCast();
            if (!_spawnedEnemies.TryGetValue(prefab, out var set))
            {
                set = new List<StaticAICore>();
                for (var i = 0; i < Mathf.Max(1, _defaultEnemies.Count); i++)
                {
                    var unit = Spawn(prefab, $"적 {i + 1} ({prefab.name})");
                    if (!unit) { _status = $"{prefab.name}: StaticAICore 없음"; _swapping = false; yield break; }
                    set.Add(unit);
                }
                yield return null;
                foreach (var unit in set) Prepare(unit, 1, true);
                _spawnedEnemies[prefab] = set;
            }
            UseEnemies(set);
            _swapping = false;
            _status = $"적: {prefab.name}";
        }

        private void UseEnemies(List<StaticAICore> set)
        {
            foreach (var old in _enemies)
                if (old && !set.Contains(old)) old.gameObject.SetActive(false);
            _enemies = new List<StaticAICore>(set);
            Layout();
        }

        private void DrawCharacters()
        {
            if (_characters.Count == 0) return;
            GUILayout.Label("캐릭터 바꾸기");
            var family = GUILayout.SelectionGrid(_familyIndex, _families, 5);
            if (family != _familyIndex) { _familyIndex = family; _characterIndex = 0; }

            var list = FamilyCharacters();
            if (list.Count == 0) return;
            _characterIndex = Mathf.Clamp(_characterIndex, 0, list.Count - 1);
            _characterIndex = GUILayout.SelectionGrid(_characterIndex, list.Select(CharacterLabel).ToArray(), 2);

            GUI.enabled = !_swapping;
            GUILayout.BeginHorizontal();
            var picked = list[_characterIndex];
            if (GUILayout.Button("시전자로")) StartCoroutine(SwapCaster(picked));
            if (GUILayout.Button("적으로")) StartCoroutine(SwapEnemies(picked));
            if (GUILayout.Button("적 원래대로")) UseEnemies(_defaultEnemies);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        // ---------- 스킬 ----------

        private List<SkillSO> SkillList()
        {
            var list = new List<SkillSO>();
            if (Current == null) return list;
            if (Current.NormalAttack) list.Add(Current.NormalAttack);
            list.AddRange(Current.Skills.Where(s => s));
            list.AddRange(_exampleSkills.Where(s => s));
            return list;
        }

        private SkillSO CurrentSkill
        {
            get
            {
                var list = SkillList();
                return list.Count == 0 ? null : list[Mathf.Clamp(_skillIndex, 0, list.Count - 1)];
            }
        }

        private string SkillLabel(SkillSO s)
        {
            if (!s) return "-";
            if (Current != null && s == Current.NormalAttack) return "평타";
            var label = string.IsNullOrEmpty(s.SkillName) ? s.name : $"{s.name} {s.SkillName}";
            return _exampleSkills.Contains(s) ? $"[예제] {label}" : label;
        }

        private void Cast(bool repeat)
        {
            if (!_ready) return;
            StopCast();
            _repeat = repeat;
            _castRoutine = StartCoroutine(CastRoutine());
        }

        private void StopCast()
        {
            _repeat = false;
            if (_castRoutine != null) StopCoroutine(_castRoutine);
            _castRoutine = null;
            var actor = CurrentActor;
            if (actor && actor.player && actor.gameObject.activeInHierarchy) actor.player.ChangeState(PlayerState.IDLE);
        }

        private IEnumerator CastRoutine()
        {
            do
            {
                var caster = CurrentActor;
                var skill = CurrentSkill;
                if (!caster || !skill) break;

                var target = ResolveTarget(caster, skill);
                if (target && target != caster.transform) caster.LookAt(target.position);
                var started = Time.time;

                // StaticSkillState와 같은 흐름: 공격 모션 재생 후 SO 루틴 실행
                if (caster.player) caster.player.ChangeState(PlayerState.ATTACK, skill.AnimationIndex);
                yield return skill.ExecuteSkillRoutine(caster, target);
                if (caster.player) caster.player.ChangeState(PlayerState.IDLE);

                if (!_repeat) break;
                var wait = _interval - (Time.time - started);
                yield return wait > 0f ? new WaitForSeconds(wait) : null;
            } while (_repeat);

            _castRoutine = null;
        }

        // 스킬 조건으로 대상을 찾고, 못 찾으면 첫 번째 적(아군 스킬은 아군)에게 강제로 사용
        private Transform ResolveTarget(StaticAICore caster, SkillSO skill)
        {
            try
            {
                if (skill.CanExecute(caster, out var found) && found)
                {
                    _status = "";
                    return found;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[전투 테스트] {skill.name} 조건 검사 실패: {e.Message}");
            }

            var fallback = skill.TargetType == SkillSO.TargetingType.Ally
                ? (_ally ? _ally.transform : caster.transform)
                : _enemies.FirstOrDefault(e => e && e.gameObject.activeInHierarchy)?.transform;
            _status = $"{SkillLabel(skill)}: 조건 불충족(사거리 등), {(fallback ? fallback.name : "-")}에게 강제 사용";
            return fallback;
        }

        // ---------- 기록 ----------

        private bool IsOurs(StaticAICore unit) =>
            unit && (unit == _ally || _enemies.Contains(unit) || _casters.Any(c => c.Actor == unit));

        private void OnDamaged(DamageResult r)
        {
            if (!IsOurs(r.Target)) return;

            var source = r.Request.Origin is UnityEngine.Object o && o ? o.name : r.Request.Profile?.Id;
            var text = r.Evaded ? "회피" : r.Blocked ? "막힘" : r.AppliedDamage.ToString();
            if (r.ShieldAbsorbed > 0) text += $" (보호막 {r.ShieldAbsorbed})";
            AddLog($"{Time.time - _logStart:0.00}s  {Name(r.Attacker)} → {Name(r.Target)}  {text}  [{source}]");

            if (!r.Evaded && !r.Blocked)
            {
                _total += r.AppliedDamage;
                _hits++;
                if (_firstHit < 0f) _firstHit = Time.time;
            }

            // 허수아비 적은 체력 자동 회복 (아군은 회복 스킬 확인용으로 그대로)
            if (_autoHeal && _enemies.Contains(r.Target)) r.Target.Health.SetCurrent(r.Target.Health.Max);
        }

        private static string Name(StaticAICore unit) => unit ? unit.name : "-";

        private void AddLog(string line)
        {
            _log.Add(line);
            if (_log.Count > MaxLog) _log.RemoveAt(0);
            _logScroll.y = float.MaxValue;
        }

        private void ClearLog()
        {
            _log.Clear();
            _logStart = Time.time;
            _total = 0;
            _hits = 0;
            _firstHit = -1f;
        }

        // ---------- SO 값 되돌리기 ----------

        private void Backup()
        {
            foreach (var c in _casters)
            {
                BackupOne(c.NormalAttack);
                foreach (var s in c.Skills) BackupOne(s);
            }
            foreach (var s in _exampleSkills) BackupOne(s);
        }

        private void BackupOne(SkillSO skill)
        {
            if (!skill || _backups.ContainsKey(skill)) return;
            var copy = Instantiate(skill);
            copy.hideFlags = HideFlags.HideAndDontSave;
            _backups[skill] = copy;
        }

        private void Restore()
        {
#if UNITY_EDITOR
            foreach (var pair in _backups)
            {
                var original = pair.Key.name;
                UnityEditor.EditorUtility.CopySerialized(pair.Value, pair.Key);
                pair.Key.name = original;
            }
            _status = "SO 값을 플레이 시작 때로 되돌림";
#endif
        }

        // ---------- 시간 ----------

        private void ApplyTime() => Time.timeScale = _paused ? 0f : Speeds[_speedIndex];

        private IEnumerator StepFrame()
        {
            Time.timeScale = Speeds[_speedIndex];
            yield return null;
            if (_paused) Time.timeScale = 0f;
        }

        // ---------- 화면 ----------

        private void OnGUI()
        {
            var scale = Mathf.Clamp(Screen.height / 900f, 1f, 2f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var height = Screen.height / scale;
            var width = Screen.width / scale;

            HandleKeys();

            GUILayout.BeginArea(new Rect(8, 8, 460, height - 16), GUI.skin.box);
            _controlScroll = GUILayout.BeginScrollView(_controlScroll);
            DrawControls();
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(width - 438, 8, 430, Mathf.Min(height - 16, 520)), GUI.skin.box);
            DrawLog();
            GUILayout.EndArea();
        }

        private void HandleKeys()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            switch (e.keyCode)
            {
                case KeyCode.Space: Cast(false); e.Use(); break;
                case KeyCode.R: if (_castRoutine != null && _repeat) StopCast(); else Cast(true); e.Use(); break;
                case KeyCode.P: _paused = !_paused; ApplyTime(); e.Use(); break;
                case KeyCode.Period: if (_paused) StartCoroutine(StepFrame()); e.Use(); break;
            }
        }

        private void DrawControls()
        {
            GUILayout.Label("<b>전투 테스트</b>   Space 사용 / R 반복 / P 정지 / . 한 프레임", Rich());
            if (!_ready)
            {
                GUILayout.Label(string.IsNullOrEmpty(_status) ? "준비 중..." : _status);
                return;
            }

            GUILayout.Label("시전자");
            var names = _casters.Select(c => c.UnitClass.ToString()).ToArray();
            var picked = GUILayout.SelectionGrid(_casterIndex, names, 3);
            if (picked != _casterIndex) SelectCaster(picked);
            if (CurrentActor) GUILayout.Label($"지금 시전자: {CurrentActor.name}");

            GUILayout.Space(4);
            DrawCharacters();

            GUILayout.Label("스킬");
            var skills = SkillList();
            _skillScroll = GUILayout.BeginScrollView(_skillScroll, GUILayout.Height(150));
            var labels = skills.Select(SkillLabel).ToArray();
            var newSkill = GUILayout.SelectionGrid(Mathf.Clamp(_skillIndex, 0, Mathf.Max(0, labels.Length - 1)), labels, 1);
            if (newSkill != _skillIndex) { StopCast(); _skillIndex = newSkill; }
            GUILayout.EndScrollView();

            var skill = CurrentSkill;
            if (skill)
                GUILayout.Label($"선딜 {skill.WindupTime:0.##}  지속 {skill.ActiveTime:0.##}  후딜 {skill.RecoveryTime:0.##}  쿨 {skill.Cooldown:0.##}\n모션 {skill.AnimationIndex}  대상 {skill.TargetType}  생성 {skill.SpawnAt}  최대타수 {skill.MaxHitTargets}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("사용")) Cast(false);
            var repeating = _castRoutine != null && _repeat;
            if (GUILayout.Button(repeating ? "반복 정지" : "반복")) { if (repeating) StopCast(); else Cast(true); }
            if (GUILayout.Button("정지")) StopCast();
            GUILayout.EndHorizontal();
            GUILayout.Label($"반복 간격 {_interval:0.0}초");
            _interval = GUILayout.HorizontalSlider(_interval, 0.3f, 5f);

            GUILayout.Space(6);
            GUILayout.Label("적 수");
            var count = GUILayout.Toolbar(_enemyCount - 1, CountLabels.Take(Mathf.Max(1, _enemies.Count)).ToArray()) + 1;
            var fit = GUILayout.Toggle(_fitRange, $"사거리에 맞춤 (사거리 {Range:0.##})");
            GUILayout.Label($"거리 {_distance:0.00}");
            var distance = GUILayout.HorizontalSlider(_distance, 0.5f, 8f);
            if (!Mathf.Approximately(distance, _distance)) fit = false; // 직접 움직이면 자동 맞춤 끔
            if (fit && !_fitRange) distance = FitDistance();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"적 방향 {_angle:0}°", GUILayout.Width(90));
            var angle = Mathf.Round(GUILayout.HorizontalSlider(_angle, -90f, 90f) / 5f) * 5f;
            if (GUILayout.Button("-45", GUILayout.Width(40))) angle = -45f;
            if (GUILayout.Button("0", GUILayout.Width(30))) angle = 0f;
            if (GUILayout.Button("45", GUILayout.Width(40))) angle = 45f;
            GUILayout.EndHorizontal();
            if (count != _enemyCount || fit != _fitRange || !Mathf.Approximately(distance, _distance) || !Mathf.Approximately(angle, _angle))
            {
                _enemyCount = count;
                _fitRange = fit;
                _distance = distance;
                _angle = angle;
                Layout();
            }
            if (_distance > Range + 0.1f)
                GUILayout.Label("<color=orange>사거리 밖: 실전에서는 이 거리에서 평타를 쓰지 않음</color>", Rich());
            _autoHeal = GUILayout.Toggle(_autoHeal, "적 체력 자동 회복");

            GUILayout.Space(6);
            GUILayout.Label("시간");
            var speed = GUILayout.Toolbar(_speedIndex, SpeedLabels);
            if (speed != _speedIndex) { _speedIndex = speed; ApplyTime(); }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_paused ? "재개" : "일시정지")) { _paused = !_paused; ApplyTime(); }
            GUI.enabled = _paused;
            if (GUILayout.Button("한 프레임")) StartCoroutine(StepFrame());
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Label($"카메라 {_cameraSize:0.0}");
            var cam = GUILayout.HorizontalSlider(_cameraSize, 1f, 6f);
            if (!Mathf.Approximately(cam, _cameraSize)) { _cameraSize = cam; FitCamera(); }

            GUILayout.Space(6);
            DrawUnit("적 1", _enemies.FirstOrDefault());
            DrawUnit("아군", _ally);
            if (_ally && GUILayout.Button("아군 체력 절반으로 (회복 스킬 확인)")) _ally.Health.SetCurrent(_ally.Health.Max / 2);

            GUILayout.Space(6);
            if (GUILayout.Button("SO 값 되돌리기 (플레이 시작 때로)")) Restore();
            if (!string.IsNullOrEmpty(_status)) GUILayout.Label(_status);
        }

        private static void DrawUnit(string label, StaticAICore unit)
        {
            if (!unit) return;
            var state = unit.OverrideMachine?.CurrentState?.GetType().Name ?? "-";
            state = state.Replace("Static", "").Replace("State", "");
            GUILayout.Label($"{label}  HP {unit.Health.Current}/{unit.Health.Max}  상태 {state}");
        }

        private void DrawLog()
        {
            var elapsed = _firstHit < 0f ? 0f : Time.time - _firstHit;
            var dps = elapsed > 0.01f ? _total / elapsed : 0f;
            var avg = _hits > 0 ? (float)_total / _hits : 0f;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"타수 {_hits}  합계 {_total:N0}  평균 {avg:0.#}  DPS {dps:0.#}");
            if (GUILayout.Button("지우기", GUILayout.Width(60))) ClearLog();
            GUILayout.EndHorizontal();

            _logScroll = GUILayout.BeginScrollView(_logScroll);
            foreach (var line in _log) GUILayout.Label(line);
            GUILayout.EndScrollView();
        }

        private static GUIStyle Rich() => new(GUI.skin.label) { richText = true };
    }
}
