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
    private Dictionary<string, UnitSkill> skillMap;

    public void RefreshUnit()
    {
        StopAllCoroutines();
        StartCoroutine(RefreshRoutine());
    }

    private IEnumerator RefreshRoutine()
    {
        ClearUI();

        string unitId = UserManager.Instance.selectedUnitId;

        currentUnit = UserManager.Instance.GetMyUnitById(unitId);

        BuildSkillMap();

        ShowSkillInfo();

        yield break;
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
        if (currentUnit == null)
            return;

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
                skillTexts[i].text = $"{skill.skillName} Lv.{skill.level}";
            }
        }

    }

    private UnitSkill GetSelectedSkill(int index)
    {
        if (index >= currentUnit.selectedSkills.Count)
            return null;

        string skillName = currentUnit.selectedSkills[index];

        if (skillMap.TryGetValue(skillName, out UnitSkill skill))
        {
            return skill;
        }

        return null;
    }

    private void BuildSkillMap()
    {
        skillMap = new Dictionary<string, UnitSkill>();

        foreach (UnitSkill skill in currentUnit.skills)
        {
            skillMap[skill.skillName] = skill;
        }
    }

    private void ChangeSkill(int rarity, int slotIndex)
    {
        if (currentUnit.rarity < rarity)
            return;

        List<SkillSO> choices =
            randomSkillGrantA.GetSkillChoices(currentUnit.unitClass, rarity);

        skillSelectUI.Show(choices, currentUnit, slotIndex);
    }

    public void ChangeThreeStarSkill()
    {
        ChangeSkill(3, 0);
    }

    public void ChangeFourStarSkill()
    {
        ChangeSkill(4, 1);
    }
}