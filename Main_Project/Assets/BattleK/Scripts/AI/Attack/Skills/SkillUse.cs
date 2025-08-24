using UnityEngine;

public class SkillUse : MonoBehaviour
{
    public void UseSkill(SkillData skillData, AICore user, Transform target)
    {
        if (skillData == null || skillData.skillLogicPrefab == null)
        {
            Debug.LogError("SkillData 또는 SkillLogicPrefab이 null입니다.");
            return;
        }

        SkillLogic logic = Instantiate(skillData.skillLogicPrefab, user.transform);
        logic.Execute(user, target);
    }
}