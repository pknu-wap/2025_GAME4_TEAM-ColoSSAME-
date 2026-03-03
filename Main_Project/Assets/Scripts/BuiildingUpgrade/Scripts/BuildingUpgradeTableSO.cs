using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    Shop,
    Training
}
[CreateAssetMenu(menuName = "Game/Building Upgrade Table", fileName = "BuildingUpgradeTableSO")]
public class BuildingUpgradeTableSO : ScriptableObject
{
    [Header("이 테이블이 담당하는 시설 타입")]
    public BuildingType buildingType;

    [Header("레벨별 업그레이드 데이터")]
    public List<LevelData> levels = new List<LevelData>();

    [Serializable]
    public class LevelData
    {
        [Tooltip("시설 레벨")]
        public int level = 1;

        [Tooltip("해당 레벨로 업그레이드할 때 드는 비용(골드)")]
        public int upgradeCostMoney = 0;

        [Tooltip("할인율 (0~1). 예: 0.1 = 10% 할인")]
        [Range(0f, 1f)]
        public float discountRate = 0f;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (levels == null)
            levels = new List<LevelData>();

        // 레벨 오름차순 정렬 (데이터 입력 실수 방지)
        levels.Sort((a, b) => a.level.CompareTo(b.level));

        // 레벨이 1 이하로 들어오면 보정(선택)
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].level < 1) levels[i].level = 1;
            if (levels[i].upgradeCostMoney < 0) levels[i].upgradeCostMoney = 0;
            levels[i].discountRate = Mathf.Clamp01(levels[i].discountRate);
        }
    }
#endif
}