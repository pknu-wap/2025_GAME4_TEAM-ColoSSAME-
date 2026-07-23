using System.Collections.Generic;
using UnityEngine;
using BattleK.Scripts.AI.Skill.Base;

public class SkillSelectUI : MonoBehaviour
{
    public static SkillSelectUI Instance;

    [SerializeField] private GameObject[] panels;
    [SerializeField] private GameObject unitUI;
    [SerializeField] private SkillSelectButton[] buttons;
    [SerializeField] private SkillTrainingManager skillTrainingManager;


    private Unit targetUnit;


    private void Awake()
    {
        Instance = this;
    }


    public void Show(List<SkillSO> skills, Unit unit)
    {
        targetUnit = unit;

        foreach(GameObject panel in panels)
        {
            panel.SetActive(false);
        }
        unitUI.SetActive(true); 


        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetSkill(skills[i], this);
        }
    }


    public void SelectSkill(SkillSO skill)
    {
        targetUnit.skills.Add(
            new UnitSkill(skill.name, 1)
        );

        foreach(GameObject panel in panels)
        {
            panel.SetActive(true);
        }
        unitUI.SetActive(false); 

        skillTrainingManager.RefreshUnit();

    }
}