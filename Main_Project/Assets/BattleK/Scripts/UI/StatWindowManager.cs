using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Data.ClassInfo;
using BattleK.Scripts.Manager;
using UnityEngine;

namespace BattleK.Scripts.UI
{
    public class StatWindowManager : MonoBehaviour
    {
        [Header("Stat Elements")]
        public List<StaticAICore> PlayerStats = new();
        public List<StaticAICore> EnemyStats  = new();
        public List<StatWindow> StatWindows = new();

        [Header("StatWindow")]
        public GameObject PlayerWindow;
        public GameObject EnemyWindow;
        [SerializeField] private GameObject _playerRow;
        [SerializeField] private GameObject _enemyRow;
        [SerializeField] private float _firstOffset = 147.5f;
        [SerializeField] private float _rowSpacing  = 270f;

        [SerializeField] private AI_Manager _aiManager;

        private void OnEnable()
        {
            UnitStatRepository.OnUnitChanged += HandleUnitChanged;
        }

        private void OnDisable()
        {
            UnitStatRepository.OnUnitChanged -= HandleUnitChanged;
        }

        public void SetStrategyList()
        {
            PlayerStats = _aiManager ? _aiManager.playerUnits : new List<StaticAICore>();
            EnemyStats  = _aiManager ? _aiManager.enemyUnits  : new List<StaticAICore>();

            ClearChildren(_playerRow);
            ClearChildren(_enemyRow);
            StatWindows.Clear();

            LinkAICore();
        }

        private void LinkAICore()
        {
            SpawnRow(PlayerStats, PlayerWindow, _playerRow, "PLAYER");
            SpawnRow(EnemyStats,  EnemyWindow,  _enemyRow,  "ENEMY");
        }

        private void SpawnRow(List<StaticAICore> list, GameObject prefab, GameObject rowGO, string tag)
        {
            if (!rowGO || !prefab || list == null) return;

            var rowRT = rowGO.GetComponent<RectTransform>();
            if (!rowRT)
            {
                Debug.LogError($"[{nameof(StatWindowManager)}] {tag}: Row({rowGO.name})에 RectTransform이 필요합니다.");
                return;
            }

            for (var i = 0; i < list.Count; i++)
            {
                var go = Instantiate(prefab, rowRT, false);
                if (!go) continue;

                var y = -(_firstOffset + i * _rowSpacing);
                var t = go.transform;
                var lp = t.localPosition;
                t.localPosition = new Vector3(lp.x, y, 0f);
                t.localRotation = Quaternion.identity;
                t.localScale    = Vector3.one;

                var statWindow = go.GetComponent<StatWindow>();
                var core = list[i];

                var unitId = core.TryGetComponent(out CharacterID characterId) ? characterId.characterKey : null;

                if (!string.IsNullOrEmpty(unitId) && UnitStatRepository.TryGet(unitId, out var info))
                {
                    statWindow.BindToCore(core, info);
                }
                else
                {
                    statWindow.SetPending(unitId);
                }

                StatWindows.Add(statWindow);
            }
        }

        private void HandleUnitChanged(string unitId, UnitDisplayInfo info)
        {
            foreach (var window in StatWindows)
            {
                if (window.UnitId == unitId)
                {
                    window.SetFromRepository(info);
                }
            }
        }

        private static void ClearChildren(GameObject rowGO)
        {
            if (!rowGO) return;
            var t = rowGO.transform;
            for (var i = t.childCount - 1; i >= 0; i--)
                Destroy(t.GetChild(i).gameObject);
        }
    }
}