using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;
using TMPro;

public class AddRarity : MonoBehaviour
{
    [Header("Rarity Objects")]
    [SerializeField] private GameObject[] rarityObjects;
    [SerializeField] private RandomSkillGrantA randomSkillGrantA;
    [SerializeField] private SkillSelectUI skillSelectUI;
    [SerializeField] private SkillTrainManager skillTrainingManager;

    [SerializeField] private TextMeshProUGUI successRateText;

    [SerializeField] private float[] baseSuccessRate =
    {
        90f, 
        80f,  
        60f,  
        50f   
    };

    [SerializeField] private int increaseAmount = 5;

    private void Start()
    {
        RefreshSelectedUnitUI();
    }

    public void RefreshSelectedUnitUI()
    {
        StartCoroutine(RefreshRoutine());
    }

    private IEnumerator RefreshRoutine()
    {
        // 먼저 전부 끄기
        HideAllRarityObjects();

        if (UserManager.Instance == null || UserManager.Instance.user == null)
            yield break;

        string unitId = UserManager.Instance.selectedUnitId;

        if (string.IsNullOrEmpty(unitId))
            yield break;

        

        Unit unit = UserManager.Instance.GetMyUnitById(unitId);

        if (unit == null)
            yield break;

        RefreshRarityObject(unit.rarity);
        UpdateSuccessRateUI(unit);
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

    private float GetSuccessRate(Unit unit)
    {
        int index = unit.rarity - 1;

        if (index < 0 || index >= baseSuccessRate.Length)
            return 100f;

        return Mathf.Clamp(baseSuccessRate[index] + unit.bonusSuccessRarity, 0f, 100f);
    }

    private void UpdateSuccessRateUI(Unit unit)
    {
        if (successRateText == null)
            return;

        successRateText.text = $"성공 확률 : {GetSuccessRate(unit):0}%\n실패(유지) 확률 : {100 - GetSuccessRate(unit):0}%";
    }

    public void RaritySuccessUp()
    {
         Unit unit = UserManager.Instance.GetMyUnitById(UserManager.Instance.selectedUnitId);

        if (GetSuccessRate(unit) >= 100f)
            return;

        unit.bonusSuccessRarity += increaseAmount;

        UserManager.Instance.SaveUser();
        UpdateSuccessRateUI(unit);
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
        
        float rate = GetSuccessRate(unit);

        if (Random.Range(0f, 100f) >= rate)
        {
            Debug.Log("강화 실패");
            unit.bonusSuccessRarity = 0;
            return;
        }

        bool success = UserManager.Instance.AddUnitRarity(unitId, 1);

        if (!success)
            return;

        unit.bonusSuccessRarity = 0;

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
            
            UserManager.Instance.SaveUser();

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