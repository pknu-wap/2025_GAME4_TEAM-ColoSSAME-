using UnityEngine;
using UnityEngine.UI;

public class GetExpButton : MonoBehaviour
{
    // FighterNameBinder에서 주입받음 - Inspector 직접 연결 불필요
    [HideInInspector] public Text curLevelText;
    [HideInInspector] public Text curExpText;
    [HideInInspector] public Slider expSlider;
    [HideInInspector] public Text expCostText;

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

        int requiredGold = UnitCostCalculator.CalculateGoldCost(unit.level);

        bool goldSuccess = UserManager.Instance.user.SpendGold(requiredGold);
        if (!goldSuccess)
        {
            Debug.LogWarning($"⚠️ 골드 부족으로 EXP 획득 실패. 필요 골드: {requiredGold}");
            return;
        }

        // ← expGain 대신 상수 참조
        bool expSuccess = UserManager.Instance.AddUnitExp(unitId, UnitCostCalculator.EXP_GAIN);
        if (!expSuccess)
        {
            Debug.LogWarning("⚠️ EXP 증가 실패");
            return;
        }

        unit = UserManager.Instance.GetMyUnitById(unitId);
        if (unit == null)
        {
            Debug.LogError("❌ EXP 적용 후 유닛 정보를 다시 불러오지 못했습니다.");
            return;
        }

        if (curLevelText != null) curLevelText.text = unit.level.ToString();
        if (curExpText != null)   curExpText.text   = unit.exp.ToString();
        if (expSlider != null)
        {
            expSlider.maxValue = 100f;
            expSlider.value    = unit.exp;
        }

        if (expCostText != null)
        {
            int newCost = UnitCostCalculator.CalculateGoldCost(unit.level);
            expCostText.text = $"레벨업 비용 {newCost}골드";
        }

        Debug.Log($"✅ EXP 반영 완료: {unit.unitName} / Lv.{unit.level} / Exp:{unit.exp} / 소모 골드:{requiredGold}");
    }
}