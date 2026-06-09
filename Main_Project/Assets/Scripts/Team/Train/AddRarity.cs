using System.Collections;
using UnityEngine;

public class AddRarity : MonoBehaviour
{
    [Header("Rarity Objects")]
    [SerializeField] private GameObject[] rarityObjects;

    private string lastUnitId;

    private void Start()
    {
        RefreshSelectedUnitUI();
    }

    public void RefreshSelectedUnitUI()
    {
        StopAllCoroutines();
        StartCoroutine(RefreshRoutine());
    }

    private IEnumerator RefreshRoutine()
    {
        // 먼저 전부 끄기
        HideAllRarityObjects();

        // 한 프레임 대기
        yield return null;

        if (UserManager.Instance == null || UserManager.Instance.user == null)
            yield break;

        string unitId = UserManager.Instance.selectedUnitId;

        if (string.IsNullOrEmpty(unitId))
            yield break;

        // 같은 유닛이면 다시 안 그림
        if (unitId == lastUnitId)
            yield break;

        lastUnitId = unitId;

        Unit unit = UserManager.Instance.GetMyUnitById(unitId);

        if (unit == null)
            yield break;

        RefreshRarityObject(unit.rarity);
    }

    private void HideAllRarityObjects()
    {
        for (int i = 0; i < rarityObjects.Length; i++)
        {
            if (rarityObjects[i] != null)
                rarityObjects[i].SetActive(false);
        }
    }

    private void RefreshRarityObject(int rarity)
    {
        int index = rarity - 1;

        if (index >= 0 && index < rarityObjects.Length)
        {
            if (rarityObjects[index] != null)
                rarityObjects[index].SetActive(true);
        }
    }

    public void OnClickUpgradeRarity()
    {
        if (UserManager.Instance == null)
            return;

        string unitId = UserManager.Instance.selectedUnitId;

        if (string.IsNullOrEmpty(unitId))
            return;

        bool success = UserManager.Instance.AddUnitRarity(unitId, 1);

        if (!success)
            return;

        lastUnitId = "";

        RefreshSelectedUnitUI();
    }
}