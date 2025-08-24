using UnityEngine;

public static class TeamLayerBinder
{
    /// <summary>
    /// root 이하의 모든 RangedAttack을 순회하며 즉시 재바인딩(force 옵션 선택 가능).
    /// 프리팹 스폰 직후 호출 추천.
    /// </summary>
    public static void Bind(GameObject root, bool force = false)
    {
        if (root == null) return;
        var rgs = root.GetComponentsInChildren<RangedAttack>(includeInactive: true);
        foreach (var rg in rgs)
        {
            rg.RebindNow(force);
        }
    }
}