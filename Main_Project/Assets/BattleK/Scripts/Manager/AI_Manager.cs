using System.Collections.Generic;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    [Tooltip("index 0 = Player 루트, index 1 = Enemy 루트(선택)")]
    public List<Transform> unitPool = new List<Transform>();

    public List<AICore> playerUnits = new List<AICore>();
    public List<AICore> EnemyUnits  = new List<AICore>();

    void Start()
    {
        // 씬에 미리 배치된 경우에만 의미 있음.
        // BattleStartUsingSlots가 스폰/배치 후 SetUnitList를 다시 호출함.
        if (unitPool.Count > 0) SetUnitList();
    }

    public void SetUnitList()
    {
        playerUnits.Clear();
        EnemyUnits.Clear();

        for (int i = 0; i < unitPool.Count; i++)
        {
            var root = unitPool[i];
            if (root == null) continue;

            foreach (Transform child in root)
            {
                if (child == null || !child.gameObject.activeInHierarchy) continue;

                var core = child.GetComponent<AICore>();
                if (core == null) continue;

                switch (i)
                {
                    case 0: // Player
                        playerUnits.Add(core);
                        child.gameObject.layer = (int)TeamUnit.Player;
                        core.targetLayer = 1 << (int)TeamUnit.Enemy;
                        break;

                    case 1: // Enemy
                        EnemyUnits.Add(core);
                        child.gameObject.layer = (int)TeamUnit.Enemy;
                        core.targetLayer = 1 << (int)TeamUnit.Player;
                        break;
                }
            }
        }

        // Debug.Log($"[AI_Manager] 등록 완료: Player={playerUnits.Count}, Enemy={EnemyUnits.Count}");
    }
}