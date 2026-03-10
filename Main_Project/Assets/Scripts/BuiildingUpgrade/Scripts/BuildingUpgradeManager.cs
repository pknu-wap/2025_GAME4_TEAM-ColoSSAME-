using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시설 업그레이드 로직 담당.
/// - 돈 결제/저장: UserManager.Instance.SpendGold() 사용
/// - 시설 레벨 저장: UserManager.Instance.user.buildingLevels(List)에 기록
/// - LINQ(람다) 제거: Unity C# 컴파일러에서 out 변수 캡처 오류 방지
/// </summary>
public class BuildingUpgradeManager : MonoBehaviour
{
    [Header("업그레이드 테이블 (시설별로 1개씩)")]
    [SerializeField] private BuildingUpgradeTableSO[] tables;

    // 업그레이드 완료 이벤트(상점/훈련소 UI 갱신 등에 사용)
    public event Action<BuildingType, int> OnBuildingUpgraded;

    // 현재 유저(런타임) 참조
    private User CurrentUser => UserManager.Instance != null ? UserManager.Instance.user : null;
    
    //유저의 buildingLevels 리스트를 안전하게 확보하여 반환
    private List<User.BuildingLevelSave> GetOrCreateBuildingLevels()
    {
        User user = CurrentUser;
        if (user == null) return null;
        
        if (user.buildingLevels == null)
            user.buildingLevels = new List<User.BuildingLevelSave>();

        return user.buildingLevels;
    }
    
    // 시설 레벨을 리스트에서 찾아 반환 (없으면 1)
    public int GetCurrentLevel(BuildingType type)
    {
        var list = GetOrCreateBuildingLevels();
        if (list == null) return 1;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].type == type)
                return Mathf.Max(1, list[i].level);
        }

        return 1;
    }

    // 현재 레벨의 할인율(효과)을 반환 (없으면 0)
    // - discountRate는 0~1 범위로 운용 (0.1 = 10%)
    public float GetCurrentDiscountRate(BuildingType type)
    {
        int curLevel = GetCurrentLevel(type);
        var table = GetTable(type);
        if (table == null || table.levels == null) return 0f;

        for (int i = 0; i < table.levels.Count; i++)
        {
            if (table.levels[i].level == curLevel)
            {
                // ✅ 테이블 값이 실수로 1.2, -0.1 같은 값이어도 안전하게 보정
                return Mathf.Clamp01(table.levels[i].discountRate);
            }
        }

        return 0f;
    }
    
    /// <summary>
    /// basePrice에 type 시설의 할인율을 적용한 최종 가격 반환
    /// - basePrice: 원래 가격
    /// - type: 어떤 시설의 할인율을 적용할지 (상점이면 BuildingType.Shop)
    /// - minPrice: 최소 가격 보장(기본 1)
    /// </summary>
    public int GetDiscountedPrice(int basePrice, BuildingType type, int minPrice = 1)
    {
        if (basePrice < 0) basePrice = 0;

        float discount = GetCurrentDiscountRate(type); // 0~1
        float final = basePrice * (1f - discount);

        // 정책: 올림(가격이 0이 되지 않게 / 할인 후 소수점 방지)
        int finalPrice = Mathf.CeilToInt(final);

        if (finalPrice < minPrice)
            finalPrice = minPrice;

        return finalPrice;
    }

    // 상점 전용: 상점 아이템 가격에 상점 할인율 적용
    public int GetDiscountedShopPrice(int basePrice, int minPrice = 1)
    {
        return GetDiscountedPrice(basePrice, BuildingType.Shop, minPrice);
    }

    // 훈련소 전용: 훈련 비용에 훈련소 할인율 적용 (나중에 필요하면 사용)
    public int GetDiscountedTrainingCost(int baseCost, int minCost = 1)
    {
        return GetDiscountedPrice(baseCost, BuildingType.Training, minCost);
    }
    // 다음 업그레이드 정보 반환.
    // 다음 레벨 데이터가 없으면 false(최대 레벨)
    public bool TryGetNextUpgradeInfo(BuildingType type, out int nextLevel, out int costMoney, out float nextDiscountRate)
    {
        int curLevel = GetCurrentLevel(type);

        nextLevel = curLevel + 1;
        costMoney = 0;
        nextDiscountRate = 0f;

        var table = GetTable(type);
        if (table == null || table.levels == null) return false;

        for (int i = 0; i < table.levels.Count; i++)
        {
            if (table.levels[i].level == nextLevel)
            {
                costMoney = table.levels[i].upgradeCostMoney;
                nextDiscountRate = Mathf.Clamp01(table.levels[i].discountRate); // ✅ 보정
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 업그레이드 실행
    /// - 돈 차감: UserManager.Instance.SpendGold()
    /// - 레벨 저장: user.buildingLevels 수정 + SaveUser()
    /// </summary>
    public bool Upgrade(BuildingType type)
    {
        if (UserManager.Instance == null || CurrentUser == null)
        {
            Debug.LogWarning("❌ Upgrade 실패: UserManager 또는 User가 없습니다.");
            return false;
        }

        var list = GetOrCreateBuildingLevels();
        if (list == null)
        {
            Debug.LogWarning("❌ Upgrade 실패: buildingLevels 리스트를 사용할 수 없습니다.");
            return false;
        }

        // 다음 업그레이드 정보 확인
        if (!TryGetNextUpgradeInfo(type, out int nextLevel, out int costMoney, out float nextDiscount))
        {
            Debug.Log($"ℹ️ {type}: 더 이상 업그레이드할 레벨이 없습니다(최대 레벨).");
            return false;
        }

        // 돈 차감(SpendGold 내부에서 SaveUser + OnMoneyChanged까지 처리)
        if (!UserManager.Instance.SpendGold(costMoney))
            return false;

        // 기존 레벨 항목을 찾아 갱신, 없으면 새로 추가 (LINQ 제거)
        bool updated = false;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].type == type)
            {
                list[i].level = nextLevel;
                updated = true;
                break;
            }
        }

        if (!updated)
        {
            list.Add(new User.BuildingLevelSave
            {
                type = type,
                level = nextLevel
            });
        }

        // 시설 레벨 변경 저장
        UserManager.Instance.SaveUser();

        // 이벤트 발행(상점/훈련소 갱신 연결)
        OnBuildingUpgraded?.Invoke(type, nextLevel);

        Debug.Log($"✅ {type} 업그레이드 완료: Lv {nextLevel} / 할인율 {(nextDiscount * 100f):0}%");
        return true;
    }

    // 시설 타입에 맞는 테이블 반환
    private BuildingUpgradeTableSO GetTable(BuildingType type)
    {
        if (tables == null) return null;

        for (int i = 0; i < tables.Length; i++)
        {
            if (tables[i] != null && tables[i].buildingType == type)
                return tables[i];
        }

        return null;
    }
}