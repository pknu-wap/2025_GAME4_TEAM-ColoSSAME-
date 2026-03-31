using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillTrainingManager : MonoBehaviour
{
    public TextMeshProUGUI[] skillTexts; // 스킬 텍스트 배열

    Unit currentUnit;

    void Start()
    {
        string unitId = UserManager.Instance.selectedUnitId;
        currentUnit = UserManager.Instance.GetMyUnitById(unitId);

        if (currentUnit != null)
        {
            ShowSkillInfo();
        }
    }

    public void UpgradeSkill(int skillIndex)
    {
        if (currentUnit == null) return;

        if (skillIndex < 0 || skillIndex >= currentUnit.skills.Count)
        {
            Debug.LogError("스킬 인덱스 오류");
            return;
        }

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
                skillTexts[i].text = $"{skill.skillId}  Lv.{skill.level}";
            }
            else
            {
                skillTexts[i].text = "스킬 없음";
            }
        }
        

    }
}