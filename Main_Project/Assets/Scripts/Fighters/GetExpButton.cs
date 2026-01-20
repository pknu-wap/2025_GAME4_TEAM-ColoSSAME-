using UnityEngine;
using UnityEngine.UI;

public class GetExpButton : MonoBehaviour
{
    [Header("버튼 클릭 시 획득 경험치")]
    public float expGain = 10f;

    [Header("playerTrain UI")]
    public Text curLevelText;     // Level 텍스트
    public Text curExpText;
    public Slider expSlider;      // CurEXP (Slider)

    public void OnClickGetExp()
    {
        Debug.Log("🟦 GetEXP 버튼 클릭됨");

        // 1) 매니저 체크
        if (UserManager.Instance == null || UserManager.Instance.user == null)
        {
            Debug.LogError("❌ UserManager 또는 user가 준비되지 않았습니다.");
            return;
        }

        // 2) 선택 유닛 확인
        string unitId = UserManager.Instance.selectedUnitId;
        if (string.IsNullOrEmpty(unitId))
        {
            Debug.LogWarning("⚠️ 선택된 유닛이 없습니다.");
            return;
        }

        // 3) EXP 증가
        bool success = UserManager.Instance.AddUnitExp(unitId, expGain);
        if (!success) return;

        // 4) 최신 유닛 정보
        Unit unit = UserManager.Instance.GetMyUnitById(unitId);
        if (unit == null) return;

        // 5) UI 갱신
        if (curLevelText != null)
            curLevelText.text = unit.level.ToString();

        if (curExpText != null)
            curExpText.text = unit.exp.ToString();
        
        if (expSlider != null)
        {
            expSlider.maxValue = 100f; // 레벨당 필요 EXP
            expSlider.value = unit.exp;
        }

        Debug.Log($"✅ EXP 반영: {unit.unitName} Lv.{unit.level} Exp:{unit.exp}");
    }
}