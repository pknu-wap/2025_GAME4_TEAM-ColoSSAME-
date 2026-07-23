using UnityEngine;
using TMPro;
using BattleK.Scripts.AI.Skill.Base;

public class SkillSelectButton : MonoBehaviour
{
    [SerializeField] private TMP_Text skillNameText;

    private SkillSO skill;
    private SkillSelectUI skillSelectUI;


    public void SetSkill(SkillSO skill, SkillSelectUI ui)
    {
        this.skill = skill;
        this.skillSelectUI = ui;

        skillNameText.text = skill.name;
    }


    public void OnClick()
    {
        skillSelectUI.SelectSkill(skill);
    }
}