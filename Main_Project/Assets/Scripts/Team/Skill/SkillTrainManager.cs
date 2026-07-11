using System.Collections;
using UnityEngine;
using TMPro;

public class SkillTrainingManager : MonoBehaviour
{
    public TextMeshProUGUI[] skillTexts;

    private Unit currentUnit;
    private string lastUnitId;

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
        if (currentUnit == null) return;

        if (skillIndex < 0 || skillIndex >= currentUnit.skills.Count)
            return;

        currentUnit.skills[skillIndex].level++;

        UserManager.Instance.SaveUser();

        ShowSkillInfo();
    }

    public void ShowSkillInfo()
    {
        for (int i = 0; i < skillTexts.Length; i++)
        {
            if (currentUnit == null)
            {
                skillTexts[i].text = "";
                continue;
            }

            if (i < currentUnit.skills.Count)
            {
                var skill = currentUnit.skills[i];
                skillTexts[i].text = $"{skill.skillId} Lv.{skill.level}";
            }
            else
            {
                skillTexts[i].text = "";
            }
        }
    }
}