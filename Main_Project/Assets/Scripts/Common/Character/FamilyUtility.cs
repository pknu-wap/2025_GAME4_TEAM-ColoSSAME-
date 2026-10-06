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

        string firstOwnedUnitId = myUnits[0].Id;

        if (string.IsNullOrEmpty(firstOwnedUnitId))
        {
            Debug.LogWarning("[FamilyUtility] 첫 번째 보유 유닛(firstOwnedUnitId)이 없습니다.");
            return null;
        }

        CharacterData firstOwnedCharacterData = UnitDataManager.Instance.GetCharacterData(firstOwnedUnitId);

        if (firstOwnedCharacterData == null)
        {
            Debug.LogWarning($"[FamilyUtility] 첫 번째 보유 유닛의 데이터를 찾을 수 없습니다: {firstOwnedUnitId}");
            return null;
        }

        return firstOwnedCharacterData.Family_ID;
    }
}