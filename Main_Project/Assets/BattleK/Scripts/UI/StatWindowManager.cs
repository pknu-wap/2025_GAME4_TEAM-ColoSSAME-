using System.Collections;
using System.Collections.Generic;
using BattleK.Scripts.AI;
using BattleK.Scripts.Manager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class StatWindowManager : MonoBehaviour
    {
        [Header("Stat Elements")]
        public List<AICore> PlayerStats = new();
        public List<AICore> EnemyStats  = new();
        public List<StatWindow> StatWindows = new();

        [Header("StatWindow")]
        public GameObject PlayerWindow;
        public GameObject EnemyWindow;
        [SerializeField] private GameObject _playerRow;
        [SerializeField] private GameObject _enemyRow;
        [SerializeField] private float _firstOffset = 147.5f;
        [SerializeField] private float _rowSpacing  = 270f;

        [SerializeField] private string _playerLayerName;
        [SerializeField] private string _enemyLayerName;

        [SerializeField] private AI_Manager _aiManager;

        public void SetStrategyList()
        {
            PlayerStats = _aiManager ? _aiManager.playerUnits : new List<AICore>();
            EnemyStats  = _aiManager ? _aiManager.enemyUnits  : new List<AICore>();

            ClearChildren(_playerRow);
            ClearChildren(_enemyRow);

            LinkAICore();
        }

        private void LinkAICore()
        {
            SpawnRow(PlayerStats, PlayerWindow, _playerRow, "PLAYER");
            SpawnRow(EnemyStats,  EnemyWindow,  _enemyRow,  "ENEMY");
        }

        private void SpawnRow(List<AICore> list, GameObject prefab, GameObject rowGO, string tag)
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

                var stat = go.GetComponent<StatWindow>();
                stat.OwnerAI = list[i];
                StatWindows.Add(stat);
            }
        }

        public void ApplyStatWindow()
        {
            foreach (var stat in StatWindows)
            {
                stat.Apply();
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
