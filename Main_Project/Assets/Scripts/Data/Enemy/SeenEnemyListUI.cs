using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BattleK.Scripts.Data;
using BattleK.Scripts.Manager;

public class SeenEnemyListUI : MonoBehaviour
{
    [SerializeField] private Transform content;

    private readonly AddressableAssetLoader<Sprite> portraitLoader 
        = new AddressableAssetLoader<Sprite>();

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
            Image image = enemyUI.GetComponentInChildren<Image>(true);
            Transform tierParent = enemyUI.transform.GetChild(2);

            bool hasEnemy = i < enemies.Count;

            text.text = $"{enemies[i].unitName}\n{enemies[i].level}";

            for (int j = 0; j < tierParent.childCount; j++)
            {
                tierParent.GetChild(j).gameObject.SetActive(false);
            }

            int tierIndex = enemies[i].Tier - 1;

            if (tierIndex >= 0 && tierIndex < tierParent.childCount)
            {
                tierParent.GetChild(tierIndex).gameObject.SetActive(true);
            }
        
        }

        StartCoroutine(LoadAllPortraits(enemies));
    }

    private IEnumerator LoadAllPortraits(List<SeenEnemyData> enemies)
    {
        for (int i = 0; i < enemies.Count && i < content.childCount; i++)
        {
            GameObject enemyUI = content.GetChild(i).gameObject;

            Image image = enemyUI.GetComponentInChildren<Image>(true);

            if (image == null)
            {
                continue;
            }

            string unitId = enemies[i].unitId;


            yield return portraitLoader.LoadAsync(
                AddressableAssetType.Character,
                unitId,
                sprite =>
                {
                    if (image != null)
                    {
                        image.sprite = sprite;
                    }
                },
                () =>
                {
                });
        }
    }
}