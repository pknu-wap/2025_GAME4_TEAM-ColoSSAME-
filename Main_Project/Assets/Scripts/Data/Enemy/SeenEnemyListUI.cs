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

            bool hasEnemy = i < enemies.Count;
            enemyUI.SetActive(hasEnemy);

            if (!hasEnemy)
                continue;

            text.text = enemies[i].unitName;

            if (image != null)
            {
                image.sprite = null;

                string unitId = enemies[i].unitId;

                StartCoroutine(LoadPortraitRoutine(unitId, image));
            }
        }
    }

     private IEnumerator LoadPortraitRoutine(string unitId, Image targetImage)
    {
        yield return portraitLoader.LoadAsync(
            AddressableAssetType.Character,
            unitId,
            sprite =>
            {
                if (targetImage != null)
                {
                    targetImage.sprite = sprite;
                }
            },
            () =>
            {
                Debug.LogWarning($"[SeenEnemyListUI] 적 초상화 로드 실패: {unitId}");
            });
    }

    private void OnDestroy()
    {
        portraitLoader.ReleaseAll();
    }
}