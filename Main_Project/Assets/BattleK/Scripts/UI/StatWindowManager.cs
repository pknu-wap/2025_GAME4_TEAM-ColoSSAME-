using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleK.Scripts.UI
{
    public class StatWindowManager : MonoBehaviour
    {
        [Header("Stat Elements")]
        public List<AICore> PlayerStats = new List<AICore>();
        public List<AICore> EnemyStats  = new List<AICore>();
        public List<StatWindow> StatWindows = new List<StatWindow>();

        [Header("StatWindow")]
        public GameObject PlayerWindow;
        public GameObject EnemyWindow;
        [SerializeField] private GameObject PlayerRow;
        [SerializeField] private GameObject EnemyRow;
        [SerializeField] private float firstOffset = 147.5f;
        [SerializeField] private float rowSpacing  = 270f;

        [Header("Layers")]
        [SerializeField] private int _playerLayer = -1;
        [SerializeField] private int _enemyLayer  = -1;

        [SerializeField] private string playerLayerName;
        [SerializeField] private string enemyLayerName;

        [SerializeField] private AI_Manager _aiManager;

        public void SetStrategyList()
        {
            PlayerStats = _aiManager ? _aiManager.playerUnits : new List<AICore>();
            EnemyStats  = _aiManager ? _aiManager.EnemyUnits  : new List<AICore>();

            ClearChildren(PlayerRow);
            ClearChildren(EnemyRow);

            LinkAICore();
        }

        private void LinkAICore()
        {
            SpawnRow(PlayerStats, PlayerWindow, PlayerRow, "PLAYER");
            SpawnRow(EnemyStats,  EnemyWindow,  EnemyRow,  "ENEMY");
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

            for (int i = 0; i < list.Count; i++)
            {
                var go = Instantiate(prefab, rowRT, false);
                if (!go) continue;
                
                float y = -(firstOffset + i * rowSpacing);
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
            for (int i = t.childCount - 1; i >= 0; i--)
                Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}
