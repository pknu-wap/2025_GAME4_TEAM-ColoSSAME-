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

    private TMP_Text[] enemyTexts;
    private Image[] enemyImages;
    private Transform[] tierParents;
    private const int TierParentIndex = 2;

    private readonly AddressableAssetLoader<Sprite> portraitLoader 
        = new AddressableAssetLoader<Sprite>();

    private void Awake()
    {
        int count = content.childCount;

        enemyTexts = new TMP_Text[count];
        enemyImages = new Image[count];
        tierParents = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            Transform enemyUI = content.GetChild(i);

            enemyTexts[i] = enemyUI.GetComponentInChildren<TMP_Text>();
            enemyImages[i] = enemyUI.GetComponentInChildren<Image>(true);
            tierParents[i] = enemyUI.GetChild(TierParentIndex);
        }
    }    

    public void ShowTeam(Team team)
    {
        if (team == null)
            return;

        List<SeenEnemyData> enemies =
            EnemySaveManager.Instance.GetSeenEnemiesByTeam(team.fid);

        for (int i = 0; i < enemies.Count && i < content.childCount; i++)
        {

            enemyTexts[i].text = $"{enemies[i].unitName}\n{enemies[i].level}";
            
            Transform tierParent = tierParents[i];


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
            Image image = enemyImages[i];
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
                () =>{}
                );
        }
    }
}