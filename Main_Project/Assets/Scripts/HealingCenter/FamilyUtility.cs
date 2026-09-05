using System.Collections.Generic;
using BattleK.Scripts.Data.Stat;
using UnityEngine;

public static class FamilyUtility
{
    public static string GetCurrentFamilyId()
    {
        List<Unit> myUnits = UserManager.Instance.user.myUnits;

        if (myUnits == null || myUnits.Count == 0)
        {
            Debug.LogWarning("[FamilyUtility] 보유한 유닛이 없습니다.");
            return null;
        }

        string selectedUnitId = myUnits[0].Id;

        if (string.IsNullOrEmpty(selectedUnitId))
        {
            Debug.LogWarning("[FamilyUtility] 선택된 유닛(selectedUnitId)이 없습니다.");
            return null;
        }

        CharacterData selectedCharacterData = UnitDataManager.Instance.GetCharacterData(selectedUnitId);

        if (selectedCharacterData == null)
        {
            Debug.LogWarning($"[FamilyUtility] 선택된 유닛의 데이터를 찾을 수 없습니다: {selectedUnitId}");
            return null;
        }

        return selectedCharacterData.Family_ID;
    }
}