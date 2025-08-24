using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/SkillData")]
public class SkillData : ScriptableObject
{
    public int skillID;
    public string skillName;
    public float cooldown;
    public float range;
    public UnitClass unitClass; // 예: Thief, Warrior 등

    [Header("실행할 SkillLogic 프리팹")]
    public SkillLogic skillLogicPrefab; // 이건 Prefab으로 만들어둬도 되고, Asset에 직접 연결해도 됨
}