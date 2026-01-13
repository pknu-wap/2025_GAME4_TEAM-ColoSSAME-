// AI_Manager.cs

using System;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using UnityEngine;

namespace BattleK.Scripts.Manager
{
    public class AI_Manager : MonoBehaviour
    {
        public static AI_Manager Instance { get; private set; }
        public static event Action<AI_Manager> OnReady;

        [Tooltip("index 0 = Player 루트, index 1 = Enemy 루트(선택)")]
        public List<Transform> unitPool = new();

        [Header("수집된 유닛 리스트(읽기전용 성격)")]
        public List<AICore> playerUnits = new();
        public List<AICore> enemyUnits  = new();

        [Header("레이어 설정")]
        [Tooltip("Project Settings > Tags and Layers 에서 만든 레이어 이름")]
        public string playerLayerName = "Player";
        public string enemyLayerName  = "Enemy";

        [Tooltip("유닛 오브젝트의 모든 하위 자식까지 레이어를 일괄 적용할지")]
        public bool applyLayerRecursively = true;

        [Header("팀/레이어 연동 옵션")]
        [Tooltip("사이드(0=Player,1=Enemy)에 맞춰 레이어를 강제로 통일할지 여부")]
        public bool forceLayerBySide = true;

        [Tooltip("AICore.targetLayer를 GameObject.layer를 기준으로 자동 산출할지(권장)")]
        public bool setTargetByLayer = true;
    
        [Header("ResultManager")]
        [SerializeField] private LeagueSceneManager leagueSceneManager;
        
        private int _playerLayer = -1;
        private int _enemyLayer  = -1;
        
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            OnReady?.Invoke(this);
        }

        private void Start()
        {
            if (unitPool.Count > 0) SetUnitList();
        }

        private void SetUnitList()
        {
            playerUnits.Clear();
            enemyUnits.Clear();

            ResolveLayers();

            for (var side = 0; side < unitPool.Count; side++)
            {
                var root = unitPool[side];
                if (!root) continue;

                var cores = root.GetComponentsInChildren<AICore>(includeInactive: false);
                foreach (var core in cores)
                {
                    if (!core || !core.gameObject.activeInHierarchy) continue;

                    var go = core.gameObject;

                    if (forceLayerBySide)
                    {
                        var wantLayer = (side == 0) ? _playerLayer : _enemyLayer;
                        AssignLayer(go, wantLayer);
                    }

                    var targetMask = ComputeTargetMaskFrom(go.layer, side);
                    core.targetLayer = targetMask;

                    switch (side)
                    {
                        case 0:
                        {
                            if (!playerUnits.Contains(core)) playerUnits.Add(core);
                            break;
                        }
                        case 1:
                        {
                            if (!enemyUnits.Contains(core)) enemyUnits.Add(core);
                            break;
                        }
                    }
                }
            }
        }
        public void TryRegister(AICore core, bool guessSideByLayer = true)
        {
            if (core == null) return;

            if (_playerLayer < 0 || _enemyLayer < 0) ResolveLayers();

            var side = 0;
            if (guessSideByLayer)
            {
                if (core.gameObject.layer == _enemyLayer) side = 1;
            }

            if (forceLayerBySide)
            {
                AssignLayer(core.gameObject, side == 0 ? _playerLayer : _enemyLayer);
            }

            var targetMask = ComputeTargetMaskFrom(core.gameObject.layer, side);
            core.targetLayer = targetMask;

            var list = (side == 0) ? playerUnits : enemyUnits;
            if (!list.Contains(core)) list.Add(core);
        }
        
        public void Unregister(AICore core)
        {
            if (core == null) return;
            playerUnits.Remove(core);
            enemyUnits.Remove(core);
        }
        
        private void ResolveLayers()
        {
            _playerLayer = LayerMask.NameToLayer(playerLayerName);
            _enemyLayer  = LayerMask.NameToLayer(enemyLayerName);

            if (_playerLayer < 0)
            {
                Debug.LogWarning($"[AI_Manager] '{playerLayerName}' 레이어를 찾을 수 없습니다. Default(0) 사용.");
                _playerLayer = 0;
            }

            if (_enemyLayer >= 0) return;
            Debug.LogWarning($"[AI_Manager] '{enemyLayerName}' 레이어를 찾을 수 없습니다. Default(0) 사용.");
            _enemyLayer = 0;
        }
        
        private int ComputeTargetMaskFrom(int selfLayerIndex, int sideIndex)
        {
            int targetLayerIndex;

            if (setTargetByLayer)
            {
                if (selfLayerIndex == _playerLayer)      targetLayerIndex = _enemyLayer;
                else if (selfLayerIndex == _enemyLayer)  targetLayerIndex = _playerLayer;
                else                                     targetLayerIndex = (sideIndex == 0) ? _enemyLayer : _playerLayer;
            }
            else
            {
                targetLayerIndex = (sideIndex == 0) ? _enemyLayer : _playerLayer;
            }

            return 1 << targetLayerIndex;
        }
        
        private void AssignLayer(GameObject go, int layerIndex)
        {
            if (go == null) return;
            if (applyLayerRecursively)
                SetLayerRecursively(go.transform, layerIndex);
            else
                go.layer = layerIndex;
        }

        private void SetLayerRecursively(Transform t, int layerIndex)
        {
            t.gameObject.layer = layerIndex;
            foreach (Transform c in t)
                SetLayerRecursively(c, layerIndex);
        }

        public void IsWinner()
        {
            if (playerUnits.Count < 1)
            {
                leagueSceneManager.OnClickLose();
            }

            if (enemyUnits.Count < 1)
            {
                leagueSceneManager.OnClickWin();
            }
        }

        public void KillAll()
        {
            foreach (var player in playerUnits)
            {
                player.Kill();
            }
            foreach (var enemy in enemyUnits)
            {
                enemy.Kill();
            }
        }
    }
}
