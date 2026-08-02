using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;

public class SkillSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private GameObject unitUI;
    [SerializeField] private SkillSelectButton[] threeStarButtons;
    [SerializeField] private SkillSelectButton[] fourStarButtons;
    [SerializeField] private SkillTrainManager skillTrainingManager;
    [SerializeField] private GameObject threeStarUI;
    [SerializeField] private GameObject fourStarUI;

    private Unit targetUnit;
    private int selectedSlot;

    public void Show(List<SkillSO> skills, Unit unit, int slot)
    {
        targetUnit = unit;
        selectedSlot = slot;

        foreach(GameObject panel in panels)
        {
            panel.SetActive(false);
        }
        unitUI.SetActive(true); 
        threeStarUI.SetActive(false);
        fourStarUI.SetActive(false);

        if(slot == 0)
        {
            threeStarUI.SetActive(true);

            for(int i = 0; i < threeStarButtons.Length; i++)
            {
                threeStarButtons[i].SetSkill(skills[i], this);
            }
        }
        else
        {
            fourStarUI.SetActive(true);

            for(int i = 0; i < fourStarButtons.Length; i++)
            {
                fourStarButtons[i].SetSkill(skills[i], this);
            }
        }

    }

    public void SelectSkill(SkillSO skill)
    {
        while (targetUnit.selectedSkills.Count <= selectedSlot)
        {
            targetUnit.selectedSkills.Add("");
        }

        targetUnit.selectedSkills[selectedSlot] = skill.name;

        UserManager.Instance.SaveUser();

        foreach(GameObject panel in panels)
        {
            panel.SetActive(true);
        }
        unitUI.SetActive(false); 
        threeStarUI.SetActive(false);
        fourStarUI.SetActive(false);

        skillTrainingManager.RefreshUnit();

    }
}