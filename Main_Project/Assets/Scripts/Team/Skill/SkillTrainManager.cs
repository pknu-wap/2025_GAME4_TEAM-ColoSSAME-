using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using BattleK.Scripts.AI.Skill.Base;

public class SkillTrainManager : MonoBehaviour
{
    public TextMeshProUGUI[] skillTexts;
    [SerializeField] private RandomSkillGrantA randomSkillGrantA;
    [SerializeField] private SkillSelectUI skillSelectUI;

    private Unit currentUnit;

    public void RefreshUnit()
    {
        StopAllCoroutines();
        StartCoroutine(RefreshRoutine());
    }

    private IEnumerator RefreshRoutine()
    {
        ClearUI();

        yield return null;

        string unitId = UserManager.Instance.selectedUnitId;

        currentUnit = UserManager.Instance.GetMyUnitById(unitId);

        ShowSkillInfo();
    }

    private void ClearUI()
    {
        for (int i = 0; i < skillTexts.Length; i++)
        {
            skillTexts[i].text = "";
        }
    }


    public void UpgradeSkill(int skillIndex)
    {
        UnitSkill skill = GetSelectedSkill(skillIndex);

        if (skill == null)
            return;

        skill.level++;

        UserManager.Instance.SaveUser();

        ShowSkillInfo();
    }

    public void ShowSkillInfo()
    {
         for (int i = 0; i < skillTexts.Length; i++)
        {
            skillTexts[i].text = "";

            UnitSkill skill = GetSelectedSkill(i);

            if (skill != null)
            {
                skillTexts[i].text = $"{skill.skillId} Lv.{skill.level}";
            }
        }

    }

    private UnitSkill GetSelectedSkill(int index)
    {
        if (currentUnit == null)
            return null;

        if (index < 0 || index >= currentUnit.selectedSkills.Count)
            return null;

        string skillName = currentUnit.selectedSkills[index];

        for (int i = 0; i < currentUnit.skills.Count; i++)
        {
            if (currentUnit.skills[i].skillId == skillName)
                return currentUnit.skills[i];
        }

        return null;
    }

    public void ChangeThreeStarSkill()
    {
        if (currentUnit.rarity < 3)
            return;

        List<SkillSO> choices =
            randomSkillGrantA.GetSkillChoices(currentUnit.unitClass, 3);

        skillSelectUI.Show(choices, currentUnit, 0);
    }


    public void ChangeFourStarSkill()
    {
        if (currentUnit.rarity < 4)
            return;

        List<SkillSO> choices =
            randomSkillGrantA.GetSkillChoices(currentUnit.unitClass, 4);

        skillSelectUI.Show(choices, currentUnit, 1);
    }
}