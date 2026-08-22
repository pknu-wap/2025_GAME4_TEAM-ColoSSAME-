using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeenEnemyListUI : MonoBehaviour
{
    [SerializeField] private Transform content;

    public void ShowTeam(Team team)
    {
        if (team == null)
            return;

        List<SeenEnemyData> enemies =
            EnemySaveManager.Instance.GetSeenEnemiesByTeam(team.fid);

        for (int i = 0; i < content.childCount; i++)
        {
            GameObject enemyUI = content.GetChild(i).gameObject;
            TMP_Text text = enemyUI.GetComponentInChildren<TMP_Text>();

            bool hasEnemy = i < enemies.Count;
            enemyUI.SetActive(hasEnemy);

            if (!hasEnemy)
                continue;

            text.text = enemies[i].unitId;
        }
    }
}