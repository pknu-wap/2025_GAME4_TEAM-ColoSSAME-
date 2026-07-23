using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;

public class AddRarity : MonoBehaviour
{
    [Header("Rarity Objects")]
    [SerializeField] private GameObject[] rarityObjects;
    [SerializeField] private RandomSkillGrantA randomSkillGrantA;
    [SerializeField] private SkillSelectUI skillSelectUI;
    [SerializeField] private SkillTrainManager skillTrainingManager;

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

        
        Unit unit = UserManager.Instance.GetMyUnitById(unitId);


        if ((unit.rarity >= 5) || (unit.level < unit.rarity * 10))
            return;
        
        bool success = UserManager.Instance.AddUnitRarity(unitId, 1);
        if (!success)
            return;

        AddSkillByRarity(unit, unit.rarity);
        
        RefreshSelectedUnitUI();
    }

    private void AddSkillByRarity(Unit unit, int newRarity)
        {
        // 3성, 4성만 선택
        if(newRarity == 3 || newRarity == 4)
        {
            List<SkillSO> choices =
                randomSkillGrantA.GetSkillChoices(unit.unitClass, newRarity);

            foreach (SkillSO skill in choices)
            {
                unit.skills.Add(new UnitSkill(skill.name, 1));
            }
            skillSelectUI.Show(choices, unit, newRarity - 3);
        }


        // 5성은 선택 없음
        else if(newRarity == 5)
        {
            SkillSO ultimate =
                randomSkillGrantA.GetUltimateSkill(unit.unitClass);

            if(ultimate != null)
            {
                unit.skills.Add(
                    new UnitSkill(ultimate.name,1)
                );

                unit.selectedSkills.Add(ultimate.name);

                UserManager.Instance.SaveUser();
                skillTrainingManager.RefreshUnit();
            }
        }
    }
}