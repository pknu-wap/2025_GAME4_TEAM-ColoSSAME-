using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BattleK.Scripts.Data;
using BattleK.Scripts.Manager;
using BattleK.Scripts.Data.Stat;

public class SeenEnemyListUI : MonoBehaviour
{
    [SerializeField] private Transform content;

    private TMP_Text[] enemyTexts;
    private Image[] enemyImages;
    private Transform[] tierParents;
    private GameObject[] emptyUIs;
    private GameObject[] activeTiers;

    private const int TierParentIndex = 2;
    private const int EnemyObIndex = 3;

    private readonly AddressableAssetLoader<Sprite> portraitLoader 
        = new AddressableAssetLoader<Sprite>();

    private void Awake()
    {
        int count = content.childCount;

        enemyTexts = new TMP_Text[count];
        enemyImages = new Image[count];
        tierParents = new Transform[count];
        emptyUIs = new GameObject[count];
        
        for (int i = 0; i < count; i++)
        {
            Transform enemyUI = content.GetChild(i);

            enemyTexts[i] = enemyUI.GetComponentInChildren<TMP_Text>();
            enemyImages[i] = enemyUI.GetComponentInChildren<Image>(true);
            tierParents[i] = enemyUI.GetChild(TierParentIndex);
            emptyUIs[i] = enemyUI.GetChild(EnemyObIndex).gameObject;

        }
    }    

    public void ShowTeam(Team team)
    {
        if (team == null)
            return;
        
        Team playerTeam = LeagueManager.Instance.league.teams.Find(
            t => t.id == LeagueManager.Instance.league.settings.playerTeamId);

        List<SeenEnemyData> enemies =
            EnemySaveManager.Instance.GetSeenEnemiesByTeam(team.fid);
        
        if (playerTeam != null && team.fid == playerTeam.fid)
        {
            ShowPlayerUnits();
            return;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            Transform tierParent = tierParents[i];


            for (int j = 0; j < tierParent.childCount; j++)
            {
                tierParent.GetChild(j).gameObject.SetActive(false);
            }

            if (i >= enemies.Count)
            {
                enemyImages[i].gameObject.SetActive(false);
                emptyUIs[i].SetActive(true);
                enemyTexts[i].text = "???";
                continue;
            }

            enemyImages[i].gameObject.SetActive(true);
            emptyUIs[i].SetActive(false);

            enemyTexts[i].text = $"Lv.{enemies[i].level}.{enemies[i].unitName}";
            

            int tierIndex = enemies[i].Tier - 1;

            if (tierIndex >= 0 && tierIndex < tierParent.childCount)
            {
                tierParent.GetChild(tierIndex).gameObject.SetActive(true);
            }
        
        }

        StartCoroutine(LoadAllPortraits(enemies));
    }

    private void ShowPlayerUnits()
    {
        List<Unit> units = UserManager.Instance.user.myUnits;

        for (int i = 0; i < content.childCount; i++)
        {
            Transform tierParent = tierParents[i];

            for (int j = 0; j < tierParent.childCount; j++)
            {
                tierParent.GetChild(j).gameObject.SetActive(false);
            }

            if (i >= units.Count)
            {
                enemyImages[i].gameObject.SetActive(false);
                emptyUIs[i].SetActive(true);
                enemyTexts[i].text = "???";
                continue;
            }

            enemyImages[i].gameObject.SetActive(true);
            emptyUIs[i].SetActive(false);

            enemyTexts[i].text = $"Lv{units[i].Level}.{units[i].UnitName}";

            int tierIndex = units[i].Tier - 1;

            if (tierIndex >= 0 && tierIndex < tierParent.childCount)
            {
                tierParent.GetChild(tierIndex).gameObject.SetActive(true);
            }
        }

        StartCoroutine(LoadPlayerPortraits(units));
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

    private IEnumerator LoadPlayerPortraits(List<Unit> units)
    {
        for (int i = 0; i < units.Count && i < content.childCount; i++)
        {
            Image image = enemyImages[i];

            if (image == null)
                continue;

            string unitId = units[i].Id;

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
                () => {}
            );
        }
    }
}