using UnityEngine;
using UnityEngine.UI;

public class GetExpButton : MonoBehaviour
{
    [HideInInspector] public Text curLevelText;
    [HideInInspector] public Text curExpText;
    [HideInInspector] public Slider expSlider;
    [HideInInspector] public Text expCostText;

    [HideInInspector] public BuildingUpgradeManager buildingUpgradeManager;

    private int GetDiscountedTrainingCost(int baseCost)
    {
        if (buildingUpgradeManager != null)
            return buildingUpgradeManager.GetDiscountedTrainingCost(baseCost);

        return baseCost;
    }

    public void RefreshSelectedUnitUI()
    {
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogWarning("⚠️ UserManager 또는 user가 준비되지 않아 UI를 갱신할 수 없습니다.");
            return;
        }

        string unitId = UserManager.Instance.selectedUnitId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("⚠️ 선택된 유닛이 없어 UI를 갱신할 수 없습니다.");
            return;
        }

        Unit unit = UserManager.Instance.GetMyUnitById(unitId);
        if (unit == null)
        {
            Debug.LogWarning("⚠️ 선택된 유닛 정보를 찾지 못해 UI를 갱신할 수 없습니다.");
            return;
        }

        if (curLevelText != null) curLevelText.text = unit.level.ToString();
        if (curExpText != null)   curExpText.text   = unit.exp.ToString();

        if (expSlider != null)
        {
            expSlider.maxValue = 100f;
            expSlider.value = unit.exp;
        }

        if (expCostText != null)
        {
            int baseCost = UnitCostCalculator.CalculateGoldCost(unit.level);
            int discountedCost = GetDiscountedTrainingCost(baseCost);
            expCostText.text = $"레벨업 비용 {discountedCost}골드";
        }
    }

    public void OnClickGetExp()
    {
        Debug.Log("🟦 GetEXP 버튼 클릭됨");

        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            return;
        }

        string unitId = UserManager.Instance.selectedUnitId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("⚠️ 선택된 유닛이 없습니다.");
            return;
        }

        Unit unit = UserManager.Instance.GetMyUnitById(unitId);
        if (unit == null)
        {
            Debug.LogError("❌ 선택된 유닛 정보를 찾을 수 없습니다.");
            return;
        }

        int maxLevel = UnitCostCalculator.GetMaxLevelByRarity(unit.rarity);
        if (unit.level >= maxLevel)
        {
            Debug.Log($"{unit.unitName}는 만렙입니다");
            return;
        }

        int baseCost = UnitCostCalculator.CalculateGoldCost(unit.level);
        int requiredGold = GetDiscountedTrainingCost(baseCost);

        bool goldSuccess = UserManager.Instance.user.SpendGold(requiredGold);
        if (!goldSuccess)
        {
            Debug.LogWarning($"⚠️ 골드 부족으로 EXP 획득 실패. 필요 골드: {requiredGold}");
            return;
        }

        bool expSuccess = UserManager.Instance.AddUnitExp(unitId, UnitCostCalculator.EXP_GAIN);
        if (!expSuccess)
        {
            Debug.LogWarning("⚠️ EXP 증가 실패");
            return;
        }

        RefreshSelectedUnitUI();

        Unit refreshedUnit = UserManager.Instance.GetMyUnitById(unitId);
        if (refreshedUnit != null)
        {
            Debug.Log($"✅ EXP 반영 완료: {refreshedUnit.unitName} / Lv.{refreshedUnit.level} / Exp:{refreshedUnit.exp} / 소모 골드:{requiredGold}");
        }
    }
}